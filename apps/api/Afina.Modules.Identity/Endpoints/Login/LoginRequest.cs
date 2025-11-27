namespace Afina.Modules.Identity.Endpoints.Login;

public record LoginRequest(
    string Username,
    string AuthHash
);
