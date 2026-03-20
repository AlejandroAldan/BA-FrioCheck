using MediatR;

namespace BA.Backend.Application.Users.Commands;

public record DeleteUserCommand(
    Guid Id,
    Guid TenantId
) : IRequest<Unit>;
