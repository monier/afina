namespace Afina.Infrastructure.Mediator;

/// <summary>
/// Represents a void response for handlers that don't return data.
/// </summary>
public readonly struct EmptyResponse
{
    public static EmptyResponse Value => default;
}