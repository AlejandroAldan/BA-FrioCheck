using BA.Backend.Application.Users.DTOs;
using BA.Backend.Application.Users.Queries;
using BA.Backend.Domain.Repositories;
using BA.Backend.Domain.Enums;
using MediatR;

namespace BA.Backend.Application.Users.Handlers;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Obtener usuario
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        
        // 2. Validar existencia y tenant match
        if (user == null || user.TenantId != request.TenantId)
            return null;

        // 3. Mapear a DTO
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            IsActive = user.IsActive,
            IsLocked = user.IsLocked,
            LastLoginAt = user.LastLoginAt
        };
    }
}
