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
- **Afina.Infrastructure**: Handles cross-cutting concerns (structured logging, serialization, etc.).
- **Afina.Contracts**: Defines public interfaces and contracts exposed by Afina services.
- **Afina.Data**: A shared library containing all Entity Framework Core entities and migrations.

### Module (Vertical Slice) Structure

Each module (e.g., `Afina.Modules.Users`) is organized around a `Features` root folder. We implement Vertical Slice Architecture at the feature level, with a flattened layout: every feature is a single folder with individual files for requests, responses, endpoints, and handlers. Shared, cross-feature code is centralized under `Shared/`.

```
Afina.Modules.<ModuleName>/
  Features/
    <FeatureName>/
      <FeatureName>Request.cs
      <FeatureName>Response.cs
      <FeatureName>Endpoint.cs
      <FeatureName>Handler.cs
  Shared/
    Persistence/
      ... repositories used across features ...
    Services/
      ... services used across features ...
```

**Example: Users Module Structure (Flattened with Separate Request/Response Files)**

```
Afina.Modules.Users/
  Features/
    Login/
      LoginRequest.cs
      LoginResponse.cs
      LoginEndpoint.cs
      LoginHandler.cs
    Register/
      RegisterRequest.cs
      RegisterResponse.cs
      RegisterEndpoint.cs
      RegisterHandler.cs
    RefreshToken/
      RefreshTokenRequest.cs
      RefreshTokenResponse.cs
      RefreshTokenEndpoint.cs
      RefreshTokenHandler.cs
    GetCurrentUser/
      GetCurrentUserRequest.cs
      GetCurrentUserResponse.cs
      GetCurrentUserEndpoint.cs
      GetCurrentUserHandler.cs
    DeleteUser/
      DeleteUserRequest.cs
      DeleteUserEndpoint.cs
      DeleteUserHandler.cs
    ExportUserData/
      ExportUserDataRequest.cs
      ExportUserDataResponse.cs
      ExportUserDataEndpoint.cs
      ExportUserDataHandler.cs
    ListApiKeys/
      ListApiKeysRequest.cs
      ListApiKeysResponse.cs
      ListApiKeysEndpoint.cs
      ListApiKeysHandler.cs
    CreateApiKey/
      CreateApiKeyRequest.cs
      CreateApiKeyResponse.cs
      CreateApiKeyEndpoint.cs
      CreateApiKeyHandler.cs
    DeleteApiKey/
      DeleteApiKeyRequest.cs
      DeleteApiKeyEndpoint.cs
      DeleteApiKeyHandler.cs

  Shared/
    Persistence/
      IUserRepository.cs, UsersRepository.cs
      IUserSessionsRepository.cs, UserSessionsRepository.cs
      IApiKeyRepository.cs, ApiKeyRepository.cs
    Services/
      ITokenService.cs, JwtTokenService.cs, TokenService.cs
```

**Guidelines:**

- **Feature Isolation**: Each feature folder contains everything needed for its use case - separate files for Request, Response, Endpoint, and Handler.
- **Request/Response Separation**: Request and Response classes are extracted into separate files for better readability and maintainability.
- **Minimize Shared Code**: Only extract to `Shared/` when truly cross-feature (repositories, services, etc.).
- **Endpoints**: Bind request and delegate to `_mediator.CallAsync(request)`.
- **Handlers**: Own application logic, use repositories, and return responses.
- **Data Access**: Repositories work on `Afina.Data` entities.
- **Cross-Module Communication**: Use the mediator abstraction.
- **Encapsulation**: Do not leak internal feature types outside the module.

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
# Architecture Overview

This document provides a high-level overview of Afina's architecture and how Vertical Slices are organized, with emphasis on the updated structure for the Users module.

## Vertical Slice Architecture (VSA)

