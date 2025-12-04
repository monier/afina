namespace Afina.Core.Api;

public sealed class ApiError
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public ApiError(string code, string message)
    {
        Code = code;
        Message = message;
    }
}
