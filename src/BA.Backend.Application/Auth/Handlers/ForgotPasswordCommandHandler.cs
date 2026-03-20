using BA.Backend.Application.Auth.Commands;
using BA.Backend.Application.Auth.DTOs;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace BA.Backend.Application.Auth.Handlers;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponseDto>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _tokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public ForgotPasswordCommandHandler(
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IPasswordResetTokenRepository tokenRepository,
        IPasswordHasher passwordHasher,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<ForgotPasswordResponseDto> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // 1. Resolver Tenant
        var tenant = await _tenantRepository.GetBySlugAsync(request.TenantSlug, cancellationToken);
        if (tenant == null)
        {
            // NO revelar que el tenant no existe
            return new ForgotPasswordResponseDto();
        }

        // 2. Buscar usuario por email
        var user = await _userRepository.GetByEmailAsync(request.Email, tenant.Id, cancellationToken);
        if (user == null)
        {
            // NO revelar que el usuario no existe - respuesta genérica siempre
            return new ForgotPasswordResponseDto();
        }

        // 3. Generar token de reset
        var resetToken = Guid.NewGuid().ToString("N");
        var tokenHash = _passwordHasher.Hash(resetToken);

        // 4. Crear registro de token
        var passwordResetToken = new Domain.Entities.PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        // 5. Guardar token en BD
        await _tokenRepository.AddAsync(passwordResetToken, cancellationToken);

        // 6. Construir link de reset
        var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:3000";
        var resetLink = $"{frontendUrl}/auth/reset-password?token={resetToken}&email={user.Email}";

        // 7. Enviar email
        try
        {
            await _emailService.SendPasswordResetEmailAsync(
                user.Email,
                resetLink,
                user.FullName,
                cancellationToken);
        }
        catch
        {
            // Log del error pero no revelar al usuario
            // En producción, usar Serilog o similar
        }

        // 8. Respuesta genérica siempre
        return new ForgotPasswordResponseDto();
    }
}
