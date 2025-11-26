# Audit Logs Design

## Overview
The system provides full visual logs for all user actions. These logs are immutable and accessible to tenant admins and system admins.

## Logging Architecture
*   **Trigger**: Every API action triggers an event.
*   **Capture**: Middleware or Service Decorators capture the event context (User, IP, Action, Resource).
*   **Storage**: Logs are stored in the `AuditLogs` table in PostgreSQL.
    *   *Future Optimization*: Move to Elasticsearch or a dedicated time-series DB for high volume.

## Log Format
Each log entry contains:
*   `CreatedAt`: Unix Timestamp of action.
*   `CreatedAtUtc`: UTC DateTime of action.
*   `Actor`: User ID and Name.
*   `Action`: Enum (e.g., `VAULT_ITEM_CREATED`, `USER_INVITED`).
*   `Target`: Resource ID (e.g., Vault Item ID).
*   `TenantId`: Context of the action.
*   `Metadata`: JSON object with specific details (e.g., "Changed role from Member to Admin").

## User Interface
*   **Dashboard**: Dedicated "Audit Log" page for Admins.
*   **Filters**:
    *   Date Range (From - To)
    *   User (Dropdown)
    *   Action Type (Dropdown)
*   **Visuals**: Timeline view of activity.

## Export
*   **Format**: CSV.
*   **Mechanism**: Backend streams the CSV file to avoid memory issues with large datasets.
*   **Endpoint**: `GET /api/v1/audit/export`
