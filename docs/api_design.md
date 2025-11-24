# API Design

## Overview
The API follows RESTful principles. All endpoints are prefixed with `/api/v1`.
Responses are in JSON format.

## Authentication
*   **POST /api/v1/auth/register**
    *   Body: `{ email, passwordHash, salt, encryptedPrivateKey, publicKey }`
    *   Response: `{ token, refreshToken, user }`
*   **POST /api/v1/auth/login**
    *   Body: `{ email, authHash }`
    *   Response: `{ token, refreshToken, user, encryptedPrivateKey }`
*   **POST /api/v1/auth/refresh**
    *   Body: `{ refreshToken }`

## Users
*   **GET /api/v1/users/me**
    *   Response: User profile details.
*   **DELETE /api/v1/users/me**
    *   Action: Deletes user account and individual tenant.
*   **POST /api/v1/users/me/export**
    *   Response: JSON/CSV export of all user data.

## Tenants
*   **GET /api/v1/tenants**
    *   Response: List of tenants the user belongs to.
*   **POST /api/v1/tenants**
    *   Body: `{ name, encryptedMasterKey }` (Encrypted with user's public key)
*   **GET /api/v1/tenants/{id}**
*   **PUT /api/v1/tenants/{id}**
*   **DELETE /api/v1/tenants/{id}**

### Tenant Membership
*   **GET /api/v1/tenants/{id}/members**
*   **POST /api/v1/tenants/{id}/members**
    *   Body: `{ userId, role, encryptedTenantKey }` (Admin encrypts key for new user)
*   **DELETE /api/v1/tenants/{id}/members/{userId}**

## Vault (Private Data)
*   **GET /api/v1/tenants/{tenantId}/items**
    *   Query Params: `type`, `search` (on metadata only)
*   **POST /api/v1/tenants/{tenantId}/items**
    *   Body: `{ type, encryptedData, metadata, iv }`
*   **PUT /api/v1/tenants/{tenantId}/items/{itemId}**
*   **DELETE /api/v1/tenants/{tenantId}/items/{itemId}**

## Audit
*   **GET /api/v1/audit**
    *   Query Params: `dateFrom`, `dateTo`, `userId`, `action`
    *   Response: Paginated list of logs.
*   **GET /api/v1/audit/export**
    *   Query Params: `format=csv`
