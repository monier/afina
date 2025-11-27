.PHONY: help clean format build run run-native stop logs clean-docker clean-native migrate migrate-create migrate-endpoint migrate-native

# Detect if using podman or docker
DOCKER_CMD := $(shell command -v podman 2> /dev/null)
ifndef DOCKER_CMD
	DOCKER_CMD := $(shell command -v docker 2> /dev/null)
endif

# Detect compose command (try podman-compose, docker-compose, then docker compose, then podman compose)
COMPOSE_CMD := $(shell command -v podman-compose 2> /dev/null)
ifndef COMPOSE_CMD
	COMPOSE_CMD := $(shell command -v docker-compose 2> /dev/null)
endif
ifndef COMPOSE_CMD
	ifneq ($(shell docker compose version 2> /dev/null),)
		COMPOSE_CMD := docker compose
	else ifneq ($(shell podman compose version 2> /dev/null),)
		COMPOSE_CMD := podman compose
	endif
endif

# Check if docker/podman tools are available
include .env
export $(shell sed 's/=.*//' .env)

# Detect if using podman or docker
define check_docker
	@if [ -z "$(DOCKER_CMD)" ]; then \
		echo "❌ Error: Neither Docker nor Podman is installed"; \
		echo "   Please install Docker or Podman to use containerized commands"; \
		echo "   Or use 'make run-native' for native development"; \
		exit 1; \
	fi
	@if [ -z "$(COMPOSE_CMD)" ]; then \
		echo "❌ Error: No compose tool found"; \
		echo "   Please install docker-compose, podman-compose, or use Docker/Podman v2 with compose plugin"; \
		exit 1; \
	fi
endef

help: ## Show this help message
	@echo 'Usage: make [target]'
	@echo ''
	@echo 'Available targets:'
	@awk 'BEGIN {FS = ":.*?## "} /^[a-zA-Z_-]+:.*?## / {printf "  %-20s %s\n", $$1, $$2}' $(MAKEFILE_LIST)

# Clean targets
clean: clean-docker clean-native ## Clean everything (Docker containers and native artifacts)

clean-docker: ## Stop and remove all Docker containers and volumes
	$(check_docker)
	@echo "🧹 Cleaning Docker resources..."
	$(COMPOSE_CMD) down -v --remove-orphans
	@echo "✅ Docker cleanup complete"

clean-native: ## Clean native build artifacts
	@echo "🧹 Cleaning native artifacts..."
	@echo "Cleaning API..."
	@cd apps/api && dotnet clean && rm -rf */bin */obj */Migrations 2>/dev/null || true
	@echo "Cleaning Web..."
	@cd apps/web && rm -rf node_modules dist .vite 2>/dev/null || true
	@echo "✅ Native cleanup complete"

# Format targets
format: format-api format-web ## Format all source code

format-api: ## Format .NET code
	@echo "🎨 Formatting API code..."
	@cd apps/api && dotnet format
	@echo "✅ API formatting complete"

format-web: ## Format web code
	@echo "🎨 Formatting web code..."
	@cd apps/web && npm run lint -- --fix 2>/dev/null || echo "⚠️  Linting skipped (no lint script)"
	@echo "✅ Web formatting complete"

# Build targets
build: build-docker ## Build everything (default: Docker)

build-docker: ## Build Docker images
	$(check_docker)
	@echo "🏗️  Building Docker images..."
	$(COMPOSE_CMD) build --no-cache
	@echo "✅ Docker build complete"

build-native: ## Build applications natively
	@echo "🏗️  Building API..."
	@cd apps/api && dotnet restore && dotnet build
	@echo "🏗️  Building Web..."
	@cd apps/web && npm install && npm run build
	@echo "✅ Native build complete"

# Run targets
run: ## Run the whole infrastructure with Docker
	$(check_docker)
	@echo "🚀 Starting infrastructure with Docker..."
	$(COMPOSE_CMD) up -d
	@echo "✅ Infrastructure started"
	@echo ""
	@echo "📍 Services available at:"
	@echo "   - Web:      http://localhost:${WEB_PORT}"
	@echo "   - API:      http://localhost:${API_PORT}"
	@echo "   - Postgres: localhost:${DB_PORT}"
	@echo ""
	@echo "💡 Run 'make logs' to view logs"
	@echo "💡 Run 'make stop' to stop all services"

