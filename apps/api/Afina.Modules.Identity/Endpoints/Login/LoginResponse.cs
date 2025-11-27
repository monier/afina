namespace Afina.Modules.Identity.Endpoints.Login;

public record LoginResponse(
    Guid UserId,
    string Username,
    string Message
);
