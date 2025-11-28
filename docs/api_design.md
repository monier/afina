# API Design

## Overview

The API follows RESTful principles. All endpoints are prefixed with `/api/v1`.
Responses are in JSON format.

## Vertical Slice Endpoint Pattern

Endpoints are thin and delegate directly to handlers via the custom mediator (`IMediator`). We use a flattened per-feature layout: each feature lives in `apps/api/Afina.Modules.Users/Features/{FeatureName}/` with separate files for Request, Response, Endpoint, and Handler. Cross-feature persistence and services sit in `apps/api/Afina.Modules.Users/Shared/`.

## Users Module: Flattened Features with Request/Response Files

### Endpoint → Feature Path Mapping

Each feature contains:

- `{FeatureName}Request.cs` - Request DTO
- `{FeatureName}Response.cs` - Response DTO (when applicable)
- `{FeatureName}Endpoint.cs` - HTTP endpoint mapping
- `{FeatureName}Handler.cs` - Business logic handler

**Authentication & User Management:**

- **POST /api/v1/auth/login**
  - Files: `Features/Login/LoginRequest.cs`, `LoginResponse.cs`, `LoginEndpoint.cs`, `LoginHandler.cs`
- **POST /api/v1/auth/register**
  - Files: `Features/Register/RegisterRequest.cs`, `RegisterResponse.cs`, `RegisterEndpoint.cs`, `RegisterHandler.cs`
- **POST /api/v1/auth/refresh**
  - Files: `Features/RefreshToken/RefreshTokenRequest.cs`, `RefreshTokenResponse.cs`, `RefreshTokenEndpoint.cs`, `RefreshTokenHandler.cs`

**User Profile:**

- **GET /api/v1/users/me**
  - Files: `Features/GetCurrentUser/GetCurrentUserRequest.cs`, `GetCurrentUserResponse.cs`, `GetCurrentUserEndpoint.cs`, `GetCurrentUserHandler.cs`
- **DELETE /api/v1/users/me**
  - Files: `Features/DeleteUser/DeleteUserRequest.cs`, `DeleteUserEndpoint.cs`, `DeleteUserHandler.cs`
  - Note: Uses `EmptyResponse` for response (void operation)
- **POST /api/v1/users/me/export**
  - Files: `Features/ExportUserData/ExportUserDataRequest.cs`, `ExportUserDataResponse.cs`, `ExportUserDataEndpoint.cs`, `ExportUserDataHandler.cs`

**API Keys:**

- **GET /api/v1/users/me/api-keys**
  - Files: `Features/ListApiKeys/ListApiKeysRequest.cs`, `ListApiKeysResponse.cs`, `ListApiKeysEndpoint.cs`, `ListApiKeysHandler.cs`
- **POST /api/v1/users/me/api-keys**
  - Files: `Features/CreateApiKey/CreateApiKeyRequest.cs`, `CreateApiKeyResponse.cs`, `CreateApiKeyEndpoint.cs`, `CreateApiKeyHandler.cs`
  - Response: `{ id, keyPrefix, secret }` (Secret shown only once)
- **DELETE /api/v1/users/me/api-keys/{keyId}**
  - Files: `Features/DeleteApiKey/DeleteApiKeyRequest.cs`, `DeleteApiKeyEndpoint.cs`, `DeleteApiKeyHandler.cs`
  - Note: Uses `EmptyResponse` for response (void operation)

### Shared Components (Users Module)

**Persistence:**

- `Shared/Persistence/IUserRepository.cs`, `UsersRepository.cs`
- `Shared/Persistence/IUserSessionsRepository.cs`, `UserSessionsRepository.cs`
- `Shared/Persistence/IApiKeyRepository.cs`, `ApiKeyRepository.cs`

**Services:**

- `Shared/Services/ITokenService.cs`, `JwtTokenService.cs`, `TokenService.cs`

## Tenants

- **GET /api/v1/tenants**
  - Response: List of tenants the user belongs to.
- **POST /api/v1/tenants**
  - Body: `{ name, encryptionServiceUrl? }`
- **PUT /api/v1/tenants/{id}/encryption**
  - Body: `{ serviceUrl, config }`
- **GET /api/v1/tenants/{id}**
- **PUT /api/v1/tenants/{id}**
- **DELETE /api/v1/tenants/{id}**
- **POST /api/v1/tenants/{id}/rotate-keys**
  - Action: Triggers key rotation in the encryption service.
- **POST /api/v1/tenants/{id}/export**
  - Query Params: `format` (csv, json, keepass)
  - Response: Zip file containing `vault.{format}`, `encryption_data.json`, `members.json`.

### Tenant Membership

- **GET /api/v1/tenants/{id}/members**
- **POST /api/v1/tenants/{id}/members**
  - Body: `{ userId, role }`
- **DELETE /api/v1/tenants/{id}/members/{userId}**

## Vault (Private Data)

- **GET /api/v1/tenants/{tenantId}/vault**
  - Query Params: `type`, `search` (on metadata only)
  - Response: List of Vault objects with decrypted `data` and standard fields (`createdAt`, `createdBy`, etc.).
- **POST /api/v1/tenants/{tenantId}/vault**
  - Body: `{ type, data, metadata }`
  - Note: `data` is plain text (sent over TLS). Backend encrypts it via Encryption Service.
  - Response: Created Vault object.
- **PUT /api/v1/tenants/{tenantId}/vault/{id}**
  - Body: `{ type, data, metadata }`
  - Note: Full update. Replaces existing data and metadata. Triggers re-encryption with active key.
- **DELETE /api/v1/tenants/{tenantId}/vault/{id}**

## Audit

- **GET /api/v1/audit**
  - Query Params: `dateFrom`, `dateTo`, `userId`, `action`
  - Response: Paginated list of logs.
- **GET /api/v1/audit/export**
  - Query Params: `format=csv`

## Encryption Service (Internal/External API)

- **POST /v1/encrypt**
  - Input: `encryptionId` (UUID), `data`, `encryptionVersionId` (UUID, optional)
  - Output: `cipherText`, `encryptionId`, `encryptionVersionId` (UUID)
- **POST /v1/decrypt**
  - Input: `encryptionId` (UUID), `cipherText`, `encryptionVersionId` (UUID)
- **POST /v1/rotate**
  - Input: `encryptionId` (UUID)
  - Output: `newVersionId` (UUID)
- **POST /v1/export**
  - Input: `encryptionId` (List of UUIDs or Single UUID)
  - Response: JSON containing encrypted key materials and metadata for backup/portability.
