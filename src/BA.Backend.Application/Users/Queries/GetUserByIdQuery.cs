using BA.Backend.Application.Users.DTOs;
using MediatR;

namespace BA.Backend.Application.Users.Queries;

public record GetUserByIdQuery(
    Guid Id,
    Guid TenantId
) : IRequest<UserDto?>;
