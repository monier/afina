# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY Afina.sln ./
COPY Afina.Api/Afina.Api.csproj Afina.Api/
COPY Afina.Data/Afina.Data.csproj Afina.Data/
COPY Afina.Shared/Afina.Shared.csproj Afina.Shared/
COPY Afina.Modules.Identity/Afina.Modules.Identity.csproj Afina.Modules.Identity/
COPY Afina.Modules.Tenant/Afina.Modules.Tenant.csproj Afina.Modules.Tenant/
COPY Afina.Modules.Vault/Afina.Modules.Vault.csproj Afina.Modules.Vault/
COPY Afina.Modules.Audit/Afina.Modules.Audit.csproj Afina.Modules.Audit/
COPY Afina.Modules.Encryption/Afina.Modules.Encryption.csproj Afina.Modules.Encryption/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY . .

# Build and publish
WORKDIR /src/Afina.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "Afina.Api.dll"]
