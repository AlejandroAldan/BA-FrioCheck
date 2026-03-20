using MediatR;

namespace BA.Backend.Application.Users.Commands;

public record LockUserCommand(
    Guid Id,
    Guid TenantId
) : IRequest<Unit>;
