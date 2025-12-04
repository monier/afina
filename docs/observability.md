# Logging

## Quick Start

Logs are printed to the console for easy viewing during development and in container logs for Docker deployments.

## Configuration

Logs are output to the console by default in both Docker and native deployments. View logs using:

```bash
# Docker: View all container logs
make logs

# Docker: View API logs specifically
make logs-api

# Native: Check console output in the terminal where the API is running
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
- **Sink**: Console output (always enabled)
- **Format**: Structured logging with timestamps and severity levels

## Viewing Logs

### Docker Deployment

```bash
# View all service logs
make logs

# View API logs only
make logs-api

# View database logs
make logs-db

# Follow logs in real-time (Ctrl+C to exit)
make logs-api
```

### Native Development

```bash
# View console output where the API is running
# Logs appear directly in the terminal

# For background processes:
# Use 'make stop-native' to stop and view final output
```

## Troubleshooting

**No logs appearing?**

```bash
# Docker: Verify services are running
make ps

# Docker: Check service logs
make logs-api

# Native: Ensure API process is still running
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

**Filter logs by category:**

```json
// appsettings.json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```
