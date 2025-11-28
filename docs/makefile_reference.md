# Makefile Command Reference

## Overview

The Afina project uses a comprehensive Makefile with consistent, standardized commands for both Docker-based and native development workflows.

## Command Naming Convention

All commands follow a consistent pattern:

- **Docker commands**: `make <command>` (default, no suffix)
- **Native commands**: `make <command>-native` (add `-native` suffix)

## Quick Reference Table

| Command         | Docker        | Native                | Description                               |
| --------------- | ------------- | --------------------- | ----------------------------------------- |
| **Clean**       | `make clean`  | `make clean-native`   | Remove all build artifacts and containers |
| **Build**       | `make build`  | `make build-native`   | Build all applications                    |
| **Run All**     | `make run`    | N/A                   | Run all services together                 |
| **Run API**     | `make run`    | `make run-api-native` | Run API service                           |
| **Run Web**     | `make run`    | `make run-web-native` | Run Web service                           |
| **Format**      | `make format` | `make format-native`  | Format all source code                    |
| **Lint**        | `make lint`   | `make lint-native`    | Lint/check all source code                |
| **Test**        | `make test`   | `make test-native`    | Run all tests                             |
| **Before Push** | N/A           | `make before-push`    | Run all quality checks                    |
| **Stop**        | `make stop`   | `make stop-native`    | Stop all services                         |

## Core Commands

### Clean

Remove build artifacts and clean up resources.

```bash
# Clean everything (Docker + native)
make clean

# Clean only native artifacts
make clean-native
```

**What it does:**

- Docker: Removes containers, volumes, and orphaned resources
- Native: Removes `bin/`, `obj/`, `node_modules/`, `dist/`, and `.vite/` directories

### Build

Build all applications.

```bash
# Build Docker images
make build

# Build applications natively
make build-native
```

**What it does:**

- Docker: Builds optimized Docker images
- Native: Restores dependencies and compiles .NET solution and React app

### Run

Start applications and services.

```bash
# Run all services with Docker
make run

# Run API natively (requires PostgreSQL)
make run-api-native

# Run Web natively
make run-web-native
```

**Service URLs:**

- Docker: Web at `:3000`, API at `:5100`, DB at `:5432`
- Native: Web at `:5173`, API at `:5100`

### Format

Format source code according to style guidelines.

```bash
# Format with Docker
make format

# Format natively
make format-native
```

**What it does:**

- API: Runs `dotnet format`
- Web: Runs `npm run lint -- --fix`

### Lint

Check code quality and style issues.

```bash
# Lint with Docker
make lint

# Lint natively
make lint-native
```

**What it does:**

- API: Runs `dotnet format --verify-no-changes`
- Web: Runs `npm run lint`

### Test

Run all test suites.

```bash
# Test with Docker
make test

# Test natively
make test-native
```

**What it does:**

- Runs all API tests with `dotnet test`
- Runs Web tests with `npm test` (if configured)

### Before Push

Run all quality checks before pushing code.

```bash
# Run all checks (clean, format, build, lint, test)
make before-push
```

**What it does:**

1. Cleans native artifacts
2. Formats all code
3. Builds applications natively
4. Lints all code
5. Runs all tests

**When to use:**

- Before committing code
- Before pushing to remote
- Before creating a pull request
- As part of your development workflow

**Note:** This command runs natively for speed and to identify issues early. It's perfect for pre-commit checks and will be used in CI/CD pipelines.

## API/Backend Specific Commands

### Database Migrations

#### Create Migration

```bash
# Docker
make add-migration NAME=AddUserTable

# Native
make add-migration-native NAME=AddUserTable
```

Creates a new Entity Framework Core migration.

#### Execute Migrations

```bash
# Docker
make execute-migration

# Native
make execute-migration-native
```

Applies pending database migrations.

## Docker Management Commands

### View Logs

```bash
# All services
make logs

# Specific service
make logs-api
make logs-web
make logs-db
```

### Service Management

```bash
# Show running containers
make ps

# Restart all services
make restart

# Stop all services
make stop
```

