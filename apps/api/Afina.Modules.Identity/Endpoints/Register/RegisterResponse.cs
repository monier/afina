namespace Afina.Modules.Identity.Endpoints.Register;

public record RegisterResponse(
    Guid UserId,
    Guid IndividualTenantId,
    string Message
);
