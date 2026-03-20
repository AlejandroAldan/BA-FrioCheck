using BA.Backend.Application.Users.DTOs;
using BA.Backend.Domain.Enums;
using MediatR;

namespace BA.Backend.Application.Users.Commands;

public record UpdateUserCommand(
    Guid Id,
    string FullName,
    UserRole Role,
    bool IsActive,
    Guid TenantId
) : IRequest<UserDto>;
