namespace Afina.Modules.Identity.Endpoints.Register;

public record RegisterRequest(
    string Username,
    string PasswordHash,
    string? PasswordHint
);
