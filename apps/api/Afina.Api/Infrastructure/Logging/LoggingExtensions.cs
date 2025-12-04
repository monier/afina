using Serilog;

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

        // Build and set Serilog as the logger
        Log.Logger = loggerConfiguration.CreateLogger();

        builder.Host.UseSerilog();

        return builder;
    }
}
