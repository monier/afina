# Logging

## Quick Start

```bash
# Enable Grafana logging in .env
COMPOSE_PROFILES=observability
LOGGING_PROVIDER=Grafana

# Start services
make run

# Access Grafana UI at http://localhost:3001
```

## Configuration

Configure logging via `.env`:

```bash
# Enable Grafana + Loki services
COMPOSE_PROFILES=observability

# Logging destination
LOGGING_PROVIDER=Grafana         # Options: Grafana, StdOut

# Loki connection
GRAFANA_LOKI_ENDPOINT=http://loki:3100
GRAFANA_SERVICE_NAME=afina-api
GRAFANA_SERVICE_VERSION=1.0.0
```

## Usage

Use standard ILogger interface for structured logging:

```csharp
public class MyHandler : IHandler<MyRequest, MyResponse>
{
    private readonly ILogger<MyHandler> _logger;

    public async Task<MyResponse> HandleAsync(MyRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Processing {RequestType} for {UserId}",
            nameof(MyRequest), request.UserId);

        return response;
    }
}
```

## Best Practices

**Use structured logging:**

```csharp
_logger.LogInformation("User {UserId} logged in", userId);
```

**Include context:**

```csharp
_logger.LogError(ex, "Failed to process order {OrderId}", orderId);
```

**Avoid:**

- String interpolation: `$"User {id}"`
- Logging sensitive data (passwords, tokens)
- Excessive logging in tight loops

## Architecture

- **Interface**: ILogger<T> (Microsoft.Extensions.Logging)
- **Implementation**: Serilog structured logging
- **Sinks**: Console (always) + Grafana Loki (when enabled)
- **Transport**: HTTP API to Loki endpoint
- **Labels**: app, environment, version

## Services

| Service    | URL                   | Purpose           |
| ---------- | --------------------- | ----------------- |
| Grafana UI | http://localhost:3001 | Log visualization |
| Loki API   | http://localhost:3100 | Log aggregation   |

## Troubleshooting

**No logs in Grafana?**

```bash
# Check .env configuration
cat .env | grep LOGGING_PROVIDER

# Verify Loki is running
docker compose ps loki

# View Loki logs
docker compose logs loki

# Restart API
make restart
```

**Toggle logging:**

```bash
# Enable Grafana
COMPOSE_PROFILES=observability
LOGGING_PROVIDER=Grafana

# Disable (console only)
COMPOSE_PROFILES=
LOGGING_PROVIDER=StdOut

# Apply changes
make restart
```

**Reduce log volume:**

```json
// appsettings.json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning"
    }
  }
}
```
