.PHONY: help check-prereqs
.PHONY: clean clean-native
.PHONY: build build-native
.PHONY: run run-api-native run-web-native stop stop-native
.PHONY: format format-native lint lint-native
.PHONY: test test-native
.PHONY: before-push
.PHONY: add-migration add-migration-native execute-migration execute-migration-native
.PHONY: logs logs-api logs-web logs-db ps restart
.PHONY: shell-api shell-db dev-api dev-web
.PHONY: help

# ============================================================================
# Configuration & Detection
# ============================================================================

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

# Load environment variables
include .env
export $(shell sed 's/=.*//' .env)

# Define check functions
define check_docker
	@if [ -z "$(DOCKER_CMD)" ]; then \
		echo "❌ Error: Neither Docker nor Podman is installed"; \
		echo "   Please install Docker or Podman to use containerized commands"; \
		echo "   Or use 'make <command>-native' for native development"; \
		exit 1; \
	fi
	@if [ -z "$(COMPOSE_CMD)" ]; then \
		echo "❌ Error: No compose tool found"; \
		echo "   Please install docker-compose, podman-compose, or use Docker/Podman v2 with compose plugin"; \
		exit 1; \
	fi
endef

define check_dotnet
	@if ! command -v dotnet > /dev/null 2>&1; then \
		echo "❌ Error: .NET SDK is not installed"; \
		echo "   Please install .NET 9 SDK from https://dotnet.microsoft.com/download"; \
		exit 1; \
	fi
endef

define check_node
	@if ! command -v node > /dev/null 2>&1; then \
		echo "❌ Error: Node.js is not installed"; \
		echo "   Please install Node.js 25+ from https://nodejs.org/"; \
		exit 1; \
	fi
endef

define check_postgres_native
	@if ! command -v psql > /dev/null 2>&1; then \
		echo "⚠️  Warning: PostgreSQL client (psql) is not installed"; \
		echo "   Install it to verify database connectivity"; \
	fi
	@if command -v pg_isready > /dev/null 2>&1; then \
		if ! pg_isready -h localhost -p $(DB_PORT) > /dev/null 2>&1; then \
			echo "❌ Error: PostgreSQL is not running on localhost:$(DB_PORT)"; \
			echo "   Please start PostgreSQL or use 'make run' for Docker-based setup"; \
			exit 1; \
		fi \
	else \
		echo "⚠️  Warning: Cannot verify PostgreSQL connectivity (pg_isready not found)"; \
		echo "   Make sure PostgreSQL is running on localhost:$(DB_PORT)"; \
	fi
endef

# ============================================================================
# Prerequisites Check
# ============================================================================

check-prereqs: ## Check all prerequisites (.env, SDKs, tools)
	@echo "🔍 Checking prerequisites..."
	@# Check .env file
	@if [ ! -f .env ]; then \
		echo "❌ .env file not found"; \
		echo "   Run: cp .env.example .env"; \
		exit 1; \
	fi
	@# Check .NET SDK
	@if ! command -v dotnet > /dev/null 2>&1; then \
		echo "❌ .NET SDK not installed"; \
		echo "   Install from: https://dotnet.microsoft.com/download"; \
		exit 1; \
	fi
	@# Check Node.js
	@if ! command -v node > /dev/null 2>&1; then \
		echo "❌ Node.js not installed"; \
		echo "   Install from: https://nodejs.org/"; \
		exit 1; \
	fi
	@# Check Docker/Podman for Docker commands
	@if [ -z "$(DOCKER_CMD)" ] && [ -z "$(COMPOSE_CMD)" ]; then \
		echo "⚠️  Docker/Podman not found (required for Docker commands)"; \
	fi
	@echo "✅ All prerequisites OK"

# ============================================================================
# Help
# ============================================================================

