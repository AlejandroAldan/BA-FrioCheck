using MediatR;

namespace BA.Backend.Application.Users.Commands;

public record UnlockUserCommand(
    Guid Id,
    Guid TenantId
) : IRequest<Unit>;
