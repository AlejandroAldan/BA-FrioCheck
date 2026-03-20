using BA.Backend.Application.Users.Commands;
using BA.Backend.Application.Users.DTOs;
using BA.Backend.Application.Exceptions;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Enums;
using BA.Backend.Domain.Repositories;
using MediatR;

namespace BA.Backend.Application.Users.Handlers;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar que tenant existe
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
            throw new InvalidOperationException("Tenant no existe");

        // 2. Validar que email no existe en este tenant
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, request.TenantId, cancellationToken);
        if (existingUser != null)
            throw new InvalidOperationException("El email ya está registrado en este tenant");

        // 3. Hashear password
        var passwordHash = _passwordHasher.Hash(request.Password);

        // 4. Crear usuario
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = passwordHash,
            Role = request.Role,
            IsActive = true,
            IsLocked = false,
            CreatedAt = DateTime.UtcNow
        };

        // 5. Guardar
        await _userRepository.AddAsync(user, cancellationToken);

        // 6. Retornar DTO
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