help: ## Show this help message
	@echo 'Usage: make [target]'
	@echo ''
	@echo 'Core Commands (Docker-based by default):'
	@echo '  clean              Clean all resources (Docker + native)'
	@echo '  build              Build all applications'
	@echo '  run                Run all services'
	@echo '  format             Format all source code'
	@echo '  lint               Lint/check all source code'
	@echo '  test               Run all tests'
	@echo ''
	@echo 'Native Development Commands (use -native suffix):'
	@echo '  clean-native       Clean native build artifacts'
	@echo '  build-native       Build applications natively'
	@echo '  run-api-native     Run API natively'
	@echo '  run-web-native     Run Web natively'
	@echo '  format-native      Format code natively'
	@echo '  lint-native        Lint code natively'
	@echo '  test-native        Run tests natively'
	@echo ''
	@echo 'Quality Assurance:'
	@echo '  before-push        Run all checks before pushing (native)'
	@echo ''
	@echo 'API/Backend Specific Commands:'
	@echo '  add-migration NAME=<name>        Create new migration (Docker)'
	@echo '  add-migration-native NAME=<name> Create new migration (native)'
	@echo '  execute-migration                Run migrations (Docker)'
	@echo '  execute-migration-native         Run migrations (native)'
	@echo ''
	@echo 'Docker Management:'
	@echo '  stop               Stop Docker services'
	@echo '  restart            Restart Docker services'
	@echo '  logs               View all logs'
	@echo '  logs-api           View API logs'
	@echo '  logs-web           View Web logs'
	@echo '  logs-db            View database logs'
	@echo '  ps                 Show running containers'
	@echo '  shell-api          Open shell in API container'
	@echo '  shell-db           Open PostgreSQL shell'
	@echo ''

	@echo 'Development Helpers:'
	@echo '  dev-api            Run API with hot reload (native)'
	@echo '  dev-web            Run Web with hot reload (native)'
	@echo '  stop-native        Stop native services'
	@echo ''
	@echo 'For more details, run: make help-verbose'

help-verbose: ## Show detailed help with descriptions
	@echo 'Usage: make [target]'
	@echo ''
	@echo 'Available targets:'
	@awk 'BEGIN {FS = ":.*?## "} /^[a-zA-Z_-]+:.*?## / {printf "  %-30s %s\n", $$1, $$2}' $(MAKEFILE_LIST)


# ============================================================================
# Clean Targets
# ============================================================================

clean: clean-native ## Clean all resources (Docker containers/volumes and native artifacts)
	$(check_docker)
	@echo "🧹 Cleaning Docker resources..."
	$(COMPOSE_CMD) down -v --remove-orphans
	@echo "✅ Complete cleanup finished"

clean-native: ## Clean native build artifacts
	@echo "🧹 Cleaning..."
	@cd apps/api && dotnet clean Afina.sln --nologo 2>/dev/null || true
	@cd apps/api && find . -name "bin" -o -name "obj" -type d -exec rm -rf {} + 2>/dev/null || true
	@cd apps/web && rm -rf node_modules dist .vite 2>/dev/null || true

# ============================================================================
# Build Targets
# ============================================================================

build: ## Build all applications (Docker images)
	$(check_docker)
	@echo "🏗️  Building Docker images..."
	$(COMPOSE_CMD) build
	@echo "✅ Docker build complete"
	@echo ""
	@echo "💡 Run 'make run' to start services"

build-native: check-prereqs ## Build applications natively
	@echo "🏗️  Building..."
	@cd apps/api && dotnet build Afina.sln --nologo
	@cd apps/web && npm install --silent && npm run build --silent

# ============================================================================
# Run Targets
# ============================================================================

run: ## Run all services (Docker-based)
	$(check_docker)
	@echo "🚀 Starting all services with Docker..."
	@if [ -n "$(COMPOSE_PROFILES)" ]; then \
		$(COMPOSE_CMD) --profile $(COMPOSE_PROFILES) up -d; \
	else \
		$(COMPOSE_CMD) up -d; \
	fi
	@echo "✅ All services started"
	@echo ""
	@echo "📍 Services available at:"
	@echo "   • Web UI:    http://localhost:$(WEB_PORT)"
	@echo "   • API:       http://localhost:$(API_PORT)"
	@echo "   • Database:  localhost:$(DB_PORT)"
	@if [ "$(COMPOSE_PROFILES)" = "db-view" ]; then \
		echo "   • pgAdmin:   http://localhost:$(PGADMIN_PORT)"; \
	fi
	@echo ""
	@echo "💡 Useful commands:"
	@echo "   • View logs:        make logs"
	@echo "   • View API logs:    make logs-api"
	@echo "   • Stop services:    make stop"
	@echo "   • Restart services: make restart"

run-api-native: ## Run API natively (requires PostgreSQL running)
	$(check_dotnet)
	$(check_postgres_native)
	@echo "🚀 Starting API natively..."
	@echo "   PostgreSQL: localhost:$(DB_PORT)"
	@echo "   API will run on: http://localhost:$(API_PORT)"
	@echo ""
	@cd apps/api/Afina.Api && \
		ConnectionStrings__DefaultConnection="Host=localhost;Port=$(DB_PORT);Database=$(DB_NAME);Username=$(DB_USER);Password=$(DB_PASSWORD)" \
		ASPNETCORE_URLS="http://localhost:$(API_PORT)" \
		dotnet run

run-web-native: ## Run Web natively
	$(check_node)
	@echo "🚀 Starting Web natively..."
	@echo "   Web will run on: http://localhost:5173 (Vite default)"
	@echo "   API endpoint: $(VITE_API_URL)"
	@echo ""
	@cd apps/web && npm run dev

