using BA.Backend.Application.Auth.Commands;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace BA.Backend.Application.Auth.Handlers;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IPasswordResetTokenRepository _tokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(
        IPasswordResetTokenRepository tokenRepository,
        IUserRepository userRepository,
        IUserSessionRepository sessionRepository,
        IPasswordHasher passwordHasher)
    {
        _tokenRepository = tokenRepository;
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // 1. Buscar todos los tokens del usuario (por verificación)
        var token = await _tokenRepository.GetByTokenAsync(request.Token, cancellationToken);
        if (token == null)
            throw new InvalidOperationException("Token inválido o expirado");

        // 2. Validar que el token no ha sido usado
        if (token.IsUsed)
            throw new InvalidOperationException("Este enlace de reset ya ha sido utilizado");

        // 3. Validar que el token no ha expirado
        if (DateTime.UtcNow > token.ExpiresAt)
            throw new InvalidOperationException("El enlace de reset ha expirado");

        // 4. Obtener el usuario
        var user = await _userRepository.GetByIdAsync(token.UserId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException("Usuario no encontrado");

        // 5. Hash nueva contraseña
        var newPasswordHash = _passwordHasher.Hash(request.NewPassword);

        // 6. Actualizar contraseña del usuario
        user.PasswordHash = newPasswordHash;
        await _userRepository.UpdateAsync(user, cancellationToken);

        // 7. Marcar token como usado
        token.IsUsed = true;
        await _tokenRepository.UpdateAsync(token, cancellationToken);

        // 8. Revocar TODAS las sesiones activas del usuario
        // (por seguridad, debe hacer login de nuevo con nueva contraseña)
        if (!string.IsNullOrEmpty(user.ActiveSessionId))
        {
            await _sessionRepository.InvalidateSessionAsync(Guid.Parse(user.ActiveSessionId), "Contraseña reseteada");
            user.ActiveSessionId = null;
            user.CurrentDeviceFingerprint = null;
            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        return Unit.Value;
    }
}
