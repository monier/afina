using System;
using Afina.Infrastructure.Mediator;

namespace Afina.Modules.Users.Features.ExportUserData;

public sealed class ExportUserDataRequest : IRequest<ExportUserDataResponse>
{
    public Guid UserId { get; init; }
}
