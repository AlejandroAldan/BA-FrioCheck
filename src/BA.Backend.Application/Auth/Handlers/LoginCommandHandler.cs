using BA.Backend.Application.Auth.Commands;
using BA.Backend.Application.Auth.DTOs;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using MediatR;

namespace BA.Backend.Application.Auth.Handlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly ISessionService _sessionService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IUserSessionRepository userSessionRepository,
        ISessionService sessionService,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _userSessionRepository = userSessionRepository;
        _sessionService = sessionService;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Resolver Tenant por Slug
        var tenant = await _tenantRepository.GetBySlugAsync(request.TenantSlug, cancellationToken);
        if (tenant is null)
            throw new InvalidOperationException("Credenciales inválidas");

        // 2. Buscar Usuario por Email + TenantId
        var user = await _userRepository.GetByEmailAsync(request.Email, tenant.Id, cancellationToken);
        if (user is null)
            throw new InvalidOperationException("Credenciales inválidas");

        // 3. Validar que la cuenta esté activa
        if (!user.IsActive)
            throw new InvalidOperationException("La cuenta no está disponible");

        // 4. Validar contraseña
        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new InvalidOperationException("Credenciales inválidas");

        // 5. Invalidar sesión anterior si existe en otro dispositivo
        var activeSessions = await _userSessionRepository.GetActiveSessionsByUserAsync(user.Id);
        var existingDeviceSession = activeSessions.FirstOrDefault(s => s.DeviceId == request.DeviceFingerprint);
        
        bool sessionReplaced = false;
        if (existingDeviceSession is null && activeSessions.Any())
        {
            // Existe sesión activa en otro dispositivo, la revocamos
            var previousSession = activeSessions.First();
            await _userSessionRepository.InvalidateSessionAsync(previousSession.Id, "Se inició una nueva sesión en otro dispositivo");
            sessionReplaced = true;
        }

        // 6. Generar sessionId
        var sessionId = Guid.NewGuid().ToString();

        // 7. Generar JWT Token
        var (token, expiresAt) = _jwtTokenService.GenerateToken(user, tenant.Id, sessionId);

        // 8. Crear sesión del usuario
        var userSession = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TenantId = tenant.Id,
            DeviceId = request.DeviceFingerprint,
            DeviceFingerprint = request.DeviceFingerprint,
            AccessToken = token,
            JwtToken = token,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _userSessionRepository.CreateSessionAsync(userSession);

        // 9. Registrar sesión en el servicio de sesiones
        await _sessionService.RegisterSessionAsync(sessionId, user.Id, expiresAt, cancellationToken);

        // 10. Determinar redirección según rol
        var redirectTo = GetRedirectUrl(user.Role.ToString());

        // 11. Retornar respuesta
        return new LoginResponseDto(
            AccessToken: token,
            ExpiresAt: expiresAt,
            UserFullName: user.FullName,
            Role: user.Role.ToString(),
            UserId: user.Id,
            TenantId: tenant.Id,
            SessionReplaced: sessionReplaced,
            RedirectTo: redirectTo
        );
    }

    private static string GetRedirectUrl(string role) => role switch
    {
        "Admin" => "/admin/dashboard",
        "Cliente" => "/cliente/dashboard",
        "Transportista" => "/transportista/panel",
        "Tecnico" => "/tecnico/panel",
        _ => "/dashboard"
    };
}
