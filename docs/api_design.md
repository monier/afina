# API Design

## Overview
The API follows RESTful principles. All endpoints are prefixed with `/api/v1`.
Responses are in JSON format.

## Authentication
*   **POST /api/v1/auth/register**
    *   Body: `{ email, passwordHash, salt }`
    *   Response: `{ token, refreshToken, user }`
*   **POST /api/v1/auth/login**
    *   Body: `{ email, authHash }`
    *   Response: `{ token, refreshToken, user }`
*   **POST /api/v1/auth/refresh**
    *   Body: `{ refreshToken }`
*   **POST /api/v1/auth/mfa/setup**
    *   Response: `{ secret, qrCodeUrl }`
*   **POST /api/v1/auth/mfa/verify**
    *   Body: `{ code }`

## Users
*   **GET /api/v1/users/me**
    *   Response: User profile details (including `createdAt`, `updatedAt`).
*   **DELETE /api/v1/users/me**
    *   Action: Deletes user account and individual tenant.
*   **POST /api/v1/users/me/export**
    *   Response: JSON/CSV export of all user data.

## Tenants
*   **GET /api/v1/tenants**
    *   Response: List of tenants the user belongs to.
*   **POST /api/v1/tenants**
    *   Body: `{ name, encryptionServiceUrl? }`
*   **PUT /api/v1/tenants/{id}/encryption**
    *   Body: `{ serviceUrl, config }`
*   **GET /api/v1/tenants/{id}**
*   **PUT /api/v1/tenants/{id}**
*   **DELETE /api/v1/tenants/{id}**
*   **POST /api/v1/tenants/{id}/rotate-keys**
    *   Action: Triggers key rotation in the encryption service.
*   **POST /api/v1/tenants/{id}/export**
    *   Query Params: `format` (csv, json, keepass)
    *   Response: Zip file containing `vault.{format}`, `encryption_data.json`, `members.json`.

### Tenant Membership
*   **GET /api/v1/tenants/{id}/members**
*   **POST /api/v1/tenants/{id}/members**
    *   Body: `{ userId, role }`
*   **DELETE /api/v1/tenants/{id}/members/{userId}**

## Vault (Private Data)
*   **GET /api/v1/tenants/{tenantId}/vault**
    *   Query Params: `type`, `search` (on metadata only)
    *   Response: List of Vault objects with decrypted `data` and standard fields (`createdAt`, `createdBy`, etc.).
*   **POST /api/v1/tenants/{tenantId}/vault**
    *   Body: `{ type, data, metadata }`
    *   Note: `data` is plain text (sent over TLS). Backend encrypts it via Encryption Service.
    *   Response: Created Vault object.
*   **PUT /api/v1/tenants/{tenantId}/vault/{id}**
    *   Body: `{ type, data, metadata }`
    *   Note: Full update. Replaces existing data and metadata. Triggers re-encryption with active key.
*   **DELETE /api/v1/tenants/{tenantId}/vault/{id}**

## Audit
*   **GET /api/v1/audit**
    *   Query Params: `dateFrom`, `dateTo`, `userId`, `action`
    *   Response: Paginated list of logs.
*   **GET /api/v1/audit/export**
    *   Query Params: `format=csv`

## Encryption Service (Internal/External API)
*   **POST /v1/encrypt**
*   **POST /v1/decrypt**
*   **POST /v1/rotate**
*   **POST /v1/export**
    *   Input: `encryptionId` (List or Single)
    *   Response: JSON containing encrypted key materials and metadata for backup/portability.
