using BA.Backend.Application.Users.DTOs;
using BA.Backend.Domain.Enums;
using MediatR;

namespace BA.Backend.Application.Users.Commands;

public record CreateUserCommand(
    string Email,
    string FullName,
    string Password,
    UserRole Role,
    Guid TenantId
) : IRequest<UserDto>;
