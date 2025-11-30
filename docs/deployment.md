# Deployment & Infrastructure

## Overview

The Afina platform is designed to be deployed as a set of Docker containers, making it easy to deploy on a single machine using Docker Compose, or scale up to Kubernetes in the future. The platform supports both containerized and native deployment strategies.

## Local Development

### Prerequisites

Choose your development approach:

#### Docker/Podman Setup (Recommended)

- Docker or Podman installed
- docker-compose or podman-compose installed

#### Native Setup

- .NET 9 SDK
- Node.js 25+
- PostgreSQL 16

### Quick Start

#### Docker Development

```bash
# Build and start all services
make build
make run

# View logs
make logs

# Stop services
make stop
```

#### Native Development

```bash
# Ensure PostgreSQL is running locally

# Build applications
make build-native

# Run with hot reload (recommended for development)
make dev-api    # Terminal 1
make dev-web    # Terminal 2
```

### Available Make Commands

The project uses a consistent Makefile with standardized commands:

#### Core Commands (Docker by default)

- `make clean` - Clean all resources
- `make build` - Build applications
- `make run` - Run all services
- `make format` - Format code
- `make lint` - Lint/check code
- `make test` - Run all tests
- `make before-push` - Run all checks before pushing

#### Native Development (add -native suffix)

- `make clean-native` - Clean native artifacts
- `make build-native` - Build natively
- `make run-api-native` - Run API natively
- `make run-web-native` - Run Web natively
- `make format-native` - Format code natively
- `make lint-native` - Lint code natively
- `make test-native` - Run tests natively

#### Database Migrations

- `make add-migration NAME=<name>` - Create migration (Docker)
- `make add-migration-native NAME=<name>` - Create migration (native)
- `make execute-migration` - Apply migrations (Docker)
- `make execute-migration-native` - Apply migrations (native)

See `make help` for the complete list of commands.

## Docker Compose (Single Node Deployment)

### Services

The `docker-compose.yml` defines the following services:

1. **postgres**: PostgreSQL 16

   - Port: 5432 (configurable via `.env`)
   - Volume: `postgres-data` for persistence
   - Health checks: Automatic readiness detection
   - Environment: Configured via `.env` file

2. **api**: .NET 9 Backend

   - Build: Multi-stage Dockerfile in `apps/api`
   - Port: 8080 (mapped to host port via `.env`)
   - Depends on: postgres (with health check)
   - Automatic migrations: Not enabled by default
   - Environment: Connection strings, CORS origins

3. **web**: React Frontend

   - Build: Multi-stage Dockerfile in `apps/web`
   - Port: 80 (mapped to host port via `.env`)
   - Serves: Static files via nginx
   - Depends on: api

4. **migrate**: Migration Runner (tools profile)
   - Build: Dedicated migration Dockerfile
   - Purpose: Run database migrations
   - Usage: `make execute-migration`
   - Profile: Only runs when explicitly called

### Environment Configuration

Copy `.env.example` to `.env` and configure:

```bash
# Ports
API_PORT=5100
WEB_PORT=3000
DB_PORT=5432

# Database
DB_HOST=postgres
DB_NAME=afina_db
DB_USER=postgres
DB_PASSWORD=<strong-password>

# API
ASPNETCORE_ENVIRONMENT=Production
CORS_ORIGINS=http://localhost:3000

# Web
VITE_API_URL=http://localhost:5100
```

### Deployment Commands

```bash
# Pull latest code
git pull origin main

# Build and start services
make build
make run

# Run migrations
make execute-migration

# View service status
make ps

# View logs
make logs

# Restart services
make restart

# Stop services
make stop

# Clean everything (including volumes)
make clean
```

## Infrastructure Requirements

### Minimum Requirements

- **OS**: Linux (Ubuntu 22.04+ or Debian 12+ recommended), macOS with Docker Desktop, or Windows with WSL2
- **CPU**: 2 cores
- **RAM**: 4GB (8GB recommended)
- **Storage**: 20GB SSD (for Docker images and database)
- **Network**: Open ports 80/443 for web traffic

### Recommended Production Setup

- **OS**: Linux (Ubuntu 22.04 LTS)
- **CPU**: 4+ cores
- **RAM**: 8GB+
- **Storage**: 50GB+ SSD with separate volume for database
- **Network**: Load balancer with SSL termination
- **Backup**: Automated PostgreSQL backups

## CI/CD Pipeline

### Source Control

- **Repository**: GitHub/GitLab
- **Branching**: GitFlow (main, develop, feature/\*)
- **Protection**: Main branch requires PR and passing tests

### Continuous Integration

Using GitHub Actions (example):

```yaml
name: CI

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: "9.0.x"

      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: "25"

      - name: Restore dependencies
        run: make restore-native

      - name: Build
        run: make build-native

      - name: Lint
        run: make lint-native

      - name: Test
        run: make test-native
```

### Continuous Deployment

```yaml
name: CD

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Build Docker images
        run: make build

      - name: Push to registry
        run: |
          docker tag afina-api:latest ghcr.io/${{ github.repository }}/api:latest
          docker tag afina-web:latest ghcr.io/${{ github.repository }}/web:latest
          docker push ghcr.io/${{ github.repository }}/api:latest
          docker push ghcr.io/${{ github.repository }}/web:latest

      - name: Deploy to production
        run: |
          # SSH to production server and run:
          # docker-compose pull
          # docker-compose up -d
```

## Production Deployment

### Docker Compose Production

1. **Server Setup**

   ```bash
   # Install Docker
   curl -fsSL https://get.docker.com -o get-docker.sh
   sh get-docker.sh

   # Install Docker Compose
   apt-get install docker-compose-plugin
   ```