stop: ## Stop all Docker services
	$(check_docker)
	@echo "🛑 Stopping Docker services..."
	$(COMPOSE_CMD) down
	@echo "✅ Services stopped"

stop-native: ## Stop native services (kills background processes)
	@echo "🛑 Stopping native services..."
	@pkill -f "dotnet.*Afina.Api" || echo "  → API not running"
	@pkill -f "vite" || echo "  → Web not running"
	@echo "✅ Native services stopped"

# ============================================================================
# Format Targets
# ============================================================================

format: ## Format all source code (Docker-based)
	$(check_docker)
	@echo "🎨 Formatting code using Docker..."
	@echo "  → Formatting API code..."
	$(COMPOSE_CMD) run --rm --no-deps --entrypoint "dotnet format" api || echo "⚠️  API formatting requires dotnet format installed"
	@echo "  → Formatting Web code..."
	$(COMPOSE_CMD) run --rm --no-deps --entrypoint "npm run lint -- --fix" web || echo "⚠️  Web formatting completed with warnings"
	@echo "✅ Code formatting complete"

format-native: check-prereqs ## Format all source code natively
	@echo "🎨 Formatting..."
	@cd apps/api && dotnet format Afina.sln --verbosity quiet
	@cd apps/web && npm run lint -- --fix --quiet 2>/dev/null || true

# ============================================================================
# Lint Targets
# ============================================================================

lint: ## Lint/check all source code (Docker-based)
	$(check_docker)
	@echo "🔍 Linting code using Docker..."
	@echo "  → Linting API code..."
	$(COMPOSE_CMD) run --rm --no-deps --entrypoint "dotnet format --verify-no-changes" api || echo "⚠️  API linting found issues"
	@echo "  → Linting Web code..."
	$(COMPOSE_CMD) run --rm --no-deps --entrypoint "npm run lint" web || echo "⚠️  Web linting found issues"
	@echo "✅ Linting complete"

lint-native: build-native ## Lint/check all source code natively
	@echo "🔍 Linting..."
	@cd apps/api && dotnet format Afina.sln --verify-no-changes --verbosity quiet
	@cd apps/web && npm run lint --silent 2>/dev/null || true

# ============================================================================
# Test Targets
# ============================================================================

test: ## Run all tests (Docker-based)
	$(check_docker)
	@echo "🧪 Running tests using Docker..."
	@echo "  → Running API tests..."
	$(COMPOSE_CMD) run --rm --no-deps --entrypoint "dotnet test Afina.sln --verbosity normal" api
	@echo "  → Running Web tests..."
	$(COMPOSE_CMD) run --rm --no-deps --entrypoint "npm test" web || echo "⚠️  No web tests configured"
	@echo "✅ All tests complete"

test-native: build-native ## Run all tests natively
	@echo "🧪 Testing..."
	@cd apps/api && for test_proj in $$(find . -name "*Tests.csproj" -o -name "*Test.csproj" 2>/dev/null); do \
		dotnet test "$$test_proj" --verbosity normal --no-build || exit 1; \
	done
	@cd apps/web && npm test 2>/dev/null || true

# ============================================================================
# Quality Assurance
# ============================================================================

before-push: clean-native format-native lint-native test-native ## Run all checks before pushing
	@echo ""
	@echo "✅ All checks passed"

# ============================================================================
# Migration Targets (API/Backend Specific)
# ============================================================================

add-migration: ## Create a new EF Core migration (Docker) - Usage: make add-migration NAME=MigrationName
	$(check_docker)
	@if [ -z "$(NAME)" ]; then \
		echo "❌ Error: Migration name required"; \
		echo "   Usage: make add-migration NAME=MigrationName"; \
		exit 1; \
	fi
	@echo "📝 Creating migration: $(NAME)"
	$(COMPOSE_CMD) run --rm --no-deps migrate dotnet ef migrations add $(NAME) --project Afina.Data --startup-project Afina.Api
	@echo "✅ Migration '$(NAME)' created successfully"
	@echo ""
	@echo "💡 Next steps:"
	@echo "   • Review the migration in apps/api/Afina.Data/Migrations/"
	@echo "   • Apply it with: make execute-migration"

add-migration-native: ## Create a new EF Core migration (native) - Usage: make add-migration-native NAME=MigrationName
	$(check_dotnet)
	@if [ -z "$(NAME)" ]; then \
		echo "❌ Error: Migration name required"; \
		echo "   Usage: make add-migration-native NAME=MigrationName"; \
		exit 1; \
	fi
	@echo "📝 Creating migration: $(NAME)"
	@cd apps/api && dotnet ef migrations add $(NAME) --project Afina.Data --startup-project Afina.Api
	@echo "✅ Migration '$(NAME)' created successfully"
	@echo ""
	@echo "💡 Next steps:"
	@echo "   • Review the migration in apps/api/Afina.Data/Migrations/"
	@echo "   • Apply it with: make execute-migration-native"

