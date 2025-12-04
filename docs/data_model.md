# Data Model

## Overview

The database uses a relational model (PostgreSQL) to store system configuration, user data, and encrypted vault items.

## Database Schema Configuration

The database schema is configurable per environment using PostgreSQL's `SearchPath` parameter in the connection string. This allows different schemas for different environments:

- **Development**: `afina_dev`
- **Test**: `afina_test`
- **Staging**: `afina_sandbox`
- **Production**: `afina_prod`

**Configuration:**

All schema configuration is done via connection strings - there is no schema management in the .NET code.

1. **Local Development** (`appsettings.dev.json`):

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=afina_db;Username=postgres;Password=postgres;SearchPath=afina_dev"
     }
   }
   ```

2. **Docker Environment** (`.env` file):

   ```bash
   DB_SCHEMA=afina_dev
   ```

   The `docker-compose.yml` automatically adds this to the connection string:

   ```yaml
   - ConnectionStrings__DefaultConnection=Host=${DB_HOST};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD};SearchPath=${DB_SCHEMA}
   ```

3. **Testing**:
   Tests automatically append `;SearchPath=afina_test` to the test container connection string.

**Creating Schemas:**

Before running migrations in a new environment, create the schema:

```sql
CREATE SCHEMA IF NOT EXISTS afina_dev;
```

Then run migrations:

```bash
dotnet ef database update --project Afina.Data
```

## Vertical Slice Persistence Access

Repositories and query helpers live inside each module's `Features/Persistence` folder and operate on entities defined exclusively in `Afina.Data`. They DO NOT redefine entities or migrations. Handlers depend on repository abstractions, keeping EF Core details localized.

## Entities

### System & Identity

- **User**

  - `Id`: UUID (PK)
  - `Username`: String (Unique)
  - `PasswordHash`: String
  - `PasswordHint`: String (Nullable)
  - `SystemRole`: Enum (Admin, Member)
  - `IndividualTenantId`: UUID (FK to Tenant)
  - `CreatedAtUtc`: DateTime (UTC)
  - `CreatedBy`: UUID (Nullable - System)
  - `UpdatedAtUtc`: DateTime (UTC, Nullable)
  - `UpdatedBy`: UUID (Nullable)

- **RefreshToken**

  - `Id`: UUID (PK)
  - `UserId`: UUID (FK)
  - `Token`: String
  - `ExpiresAt`: DateTime
  - `CreatedAtUtc`: DateTime (UTC)
  - `CreatedBy`: UUID
  - `UpdatedAtUtc`: DateTime (UTC, Nullable)
  - `UpdatedBy`: UUID

- **ApiKey**

  - `Id`: UUID (PK)
  - `UserId`: UUID (FK)
  - `Name`: String (Friendly name)
  - `KeyPrefix`: String (First few chars for display)
  - `SecretHash`: String (Hashed secret)
  - `Scopes`: JSONB (List of allowed actions: "EXPORT", "ROTATE", "USER_MGMT")
  - `ExpiresAt`: DateTime (Nullable)
  - `CreatedAtUtc`: DateTime (UTC)
  - `CreatedBy`: UUID
  - `UpdatedAtUtc`: DateTime (UTC, Nullable)
  - `UpdatedBy`: UUID

- **ApiKeyTenantAccess**
  - `ApiKeyId`: UUID (FK)
  - `TenantId`: UUID (FK)
  - `CreatedAtUtc`: DateTime (UTC)
  - `CreatedBy`: UUID
  - _PK_: (ApiKeyId, TenantId)

### Multi-Tenancy

- **Tenant**

  - `Id`: UUID (PK)
  - `Name`: String
  - `Type`: Enum (Individual, Organization)
  - `EncryptionServiceUrl`: String (Nullable, overrides default)
  - `EncryptionConfig`: JSONB (Auth credentials, etc.)
  - `EncryptionId`: UUID (The root identifier for this tenant's keys in the Encryption Service)
  - `CreatedAtUtc`: DateTime (UTC)
  - `CreatedBy`: UUID
  - `UpdatedAtUtc`: DateTime (UTC, Nullable)
  - `UpdatedBy`: UUID

- **TenantMembership**
  - `TenantId`: UUID (FK)
  - `UserId`: UUID (FK)
  - `Role`: Enum (TenantAdmin, TenantMember)
  - `CreatedAtUtc`: DateTime (UTC)
  - `CreatedBy`: UUID
  - `UpdatedAtUtc`: DateTime (UTC, Nullable)
  - `UpdatedBy`: UUID
  - _PK_: (TenantId, UserId)

### Vault (Private Data)

- **Vault**
  - `Id`: UUID (PK)
  - `TenantId`: UUID (FK)
  - `Type`: Enum (Credential, Document, Note, Media)
  - `CipherText`: String (Encrypted content)
  - `EncryptionId`: UUID (ID of the encryption context)
  - `EncryptionVersionId`: UUID (ID of the specific key version used)
  - `EncryptionMetadata`: JSONB (IV, Nonce, etc.)
  - `Metadata`: JSONB (Unencrypted metadata for filtering/searching)
  - `CreatedAtUtc`: DateTime (UTC)
  - `CreatedBy`: UUID (FK to User)
  - `UpdatedAtUtc`: DateTime (UTC, Nullable)
  - `UpdatedBy`: UUID

### Audit

- **AuditLog**
  - `Id`: UUID (PK)
  - `CreatedAtUtc`: DateTime (UTC)
  - `UserId`: UUID (FK)
  - `Action`: String (e.g., "LOGIN", "CREATE_ITEM", "DELETE_USER")
  - `TenantId`: UUID (Nullable FK)
  - `Details`: JSONB (Contextual info)

### Encryption Service (Internal Database - Default Implementation)

- **Encryption**

  - `Id`: UUID (PK) - Corresponds to `EncryptionId` in Tenant.
  - `MasterKeyHash`: String (Hash of the master key for verification)
  - `CurrentSymmetricKeyId`: UUID (FK to EncryptionKeyVersion)
  - `CurrentAsymmetricKeyId`: UUID (FK to EncryptionKeyVersion)
  - `CreatedAtUtc`: DateTime (UTC)
  - `CreatedBy`: UUID
  - `UpdatedAtUtc`: DateTime (UTC, Nullable)
  - `UpdatedBy`: UUID

- **EncryptionKeyVersion**
  - `Id`: UUID (PK)
  - `EncryptionId`: UUID (FK)
  - `Type`: Enum (Symmetric, Asymmetric)
  - `Algorithm`: String (e.g., "AES-256-GCM", "RSA-4096")
  - `EncryptedKeyMaterial`: String (The actual key, encrypted by the Master Key)
  - `Status`: Enum (Active, Archived)
  - `CreatedAtUtc`: DateTime (UTC)
  - `CreatedBy`: UUID
  - `UpdatedAtUtc`: DateTime (UTC, Nullable)
  - `UpdatedBy`: UUID

## ER Diagram

```mermaid
erDiagram
    User ||--o{ TenantMembership : "has"
    User ||--o{ AuditLog : "generates"
    Tenant ||--o{ TenantMembership : "has members"
    Tenant ||--o{ VaultItem : "owns"
    Tenant ||--o{ AuditLog : "related to"

    User ||--o{ ApiKey : "owns"
    ApiKey ||--o{ ApiKeyTenantAccess : "accesses"
    Tenant ||--o{ ApiKeyTenantAccess : "accessed by"

    User {
        UUID Id
        String Username
        String PasswordHash
        Enum SystemRole
    }

    Tenant {
        UUID Id
        String Name
        Enum Type
        String EncryptionServiceUrl
    }

    TenantMembership {
        UUID TenantId
        UUID UserId
        Enum Role
    }

    Vault {
        UUID Id
        UUID TenantId
        Enum Type
        String CipherText
        UUID EncryptionId
        UUID EncryptionVersionId
    }

    AuditLog {
        UUID Id
        Timestamp CreatedAt
        DateTime CreatedAtUtc
        UUID UserId
        String Action
    }
```
