# Afina Platform

A modern, modular platform for secure credential management and multi-tenancy support built with .NET 9 and React.

## Prerequisites

Choose one of the following setups:

### Docker/Podman Setup (Recommended)

- **Docker** or **Podman** installed
- **docker-compose** or **podman-compose** installed

### Native Development Setup

- **.NET 9 SDK** - [Download](https://dotnet.microsoft.com/download)
- **Node.js 25+** - [Download](https://nodejs.org/)
- **PostgreSQL 16** - Running on `localhost:5432`

## Quick Start

### Using Docker/Podman (Recommended)

```bash
# Clone and setup
git clone <repository-url>
cd afina

# Copy environment file
cp .env.example .env

# Build and run everything
make build
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

# Run API (in one terminal)
make run-api-native

# Run Web (in another terminal)
make run-web-native

# Or use hot reload for development
make dev-api    # Terminal 1
make dev-web    # Terminal 2
```

## Available Commands

Run `make help` to see all available commands. Here are the most commonly used:

### Core Commands (Docker by default)

| Command            | Description                                                      |
| ------------------ | ---------------------------------------------------------------- |
| `make clean`       | Clean all resources (Docker + native artifacts)                  |
| `make build`       | Build all applications                                           |
| `make run`         | Run all services                                                 |
| `make format`      | Format all source code                                           |
| `make lint`        | Lint/check all source code                                       |
| `make test`        | Run all tests                                                    |
| `make before-push` | Run all checks before pushing (clean, format, build, lint, test) |

### Native Development Commands

Add `-native` suffix to any core command for native development:

| Command               | Description                  |
| --------------------- | ---------------------------- |
| `make clean-native`   | Clean native build artifacts |
| `make build-native`   | Build applications natively  |
| `make run-api-native` | Run API natively             |
| `make run-web-native` | Run Web natively             |
| `make format-native`  | Format code natively         |
| `make lint-native`    | Lint code natively           |
| `make test-native`    | Run tests natively           |

### API/Backend Specific Commands

| Command                                 | Description                     |
| --------------------------------------- | ------------------------------- |
| `make add-migration NAME=<name>`        | Create new migration (Docker)   |
| `make add-migration-native NAME=<name>` | Create new migration (native)   |
| `make execute-migration`                | Run pending migrations (Docker) |
| `make execute-migration-native`         | Run pending migrations (native) |

### Docker Management

| Command          | Description                 |
| ---------------- | --------------------------- |
| `make stop`      | Stop Docker services        |
| `make restart`   | Restart Docker services     |
| `make logs`      | View all logs               |
| `make logs-api`  | View API logs               |
| `make logs-web`  | View Web logs               |
| `make logs-db`   | View database logs          |
| `make ps`        | Show running containers     |
| `make shell-api` | Open shell in API container |
| `make shell-db`  | Open PostgreSQL shell       |

### Development Helpers

| Command            | Description             |
| ------------------ | ----------------------- |
| `make dev-api`     | Run API with hot reload |
| `make dev-web`     | Run Web with hot reload |
| `make stop-native` | Stop native services    |

## Service URLs

After running `make run` (Docker):

- **Web UI**: http://localhost:3000
- **API**: http://localhost:5100
- **PostgreSQL**: localhost:5432

After running native commands:

- **Web UI**: http://localhost:5173 (Vite default)
- **API**: http://localhost:5100

## Development Workflow

### Docker-based Development

```bash
# Start all services
make run

# View logs in real-time
make logs

# Make code changes
# (Rebuild required for changes to take effect)

# Rebuild and restart
make build
make restart

# When done
make stop
```

### Native Development (Recommended for active development)

```bash
# Terminal 1 - API with hot reload
make dev-api

# Terminal 2 - Web with hot reload
make dev-web

# Code changes will auto-reload!
```

### Database Migrations

#### Create a new migration

```bash
# Docker
make add-migration NAME=AddUserTable

# Native
make add-migration-native NAME=AddUserTable
```

#### Apply migrations

```bash
# Docker
make execute-migration

# Native
make execute-migration-native
```

## Testing

```bash
# Run all tests (Docker)
make test

# Run all tests (native)
make test-native

# API tests only
cd apps/api && dotnet test

# Web tests only
cd apps/web && npm test
```

## Code Quality

### Format code

```bash
# Format all code (Docker)
make format

# Format all code (native)
make format-native
```

### Lint code

```bash
# Lint all code (Docker)
make lint

# Lint all code (native)
make lint-native
```

## Database Management

### Access Database Shell

```bash
# Docker
make shell-db

# Native (requires psql)
psql -h localhost -U postgres -d afina_db
```

### Verify PostgreSQL is running

```bash
# Check if PostgreSQL is ready
pg_isready -h localhost -p 5432
```

## Cleaning Up

```bash
# Clean Docker resources only
docker-compose down -v --remove-orphans

# Clean native artifacts only
make clean-native

# Clean everything
make clean
```

## Troubleshooting

### Port Conflicts

If ports 3000, 5100, or 5432 are already in use:

1. Stop the conflicting service
2. Or modify the port mappings in `.env` file

### Podman on macOS

If using Podman on macOS, ensure the Podman machine is running:

```bash
podman machine start
```

### Database Connection Issues

For Docker:

```bash
make logs-db
```

For native development:

```bash
pg_isready -h localhost -p 5432
```

### Build Errors

Try a clean rebuild:

```bash
# Docker
make rebuild

# Native
make rebuild-native
```

## Architecture

- **Modular Architecture**: Clean separation of concerns with module-based design
- **Multi-stage Docker builds**: Optimized image sizes
- **Health checks**: Automatic service health monitoring
- **Automatic migrations**: Database migrations on API startup (Docker)
- **Hot reload**: Native development with instant code updates
- **Persistent volumes**: Database data survives container restarts

## Project Structure

```
afina/
├── apps/
│   ├── api/              # .NET 9 Backend
│   │   ├── Afina.Api/    # Main API project
│   │   └── Afina.Modules.*/ # Feature modules
│   └── web/              # React Frontend
├── docs/                 # Documentation
├── docker-compose.yml    # Docker orchestration
├── Makefile             # Build & run commands
└── README.md            # This file
```

## Contributing

1. Create a feature branch
2. Make your changes
3. Run quality checks: `make before-push`
4. Submit a pull request

## License

See [LICENSE](LICENSE) file for details.
