namespace Afina.Core.Configuration;

/// <summary>
/// Defines the accepted application environments.
/// </summary>
public enum AppEnvironment
{
    /// <summary>
    /// Development environment - local development with full debugging capabilities.
    /// </summary>
    Dev,

    /// <summary>
    /// Test environment - for unit tests and integration tests.
    /// </summary>
    Test,

    /// <summary>
    /// Sandbox environment - staging environment for pre-production testing.
    /// </summary>
    Sandbox,

    /// <summary>
    /// Production environment - live production deployment.
    /// </summary>
    Prod
}
