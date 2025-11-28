# Architecture Overview

## Introduction

The Private Data Manager platform is designed as a secure, zero-knowledge, multi-tenant system for storing private data. It follows a modular monolith architecture to ensure maintainability, scalability, and ease of deployment.

## High-Level Architecture

The system consists of the following main components:

1.  **Client Layer**:
    - **Web Application**: A Single Page Application (SPA) built with React, Vite, Styled-Components, and Tailwind CSS.
    - **Mobile Apps**: (Future scope) Native or cross-platform apps.
2.  **API Gateway / Load Balancer**: Entry point for all client requests (Nginx or similar in production, direct Kestrel in dev).
3.  **Application Server (Modular Monolith)**:
    - Built with .NET Core (Latest LTS).
    - Hosts REST APIs.
    - Implements business logic, authentication, and authorization.
    - Modules: Identity, Tenant Management, Private Data, Audit Logs.
4.  **Encryption Service (Pluggable)**:
    - **Role**: Handles all encryption/decryption and key management.
    - **Default Implementation**: Embedded within the Modular Monolith (as a module) but logically distinct.
    - **External Implementation**: Tenants can override this with a URL to an external service.
5.  **Data Layer**:
    - **Database**: PostgreSQL accessed via EF Core.
    - **Storage**: File storage for documents/media (Local filesystem or S3-compatible).

## High-Level Architecture

The Afina API follows a **Modular Monolith** architecture with **Vertical Slices**. Each module itself is a vertical slice (e.g., Users, Tenant, Vault, Audit, Encryption). A slice encapsulates everything required for its behavior: endpoints, handlers, validation, domain logic, orchestration services, and persistence access.

## Project Structure

### Core & Infrastructure

- **Afina.Core**: Contains common business logic shared between modules.
- **Afina.Infrastructure**: Handles cross-cutting concerns (logging, serialization, etc.) not directly related to business logic.
- **Afina.Contracts**: Defines public interfaces and contracts exposed by Afina services.
- **Afina.Data**: A shared library containing all Entity Framework Core entities and migrations.

### Module (Vertical Slice) Structure

Each module (e.g., `Afina.Modules.Users`) is fully self-contained and organized around a `Features` root folder implementing the vertical slice pattern. Concepts are separated by concern, not by layered abstractions.

```
Afina.Modules.<SliceName>/
    Features/
        Endpoints/        # Minimal API endpoint classes; thin – delegate to handlers.
        Handlers/         # Request handlers (business use cases) implementing IRequest<T> pattern.
        Validators/       # Fluent validation or custom validators bound to handler/request types.
        Domains/          # Rich domain models, value objects, domain services (pure business logic).
        Services/         # Orchestration, cross-cutting integration (e.g., external APIs, encryption calls).
        Persistence/      # Repository abstractions or query objects – using entities & DbContext from Afina.Data.
```

Guidelines:

- Endpoints perform request binding and immediately call `_mediator.CallAsync(request)`.
- Handlers contain application logic; they depend on domain models and repositories.
- Repositories operate only on `Afina.Data` entities; mapping to domain models occurs in the handler or persistence layer.
- Cross-module calls MUST use the mediator abstraction; avoid direct service references that cause circular dependencies.
- No leaking of internal slice types outside the module – only contracts explicitly published through `Afina.Contracts` for third-party or external integrations.

### Data Layer

- **Afina.Data**:
  - **Entities**: All database entities. Must inherit from `EntityBase`.
  - **Migrations**: EF Core migrations.
- **Module Persistence**: Each slice defines repositories/query helpers in `Persistence/` that depend on the shared DbContext and entities but never redefine them.

### Mediator Design

To support cross-slice communication without circular dependencies, a lightweight custom mediator lives in `Afina.Infrastructure`.

Key Interfaces:

```csharp
public interface IRequest<TResponse> { }
public interface IRequestHandler<TRequest,TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken ct);
}
public interface IMediator
{
    Task<TResponse> CallAsync<TResponse>(IRequest<TResponse> request, CancellationToken ct = default);
}
```

Registration:

```csharp
// In Program.cs
builder.Services.AddMediator();
```

Usage in an Endpoint:

```csharp
public class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
        => app.MapPost("/api/v1/auth/login", HandleAsync);

    private static async Task<IResult> HandleAsync(LoginRequest request, IMediator mediator, CancellationToken ct)
    {
        var response = await mediator.CallAsync(request, ct);
        return Results.Ok(response);
    }
}
```

Handler Example:

```csharp
public sealed class LoginRequest : IRequest<LoginResponse>
{
    public string Username { get; init; } = string.Empty;
    public string AuthHash { get; init; } = string.Empty;
}

public sealed class LoginHandler : IRequestHandler<LoginRequest, LoginResponse>
{
    private readonly IUserRepository _users;
    private readonly ITokenService _tokens;
    public LoginHandler(IUserRepository users, ITokenService tokens)
        => (_users, _tokens) = (users, tokens);

    public async Task<LoginResponse> HandleAsync(LoginRequest request, CancellationToken ct)
    {
        var user = await _users.GetByUsernameAsync(request.Username, ct) ?? throw new UnauthorizedAccessException();
        // verify hash, issue tokens
        return _tokens.CreateLoginResponse(user);
    }
}
```

Benefits:

- Decoupled cross-module invocation.
- Explicit request/response contracts per use case.
- Easy extension with pipeline behaviors (validation, audit logging) later.

## Technology Stack

- **Framework**: .NET 8 (ASP.NET Core)
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core
- **API**: Minimal APIs

* **Frontend**: React, TypeScript, Vite, Styled-Components, Tailwind CSS.
* **Containerization**: Docker, Docker Compose.
* **Testing**: xUnit, Testcontainers.

## Zero-Knowledge / Pluggable Encryption Architecture

- **Server-Side Encryption**: Data is encrypted by a dedicated Encryption Service before being stored.
- **Pluggable**: Tenants can define their own Encryption Service URL.
- **Key Management**: Keys are managed by the Encryption Service, not the main application.
- **Versioning & Rotation**: Data includes metadata about the key version and algorithm used.

## Diagram

```mermaid
graph TD
    Client[Web Client / Mobile App] -->|HTTPS/REST| API[API Gateway / Backend]
    subgraph "Modular Monolith Backend"
        Identity[Identity Module]
        Tenant[Tenant Module]
        Vault[Vault Module]
        Audit[Audit Module]
    end

    subgraph "Encryption Layer"
        ES[Encryption Service (Default or Custom)]
    end

    API --> Identity
    API --> Tenant
    API --> Vault
    API --> Audit

    Vault -->|Plain Text| ES
    ES -->|Encrypted Data| Vault

    Identity --> DB[(PostgreSQL)]
    Tenant --> DB
    Vault --> DB
    Audit --> DB
```
