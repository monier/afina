namespace Afina.Modules.Identity.DTOs;

public record LoginRequest(
    string Username,
    string AuthHash
);

public record LoginResponse(
    Guid UserId,
    string Username,
    string Message
);
