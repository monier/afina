# Afina Platform - Docker & Build Guide

## Prerequisites

Choose one of the following setups:

### Docker/Podman Setup
- Docker or Podman installed
- docker-compose or podman-compose installed

### Native Setup
- .NET 9 SDK
- Node.js 25+
- PostgreSQL 16

## Quick Start

### Using Docker/Podman (Recommended)

```bash
# Build and run everything
make run

# View logs
make logs

# Stop services
make stop
```

### Using Native Development

```bash
# Ensure PostgreSQL is running on localhost:5432

# Build applications
make build-native

# Run applications
make run-native

# Stop applications
make stop-native
```

## Available Make Commands

Run `make help` to see all available commands:

### Essential Commands

- **`make run`** - Start the full infrastructure with Docker (Web, API, PostgreSQL)
- **`make run-native`** - Run applications natively (requires local PostgreSQL)
- **`make stop`** - Stop Docker services
- **`make stop-native`** - Stop native services
- **`make build`** - Build Docker images
- **`make build-native`** - Build applications natively
- **`make clean`** - Clean everything (Docker and native artifacts)
- **`make format`** - Format all source code

### Development Commands

- **`make dev-api`** - Run API with hot reload
- **`make dev-web`** - Run Web with hot reload
- **`make logs`** - View all logs
- **`make logs-api`** - View API logs only
- **`make logs-web`** - View Web logs only
- **`make migrate`** - Run database migrations (Docker)
- **`make migrate-native`** - Run database migrations (native)

### Docker Management

- **`make shell-api`** - Open shell in API container
- **`make shell-db`** - Open PostgreSQL shell
- **`make restart`** - Restart all services
- **`make ps`** - Show running containers

## Service URLs

After running `make run`:

- **Web UI**: http://localhost:3000
- **API**: http://localhost:5100
- **PostgreSQL**: localhost:5432

After running `make run-native`:

- **Web UI**: http://localhost:5173
- **API**: http://localhost:5100

## Docker Compose Details

The `docker-compose.yml` includes:

- **postgres**: PostgreSQL 16 with persistent volume
- **api**: .NET API with automatic migrations
- **web**: React SPA served by nginx

### Podman Compatibility

The compose file is fully compatible with Podman. The Makefile automatically detects whether you're using Docker or Podman.

## Development Workflow

### Docker-based Development

```bash
# Start infrastructure
make run

# View logs in real-time
make logs

# Make code changes (they won't be hot-reloaded in containers)

# Rebuild and restart
make build
make restart

# When done
make stop
```

### Native Development (with hot reload)

```bash
# Terminal 1 - API with hot reload
make dev-api

# Terminal 2 - Web with hot reload
make dev-web

# Code changes will auto-reload
```

## Database Management

### Run Migrations (Docker)
```bash
make migrate
```

### Run Migrations (Native)
```bash
make migrate-native
```

### Access Database Shell
```bash
# Docker
make shell-db

# Native (requires psql)
psql -h localhost -U postgres -d afina_db
```

## Cleaning Up

### Clean Docker Resources
```bash
make clean-docker
```

### Clean Native Build Artifacts
```bash
make clean-native
```

### Clean Everything
```bash
make clean
```

## Troubleshooting

### Port Conflicts

If ports 3000, 5100, or 5432 are already in use, modify the port mappings in `docker-compose.yml`.

### Podman on macOS

If using Podman on macOS, ensure the Podman machine is running:
```bash
podman machine start
```

### Database Connection Issues

Ensure PostgreSQL is healthy:
```bash
make logs-db
```

For native development, verify PostgreSQL is running:
```bash
pg_isready -h localhost -p 5432
```

## Building for Production

```bash
# Build optimized Docker images
make build

# Or build native artifacts
make build-native
cd apps/web && npm run build
```

## Architecture

- **Multi-stage Docker builds** for optimized image sizes
- **Health checks** for PostgreSQL
- **Automatic database migrations** on API startup
- **Nginx** for serving the React SPA
- **Persistent volumes** for database data
