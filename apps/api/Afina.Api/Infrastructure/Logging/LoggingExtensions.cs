using Serilog;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;

namespace Afina.Api.Infrastructure.Logging;

public static class LoggingExtensions
{
    public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        // Configure Serilog
        var loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "Afina.Api")
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName);

        // Console logging (always enabled)
        loggerConfiguration.WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");

        var loggingProvider = configuration["Logging:Provider"]?.ToLowerInvariant() ?? "stdout";

        if (loggingProvider is "grafana")
        {
            // Use docker service hostname fallback instead of localhost (container local loopback)
            var endpoint = configuration["Grafana:LokiEndpoint"] ?? "http://loki:3100";
            loggerConfiguration.WriteTo.GrafanaLoki(
                endpoint,
                labels: new[]
                {
                    new Serilog.Sinks.Grafana.Loki.LokiLabel
                    {
                        Key = "app",
                        Value = configuration["Grafana:ServiceName"] ?? "afina-api"
                    },
                    new Serilog.Sinks.Grafana.Loki.LokiLabel
                    {
                        Key = "environment",
                        Value = builder.Environment.EnvironmentName
                    },
                    new Serilog.Sinks.Grafana.Loki.LokiLabel
                    {
                        Key = "version",
                        Value = configuration["Grafana:ServiceVersion"] ?? "1.0.0"
                    }
                });
        }

        // Build and set Serilog as the logger
        Log.Logger = loggerConfiguration.CreateLogger();

        builder.Host.UseSerilog();

        return builder;
    }
}