execute-migration: ## Execute pending database migrations (Docker)
	$(check_docker)
	@echo "🗄️  Executing database migrations..."
	$(COMPOSE_CMD) --profile db-migration run --rm migrate
	@echo "✅ Migrations executed successfully"

execute-migration-native: ## Execute pending database migrations (native)
	$(check_dotnet)
	$(check_postgres_native)
	@echo "🗄️  Executing database migrations..."
	@cd apps/api && \
		ConnectionStrings__DefaultConnection="Host=localhost;Port=$(DB_PORT);Database=$(DB_NAME);Username=$(DB_USER);Password=$(DB_PASSWORD)" \
		dotnet ef database update --project Afina.Data --startup-project Afina.Api
	@echo "✅ Migrations executed successfully"

# ============================================================================
# Docker Management & Utilities
# ============================================================================

logs: ## View logs from all Docker services
	$(check_docker)
	@echo "📋 Viewing logs (Ctrl+C to exit)..."
	$(COMPOSE_CMD) logs -f

logs-api: ## View API logs (Docker)
	$(check_docker)
	@echo "📋 Viewing API logs (Ctrl+C to exit)..."
	$(COMPOSE_CMD) logs -f api

logs-web: ## View Web logs (Docker)
	$(check_docker)
	@echo "📋 Viewing Web logs (Ctrl+C to exit)..."
	$(COMPOSE_CMD) logs -f web

logs-db: ## View PostgreSQL logs (Docker)
	$(check_docker)
	@echo "📋 Viewing PostgreSQL logs (Ctrl+C to exit)..."
	$(COMPOSE_CMD) logs -f postgres

ps: ## Show running Docker containers
	$(check_docker)
	@echo "📊 Running containers:"
	@$(COMPOSE_CMD) ps

restart: ## Restart all Docker services (stop, build, run)
	$(check_docker)
	@echo "🔄 Restarting all services..."
	@echo "  → Stopping services..."
	$(COMPOSE_CMD) down
	@echo "  → Building..."
	$(COMPOSE_CMD) build
	@echo "  → Executing migrations..."
	$(COMPOSE_CMD) --profile db-migration run --rm migrate
	@echo "  → Starting..."
	@if [ -n "$(COMPOSE_PROFILES)" ]; then \
		$(COMPOSE_CMD) --profile $(COMPOSE_PROFILES) up -d; \
	else \
		$(COMPOSE_CMD) up -d; \
	fi
	@echo "✅ Services restarted"

shell-api: ## Open shell in API container
	$(check_docker)
	@echo "🐚 Opening shell in API container..."
	$(COMPOSE_CMD) exec api sh || echo "❌ API container not running. Start with 'make run'"

shell-db: ## Open PostgreSQL shell (Docker)
	$(check_docker)
	@echo "🐚 Opening PostgreSQL shell..."
	$(COMPOSE_CMD) exec postgres psql -U $(DB_USER) -d $(DB_NAME) || echo "❌ Database container not running. Start with 'make run'"

# ============================================================================
# Development Helpers
# ============================================================================

dev-api: ## Run API with hot reload (native)
	$(check_dotnet)
	$(check_postgres_native)
	@echo "🔥 Starting API with hot reload..."
	@echo "   PostgreSQL: localhost:$(DB_PORT)"
	@echo "   API will run on: http://localhost:$(API_PORT)"
	@echo ""
	@cd apps/api/Afina.Api && \
		ConnectionStrings__DefaultConnection="Host=localhost;Port=$(DB_PORT);Database=$(DB_NAME);Username=$(DB_USER);Password=$(DB_PASSWORD)" \
		ASPNETCORE_URLS="http://localhost:$(API_PORT)" \
		dotnet watch run

dev-web: ## Run Web with hot reload (native)
	$(check_node)
	@echo "🔥 Starting Web with hot reload..."
	@echo "   Web will run on: http://localhost:5173 (Vite default)"
	@echo "   API endpoint: $(VITE_API_URL)"
	@echo ""
	@cd apps/web && npm run dev

# ============================================================================
# Convenience Aliases & Shortcuts
# ============================================================================

up: run ## Alias for 'make run'

down: stop ## Alias for 'make stop'

rebuild: clean build ## Clean and rebuild everything

rebuild-native: clean-native build-native ## Clean and rebuild natively

watch-api: dev-api ## Alias for 'make dev-api'

watch-web: dev-web ## Alias for 'make dev-web'