2. **Deploy Application**

   ```bash
   # Clone repository
   git clone <repository-url> /opt/afina
   cd /opt/afina

   # Configure environment
   cp .env.example .env
   nano .env  # Edit with production values

   # Build and start
   make build
   make run
   make execute-migration
   ```

3. **Setup Reverse Proxy (nginx)**

   ```nginx
   server {
       listen 80;
       server_name afina.example.com;

       location / {
           proxy_pass http://localhost:3000;
           proxy_set_header Host $host;
           proxy_set_header X-Real-IP $remote_addr;
       }

       location /api {
           proxy_pass http://localhost:5100;
           proxy_set_header Host $host;
           proxy_set_header X-Real-IP $remote_addr;
       }
   }
   ```

4. **Enable SSL with Let's Encrypt**
   ```bash
   apt-get install certbot python3-certbot-nginx
   certbot --nginx -d afina.example.com
   ```

### Database Backups

```bash
# Backup
docker exec afina-postgres pg_dump -U postgres afina_db > backup.sql

# Restore
cat backup.sql | docker exec -i afina-postgres psql -U postgres -d afina_db

# Automated daily backups (crontab)
0 2 * * * cd /opt/afina && docker exec afina-postgres pg_dump -U postgres afina_db | gzip > /backups/afina_$(date +\%Y\%m\%d).sql.gz
```

## Future Kubernetes Support

The platform is designed with Kubernetes deployment in mind:

### Planned Features

- **Helm Charts**: Package management for Kubernetes
- **Horizontal Pod Autoscaling**: Automatic scaling based on load
- **Ingress Controller**: Nginx/Traefik for routing
- **Secret Management**: Kubernetes Secrets or HashiCorp Vault
- **Persistent Volumes**: For PostgreSQL data
- **Service Mesh**: Istio/Linkerd for advanced networking

### Example Kubernetes Architecture

```
┌─────────────────────────────────────────┐
│           Ingress Controller            │
│         (nginx/traefik + SSL)           │
└──────────────┬──────────────────────────┘
               │
       ┌───────┴────────┐
       │                │
┌──────▼──────┐  ┌─────▼──────┐
│ Web Service │  │ API Service│
│   (3 pods)  │  │  (3 pods)  │
└──────┬──────┘  └─────┬──────┘
       │                │
       └────────┬───────┘
                │
        ┌───────▼────────┐
        │  PostgreSQL    │
        │ StatefulSet    │
        │ (with PV/PVC)  │
        └────────────────┘
```

### Sample Helm Values

```yaml
api:
  replicaCount: 3
  image:
    repository: ghcr.io/org/afina-api
    tag: latest
  resources:
    limits:
      cpu: 1000m
      memory: 2Gi
    requests:
      cpu: 500m
      memory: 1Gi

web:
  replicaCount: 3
  image:
    repository: ghcr.io/org/afina-web
    tag: latest

postgresql:
  enabled: true
  persistence:
    size: 50Gi
  resources:
    requests:
      memory: 4Gi
      cpu: 2000m
```

## Logging

The platform includes optional Grafana + Loki logging:

### Enable Logging

```bash
# In .env
COMPOSE_PROFILES=observability
LOGGING_PROVIDER=Grafana

# Restart services
make restart
```

### Access Logs

- **Grafana UI**: http://localhost:3001
- **Query logs**: Use LogQL in Grafana Explore

See [Logging Documentation](observability.md) for details.

### Health Check Endpoints

The API provides health check endpoints:

- `/health` - Overall health status
- `/health/ready` - Readiness probe
- `/health/live` - Liveness probe

## Security Considerations

### Production Checklist

- [ ] Use strong, unique passwords in `.env`
- [ ] Enable HTTPS/TLS for all external traffic
- [ ] Configure firewall rules (only expose necessary ports)
- [ ] Enable PostgreSQL SSL connections
- [ ] Use secret management (not plain text `.env`)
- [ ] Implement rate limiting
- [ ] Enable structured logging
- [ ] Regular security updates (`make build` with latest base images)
- [ ] Database backups with encryption
- [ ] Implement intrusion detection

### Environment Variables Security

Use Docker secrets or external secret management:

```bash
# Docker secrets example
echo "my-db-password" | docker secret create db_password -
```

## Troubleshooting

### Common Issues

**Port conflicts**

```bash
# Check what's using the port
lsof -i :5100
# Change port in .env file
```

**Database connection issues**

```bash
# Check PostgreSQL is running
make logs-db
# Verify connection
make shell-db
```

**Build failures**

```bash
# Clean rebuild
make clean
make build
```

**Out of disk space**

```bash
# Clean Docker resources
docker system prune -a --volumes
```

### Getting Help

- Check logs: `make logs`
- Verify service status: `make ps`
- Run tests: `make test-native`
- Review documentation in `/docs`

## Performance Tuning

### Docker

- Use BuildKit for faster builds: `DOCKER_BUILDKIT=1 make build`
- Adjust resource limits in `docker-compose.yml`
- Use Docker layer caching

### PostgreSQL

- Tune `shared_buffers`, `work_mem`, `maintenance_work_mem`
- Enable connection pooling (PgBouncer)
- Regular VACUUM and ANALYZE

### .NET API

- Enable response compression
- Use output caching
- Configure connection pooling
- Consider AOT compilation for faster startup

### React Web

- Enable production build optimizations
- Use CDN for static assets
- Implement code splitting
- Enable gzip/brotli compression in nginx
