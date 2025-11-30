# Audit Logs

## Overview

The system captures all user actions in immutable audit logs accessible to tenant and system admins.

## Architecture

- **Trigger**: API actions generate audit events
- **Capture**: Middleware captures event context (User, IP, Action, Resource)
- **Storage**: PostgreSQL `AuditLogs` table
- **Implementation**: Serilog structured logging

## Log Format

Each audit log entry contains:

- `CreatedAt`: Unix timestamp
- `CreatedAtUtc`: UTC datetime
- `Actor`: User ID and name
- `Action`: Action type enum (e.g., `VAULT_ITEM_CREATED`, `USER_INVITED`)
- `Target`: Resource ID
- `TenantId`: Tenant context
- `Metadata`: JSON with action-specific details

## User Interface

- **Dashboard**: Admin-only "Audit Log" page
- **Filters**: Date range, user, action type
- **Display**: Timeline view

## Export

- **Format**: CSV
- **Mechanism**: Streaming for large datasets
- **Endpoint**: `GET /api/v1/audit/export`
