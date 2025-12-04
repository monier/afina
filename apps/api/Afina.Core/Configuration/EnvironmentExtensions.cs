using Microsoft.AspNetCore.Hosting;

namespace Afina.Core.Configuration;

/// <summary>
/// Extension methods for environment management.
/// </summary>
public static class EnvironmentExtensions
{
    /// <summary>
    /// Gets the current application environment from the IWebHostEnvironment.
    /// </summary>
    /// <param name="environment">The web host environment.</param>
    /// <returns>The AppEnvironment enum value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the environment name is not recognized.</exception>
    public static AppEnvironment GetAppEnvironment(this IWebHostEnvironment environment)
    {
        return environment.EnvironmentName.ToLower() switch
        {
            "dev" => AppEnvironment.Dev,
            "test" => AppEnvironment.Test,
            "sandbox" => AppEnvironment.Sandbox,
            "prod" => AppEnvironment.Prod,
            _ => throw new InvalidOperationException($"Unknown environment: {environment.EnvironmentName}")
        };
    }

    /// <summary>
    /// Checks if the current environment is Development.
    /// </summary>
    public static bool IsDevEnvironment(this IWebHostEnvironment environment)
    {
        return environment.EnvironmentName.Equals("dev", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the current environment is Test.
    /// </summary>
    public static bool IsTestEnvironment(this IWebHostEnvironment environment)
    {
        return environment.EnvironmentName.Equals("test", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the current environment is Sandbox.
    /// </summary>
    public static bool IsSandboxEnvironment(this IWebHostEnvironment environment)
    {
        return environment.EnvironmentName.Equals("sandbox", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the current environment is Production.
    /// </summary>
    public static bool IsProdEnvironment(this IWebHostEnvironment environment)
    {
        return environment.EnvironmentName.Equals("prod", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the current environment is not Production (i.e., dev, test, or sandbox).
    /// </summary>
    public static bool IsNotProduction(this IWebHostEnvironment environment)
    {
        return !environment.IsProdEnvironment();
    }

    /// <summary>
    /// Gets the environment name as a string.
    /// </summary>
    public static string GetEnvironmentName(this AppEnvironment environment)
    {
        return environment switch
        {
            AppEnvironment.Dev => "dev",
            AppEnvironment.Test => "test",
            AppEnvironment.Sandbox => "sandbox",
            AppEnvironment.Prod => "prod",
            _ => throw new ArgumentOutOfRangeException(nameof(environment), environment, null)
        };
    }
}
