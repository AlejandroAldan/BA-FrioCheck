using BA.Backend.Application.Users.DTOs;
using MediatR;

namespace BA.Backend.Application.Users.Queries;

public record GetAllUsersQuery(
    Guid TenantId,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PagedResultDto<UserDto>>;
