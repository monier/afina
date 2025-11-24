# Deployment & Infrastructure

## Overview
The platform is designed to be deployed as a set of Docker containers. This allows for easy deployment on a single machine using Docker Compose, or scaling up to Kubernetes in the future.

## Docker Compose (Local/Single Node)
The `docker-compose.yml` defines the following services:

1.  **`db`**: PostgreSQL 15+
    *   Volume: `pgdata` for persistence.
    *   Environment: `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`.
2.  **`api`**: .NET Core Backend
    *   Build: `Dockerfile` in `src/backend`.
    *   Depends on: `db`.
    *   Environment: Connection strings, JWT secrets.
3.  **`web`**: React Frontend
    *   Build: `Dockerfile` in `src/frontend`.
    *   Ports: `80:80`.
    *   Serves static files via Nginx.

## Infrastructure Requirements
*   **OS**: Linux (Ubuntu/Debian recommended) or macOS/Windows with Docker Desktop.
*   **CPU**: 2+ Cores.
*   **RAM**: 4GB+ (mostly for DB and .NET runtime).
*   **Storage**: SSD recommended for DB performance.

## CI/CD Pipeline
*   **Source Control**: GitHub/GitLab.
*   **CI**: GitHub Actions.
    *   **Build**: Compile .NET and React apps.
    *   **Test**: Run Unit and Integration tests (using Testcontainers).
    *   **Lint**: Check code style.
*   **CD**: Push Docker images to registry (GHCR/DockerHub).

## Future Kubernetes Support
*   Helm charts will be created to deploy the services.
*   Ingress Controller (Nginx/Traefik) for routing.
*   Secret Management (Kubernetes Secrets or HashiCorp Vault).
