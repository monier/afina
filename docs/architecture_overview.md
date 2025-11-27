# Architecture Overview

## Introduction
The Private Data Manager platform is designed as a secure, zero-knowledge, multi-tenant system for storing private data. It follows a modular monolith architecture to ensure maintainability, scalability, and ease of deployment.

## High-Level Architecture

The system consists of the following main components:

1.  **Client Layer**:
    *   **Web Application**: A Single Page Application (SPA) built with React, Vite, Styled-Components, and Tailwind CSS.
    *   **Mobile Apps**: (Future scope) Native or cross-platform apps.
2.  **API Gateway / Load Balancer**: Entry point for all client requests (Nginx or similar in production, direct Kestrel in dev).
3.  **Application Server (Modular Monolith)**:
    *   Built with .NET Core (Latest LTS).
    *   Hosts REST APIs.
    *   Implements business logic, authentication, and authorization.
    *   Modules: Identity, Tenant Management, Private Data, Audit Logs.
4.  **Encryption Service (Pluggable)**:
    *   **Role**: Handles all encryption/decryption and key management.
    *   **Default Implementation**: Embedded within the Modular Monolith (as a module) but logically distinct.
    *   **External Implementation**: Tenants can override this with a URL to an external service.
5.  **Data Layer**:
    *   **Database**: PostgreSQL accessed via EF Core.
    *   **Storage**: File storage for documents/media (Local filesystem or S3-compatible).

## High-Level Architecture
The Afina API follows a **Modular Monolith** architecture with **Vertical Slices**. Each module is self-contained and represents a specific business domain (e.g., Identity, Tenant, Vault).

## Project Structure

### Core & Infrastructure
- **Afina.Core**: Contains common business logic shared between modules.
- **Afina.Infrastructure**: Handles cross-cutting concerns (logging, serialization, etc.) not directly related to business logic.
- **Afina.Contracts**: Defines public interfaces and contracts exposed by Afina services.
- **Afina.Data**: A shared library containing all Entity Framework Core entities and migrations.

### Modules
Each module (e.g., `Afina.Modules.Identity`) is a vertical slice designed to fit by itself.
- **Endpoints**: Contains API endpoints. Each endpoint corresponds to a single action and resides in its own subfolder containing:
    - `{Name}Endpoint.cs`
    - `{Name}Request.cs` (with validators)
    - `{Name}Response.cs`
    - Middlewares (if any)
- **Services**: Contains business logic.
- **Models**: Contains business logic models. Services output only objects from Models.
- **Repositories**: Handles data access. Outputs only objects from Entities (defined in `Afina.Data`).
- **Proxies**: Proxies to external services.

### Data Layer
- **Afina.Data**:
    - **Entities**: All database entities. Must inherit from `EntityBase` (contains `CreatedAt`, `UpdatedAt`, etc.).
    - **Migrations**: EF Core migrations.

## Technology Stack
- **Framework**: .NET 8 (ASP.NET Core)
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core
- **API**: Minimal APIs
*   **Frontend**: React, TypeScript, Vite, Styled-Components, Tailwind CSS.
*   **Containerization**: Docker, Docker Compose.
*   **Testing**: xUnit, Testcontainers.

## Zero-Knowledge / Pluggable Encryption Architecture
*   **Server-Side Encryption**: Data is encrypted by a dedicated Encryption Service before being stored.
*   **Pluggable**: Tenants can define their own Encryption Service URL.
*   **Key Management**: Keys are managed by the Encryption Service, not the main application.
*   **Versioning & Rotation**: Data includes metadata about the key version and algorithm used.

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