run-native: ## Run applications natively (requires PostgreSQL running)
	@echo "🚀 Starting native development..."
	@echo ""
	@echo "⚠️  Make sure PostgreSQL is running on localhost:${DB_PORT}"
	@echo ""
	@echo "Starting API in background..."
	@cd apps/api/Afina.Api && dotnet run > /tmp/afina-api.log 2>&1 & echo $$! > /tmp/afina-api.pid
	@sleep 3
	@echo "Starting Web dev server in background..."
	@cd apps/web && npm run dev > /tmp/afina-web.log 2>&1 & echo $$! > /tmp/afina-web.pid
	@sleep 2
	@echo ""
	@echo "✅ Native services started"
	@echo ""
	@echo "📍 Services available at:"
	@echo "   - Web:      http://localhost:${WEB_PORT}"
	@echo "   - API:      http://localhost:${API_PORT}"
	@echo ""
	@echo "💡 Logs:"
	@echo "   - API: tail -f /tmp/afina-api.log"
	@echo "   - Web: tail -f /tmp/afina-web.log"
	@echo ""
	@echo "💡 To stop: make stop-native"

stop: ## Stop Docker services
	$(check_docker)
	@echo "🛑 Stopping Docker services..."
	$(COMPOSE_CMD) down
	@echo "✅ Services stopped"

stop-native: ## Stop native services
	@echo "🛑 Stopping native services..."
	@if [ -f /tmp/afina-api.pid ]; then kill $$(cat /tmp/afina-api.pid) 2>/dev/null || true; rm /tmp/afina-api.pid; fi
	@if [ -f /tmp/afina-web.pid ]; then kill $$(cat /tmp/afina-web.pid) 2>/dev/null || true; rm /tmp/afina-web.pid; fi
	@echo "✅ Native services stopped"

logs: ## View Docker logs
	$(check_docker)
	$(COMPOSE_CMD) logs -f

logs-api: ## View API logs (Docker)
	$(check_docker)
	$(COMPOSE_CMD) logs -f api

logs-web: ## View Web logs (Docker)
	$(check_docker)
	$(COMPOSE_CMD) logs -f web

logs-db: ## View PostgreSQL logs (Docker)
	$(check_docker)
	$(COMPOSE_CMD) logs -f postgres

# Migration targets
migrate: ## Run database migrations using dedicated container
	$(check_docker)
	@echo "🗄️  Running database migrations..."
	$(COMPOSE_CMD) run --rm migrate
	@echo "✅ Migrations complete"

migrate-create: ## Create a new migration (usage: make migrate-create NAME=MigrationName)
	$(check_docker)
	@if [ -z "$(NAME)" ]; then \
		echo "❌ Error: Migration name required"; \
		echo "   Usage: make migrate-create NAME=MigrationName"; \
		exit 1; \
	fi
	@echo "📝 Creating migration: $(NAME)"
	$(COMPOSE_CMD) run --rm --entrypoint "dotnet ef migrations add $(NAME) --project Afina.Api" migrate
	@echo "✅ Migration created"

migrate-endpoint: ## Trigger migrations via API endpoint (Development only)
	@echo "🗄️  Triggering migrations via API endpoint..."
	@curl -X POST http://localhost:5100/api/migrate -H "Content-Type: application/json" -s | python3 -m json.tool || echo "❌ Failed to connect. Is the API running? (make run)"

migrate-native: ## Run database migrations (native)
	@echo "🗄️  Running migrations..."
	@cd apps/api && dotnet ef database update --project Afina.Api
	@echo "✅ Migrations complete"

# Development helpers
dev-api: ## Run API in development mode (native)
	@cd apps/api/Afina.Api && dotnet watch run

dev-web: ## Run Web in development mode (native)
	@cd apps/web && npm run dev

shell-api: ## Open shell in API container
	$(check_docker)
	$(COMPOSE_CMD) exec api sh

shell-db: ## Open PostgreSQL shell
	$(check_docker)
	$(COMPOSE_CMD) exec postgres psql -U postgres -d afina_db

restart: ## Restart all Docker services
	$(check_docker)
	@echo "🔄 Restarting services..."
	$(COMPOSE_CMD) restart
	@echo "✅ Services restarted"

ps: ## Show running containers
	$(check_docker)
	$(COMPOSE_CMD) ps
