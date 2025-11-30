using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Afina.Infrastructure.Logging;

/// <summary>
/// Performance monitoring helper for logging operation durations
/// </summary>
public sealed class PerformanceTimer : IDisposable
{
    private readonly ILogger _logger;
    private readonly string _operationName;
    private readonly LogLevel _logLevel;
    private readonly Stopwatch _stopwatch;
    private readonly Dictionary<string, object>? _properties;

    public PerformanceTimer(
        ILogger logger,
        string operationName,
        LogLevel logLevel = LogLevel.Information,
        Dictionary<string, object>? properties = null)
    {
        _logger = logger;
        _operationName = operationName;
        _logLevel = logLevel;
        _properties = properties;
        _stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        _stopwatch.Stop();

        var logProperties = _properties ?? new Dictionary<string, object>();
        logProperties["OperationName"] = _operationName;
        logProperties["ElapsedMilliseconds"] = _stopwatch.ElapsedMilliseconds;

        using (_logger.BeginScope(logProperties))
        {
            _logger.Log(
                _logLevel,
                "Operation {OperationName} completed in {ElapsedMilliseconds}ms",
                _operationName,
                _stopwatch.ElapsedMilliseconds
            );
        }
    }
}

/// <summary>
/// Extension methods for structured logging with performance tracking
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Creates a performance timer that logs operation duration when disposed
    /// </summary>
    public static PerformanceTimer TrackPerformance(
        this ILogger logger,
        string operationName,
        LogLevel logLevel = LogLevel.Information,
        Dictionary<string, object>? properties = null)
    {
        return new PerformanceTimer(logger, operationName, logLevel, properties);
    }

    /// <summary>
    /// Logs with enriched context properties
    /// </summary>
    public static void LogWithContext(
        this ILogger logger,
        LogLevel logLevel,
        string message,
        Dictionary<string, object> context,
        Exception? exception = null)
    {
        using (logger.BeginScope(context))
        {
            if (exception != null)
            {
                logger.Log(logLevel, exception, message);
            }
            else
            {
                logger.Log(logLevel, message);
            }
        }
    }

    /// <summary>
    /// Logs an error with enriched context
    /// </summary>
    public static void LogErrorWithContext(
        this ILogger logger,
        Exception exception,
        string message,
        Dictionary<string, object>? context = null)
    {
        if (context != null)
        {
            using (logger.BeginScope(context))
            {
                logger.LogError(exception, message);
            }
        }
        else
        {
            logger.LogError(exception, message);
        }
    }

    /// <summary>
    /// Logs a warning with enriched context
    /// </summary>
    public static void LogWarningWithContext(
        this ILogger logger,
        string message,
        Dictionary<string, object> context)
    {
        using (logger.BeginScope(context))
        {
            logger.LogWarning(message);
        }
    }

    /// <summary>
    /// Logs information with enriched context
    /// </summary>
    public static void LogInformationWithContext(
        this ILogger logger,
        string message,
        Dictionary<string, object> context)
    {
        using (logger.BeginScope(context))
        {
            logger.LogInformation(message);
        }
    }
}
