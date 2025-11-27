namespace Afina.Modules.Identity.DTOs;

public record RegisterRequest(
    string Username,
    string PasswordHash,
    string? PasswordHint
);

public record RegisterResponse(
    Guid UserId,
    Guid IndividualTenantId,
    string Message
);