We have migrated from feature-group folders (e.g., `Authentication/`, `UserProfile/`, `ApiKeys/` each with `Endpoints/` and `Handlers/` subfolders) to a flattened, per-feature layout. Each feature now lives in a single folder with separate files for Request, Response, Endpoint, and Handler. Cross-feature code is centralized under `Shared/`.

- Per-feature folders: `apps/api/Afina.Modules.Users/Features/{FeatureName}/`
- Typical contents: `{FeatureName}Request.cs`, `{FeatureName}Response.cs`, `{FeatureName}Endpoint.cs`, `{FeatureName}Handler.cs`
- Cross-feature code: `apps/api/Afina.Modules.Users/Shared/` (Persistence and Services)
- `Program.cs`: Maps endpoints using flattened namespaces and configures DI to use Shared repositories and services

### Example: Users Module Structure

`apps/api/Afina.Modules.Users/`

- `Features/`
    - `Login/` → `LoginRequest.cs`, `LoginResponse.cs`, `LoginEndpoint.cs`, `LoginHandler.cs`
    - `Register/` → `RegisterRequest.cs`, `RegisterResponse.cs`, `RegisterEndpoint.cs`, `RegisterHandler.cs`
    - `RefreshToken/` → `RefreshTokenRequest.cs`, `RefreshTokenResponse.cs`, `RefreshTokenEndpoint.cs`, `RefreshTokenHandler.cs`
    - `GetCurrentUser/` → `GetCurrentUserRequest.cs`, `GetCurrentUserResponse.cs`, `GetCurrentUserEndpoint.cs`, `GetCurrentUserHandler.cs`
    - `DeleteUser/` → `DeleteUserRequest.cs`, `DeleteUserEndpoint.cs`, `DeleteUserHandler.cs`
    - `ExportUserData/` → `ExportUserDataRequest.cs`, `ExportUserDataResponse.cs`, `ExportUserDataEndpoint.cs`, `ExportUserDataHandler.cs`
    - `ListApiKeys/` → `ListApiKeysRequest.cs`, `ListApiKeysResponse.cs`, `ListApiKeysEndpoint.cs`, `ListApiKeysHandler.cs`
    - `CreateApiKey/` → `CreateApiKeyRequest.cs`, `CreateApiKeyResponse.cs`, `CreateApiKeyEndpoint.cs`, `CreateApiKeyHandler.cs`
    - `DeleteApiKey/` → `DeleteApiKeyRequest.cs`, `DeleteApiKeyEndpoint.cs`, `DeleteApiKeyHandler.cs`

- `Shared/`
    - `Persistence/`
        - `IUserRepository.cs`, `UsersRepository.cs`
        - `IUserSessionsRepository.cs`, `UserSessionsRepository.cs`
        - `IApiKeyRepository.cs`, `ApiKeyRepository.cs`
    - `Services/`
        - `ITokenService.cs`, `JwtTokenService.cs`, `TokenService.cs`

### Rationale

- **Clarity**: Feature code sits together with clear separation of concerns (Request, Response, Endpoint, Handler).
- **Readability**: Extracting Request and Response into separate files improves code navigation and understanding.
- **Isolation**: Slices are self-contained; shared persistence and services are explicit.
- **Maintainability**: Individual files for each concern make features easier to modify and test.
- **Less boilerplate**: Removes redundant nested folder structures; faster navigation and maintenance.

## Backend Components

- API: .NET minimal APIs organized by modules (Users, Identity, Tenant, Vault).
- Infrastructure: Persistence, mediator, and shared services via DI.
- Modules: Each module owns its features; cross-feature persistence/contracts live in `Shared/`.

## Frontend Components

- Web: Vite + TypeScript SPA with feature-oriented pages and services.
- Integration: Communicates with endpoints mapped per feature.

## Cross-Cutting Concerns

- Security: Authentication, authorization, and encryption modules.
- Logging: Serilog with Grafana + Loki integration.
- Deployment: Docker Compose for API and web; Nginx serves static assets.
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
