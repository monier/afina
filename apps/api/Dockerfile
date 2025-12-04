# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY Afina.sln ./
COPY Afina.ApiApp/Afina.ApiApp.csproj Afina.ApiApp/
COPY Afina.Data/Afina.Data.csproj Afina.Data/
COPY Afina.Core/Afina.Core.csproj Afina.Core/
COPY Afina.Infrastructure/Afina.Infrastructure.csproj Afina.Infrastructure/
COPY Afina.Contracts/Afina.Contracts.csproj Afina.Contracts/
COPY Afina.Modules.Tenant/Afina.Modules.Tenant.csproj Afina.Modules.Tenant/
COPY Afina.Modules.Vault/Afina.Modules.Vault.csproj Afina.Modules.Vault/
COPY Afina.Modules.Audit/Afina.Modules.Audit.csproj Afina.Modules.Audit/
COPY Afina.Modules.Encryption/Afina.Modules.Encryption.csproj Afina.Modules.Encryption/
COPY Afina.Modules.Users/Afina.Modules.Users.csproj Afina.Modules.Users/
COPY Afina.Modules.Users.Tests/Afina.Modules.Users.Tests.csproj Afina.Modules.Users.Tests/
COPY Afina.MigrationRunner/Afina.MigrationRunner.csproj Afina.MigrationRunner/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY . .

# Build and publish
WORKDIR /src/Afina.ApiApp
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "Afina.ApiApp.dll"]
