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

Run `make help` to see all available commands.

### Docker Profiles

Profiles enable optional services via `COMPOSE_PROFILES` in `.env`:

- **`db-view`**: Includes pgAdmin for database administration at http://localhost:5050

### Essential Commands

| Command            | Description                   |
| ------------------ | ----------------------------- |
| `make build`       | Build all applications        |
| `make run`         | Run all services              |
| `make stop`        | Stop services                 |
| `make restart`     | Restart services              |
| `make logs`        | View all logs                 |
| `make test`        | Run tests                     |
| `make before-push` | Run all checks before pushing |
| `make dev-api`     | Run API with hot reload       |
| `make dev-web`     | Run Web with hot reload       |

### Native Development

Add `-native` suffix to run commands natively (e.g., `make build-native`, `make test-native`).

### Database Migrations

| Command                                 | Description      |
| --------------------------------------- | ---------------- |
| `make add-migration-native NAME=<name>` | Create migration |
| `make execute-migration`                | Apply migrations |

## Service URLs

### Docker Deployment

- **Web UI**: http://localhost:3000
- **API**: http://localhost:5100
- **API Docs**: http://localhost:5100/swagger
- **PostgreSQL**: localhost:5432
- **pgAdmin**: http://localhost:5050 (when using `db-view` profile)

### Native Development

- **Web UI**: http://localhost:5173 (Vite default)
- **API**: http://localhost:5100

## Development Workflow

```bash
# Start services
make run

# View logs
make logs

# Rebuild after changes
make build && make restart

# Stop services
make stop
```

For active development with hot reload:

```bash
make dev-api    # Terminal 1
make dev-web    # Terminal 2
```

## Database Migrations

```bash
# Create migration
make add-migration NAME=AddUserTable

# Apply migrations
make execute-migration
```

Add `-native` suffix for native development.

## Testing

```bash
make test              # Docker
make test-native       # Native
```

## Code Quality

```bash
make format            # Format code
make lint              # Lint code
make before-push       # Run all checks
```

## Database Management

```bash
# Docker
make shell-db

# Native
psql -h localhost -U postgres -d afina_db
```

**pgAdmin**: For a web UI to manage the database, enable the `db-view` profile in `.env` and see [pgAdmin documentation](config/pgadmin/README.md).

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

**Port conflicts**: Modify ports in `.env` file.

**Database issues**: Run `make logs-db` to check PostgreSQL logs.

**Build errors**: Run `make clean && make build` for clean rebuild.

**Podman on macOS**: Ensure machine is running with `podman machine start`.

## Architecture

- **Modular Design**: Clean separation of concerns with feature-based modules
- **Multi-stage Docker builds**: Optimized image sizes
- **Health checks**: Automatic service health monitoring
- **Automatic migrations**: Database migrations on API startup (Docker)
- **Hot reload**: Native development with instant code updates
- **Persistent volumes**: Database data survives container restarts
- **Logging**: Structured logging to console output

## Logging

Afina uses Serilog for structured logging. Logs are output to the console in both Docker and native deployments.

```bash
# Docker: View logs
make logs

# Docker: View API logs specifically
make logs-api
```

See [Logging Documentation](docs/observability.md) for details.

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