### Shell Access

```bash
# Open shell in API container
make shell-api

# Open PostgreSQL shell
make shell-db
```

## Development Helper Commands

### Hot Reload Development

```bash
# Run API with hot reload (auto-restart on code changes)
make dev-api

# Run Web with hot reload (auto-refresh on code changes)
make dev-web
```

**Recommended workflow:**

1. Terminal 1: `make dev-api`
2. Terminal 2: `make dev-web`
3. Edit code and see changes instantly

### Convenience Aliases

```bash
# Start services
make up        # Same as 'make run'

# Stop services
make down      # Same as 'make stop'

# Clean rebuild
make rebuild           # Docker
make rebuild-native    # Native

# Hot reload aliases
make watch-api   # Same as 'make dev-api'
make watch-web   # Same as 'make dev-web'
```

## Command Workflow Examples

### Full Docker Workflow

```bash
# First time setup
make build
make execute-migration
make run

# View the application
open http://localhost:3000

# View logs
make logs

# Stop when done
make stop

# Clean up everything
make clean
```

### Native Development Workflow

```bash
# First time setup
make build-native
make execute-migration-native

# Start development with hot reload
# Terminal 1
make dev-api

# Terminal 2
make dev-web

# Run tests when needed
make test-native

# Before committing
make before-push

# Stop services
make stop-native
```

### Adding a Database Migration

```bash
# 1. Create migration
make add-migration-native NAME=AddUserPreferences

# 2. Review generated migration in:
#    apps/api/Afina.Api/Migrations/

# 3. Apply migration
make execute-migration-native

# 4. Verify in database
make shell-db
# Then: \dt to list tables
```

### Before Committing Code

```bash
# Run all quality checks
make before-push

# Or run checks individually
make format-native
make lint-native
make test-native

# If all passes, commit your changes
git add .
git commit -m "Your message"
```

## Troubleshooting Commands

### Check What's Running

```bash
# Docker
make ps

# Native
ps aux | grep dotnet
ps aux | grep vite
```

### View Detailed Logs

```bash
# Docker
make logs-api    # API only
make logs-db     # Database only

# Native
tail -f /tmp/afina-api.log
tail -f /tmp/afina-web.log
```

### Force Clean Rebuild

```bash
# Docker - complete clean
make clean
docker system prune -a
make build

# Native - complete clean
make clean-native
make build-native
```

### Check Service Health

```bash
# Docker
make ps              # Shows container status
make shell-api       # Interactive shell
curl http://localhost:5100/health

# Native
curl http://localhost:5100/health
pg_isready -h localhost -p 5432
```

## Environment Variables

All commands respect the `.env` file in the project root:

```bash
# Ports
API_PORT=5100
WEB_PORT=3000
DB_PORT=5432

# Database
DB_HOST=postgres
DB_NAME=afina_db
DB_USER=postgres
DB_PASSWORD=postgres

# API
ASPNETCORE_ENVIRONMENT=Development
CORS_ORIGINS=http://localhost:3000,http://localhost:5173

# Web
VITE_API_URL=http://localhost:5100
```

## Help Commands

```bash
# Quick help (common commands)
make help

# Detailed help (all commands with descriptions)
make help-verbose
```

## Best Practices

1. **Use native commands for active development** - Hot reload makes development faster
2. **Use Docker commands for testing deployments** - Ensures production-like environment
3. **Always run `make before-push` before committing** - Catches issues early
4. **Use individual checks during development** - `make format-native`, `make lint-native`, `make test-native`
5. **Create descriptive migration names** - `AddUserPreferences`, not `Update1`
6. **Clean build when switching branches** - `make clean && make build`
7. **Check logs when things fail** - `make logs` or `make logs-api`

## Platform Support

The Makefile automatically detects and supports:

- **Docker** or **Podman**
- **docker-compose**, **podman-compose**, or **docker compose** plugin
- **macOS**, **Linux**, and **Windows** (via WSL2)

All error messages provide helpful guidance for fixing issues.
