using BA.Backend.Application.Auth.Commands;
using BA.Backend.Application.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace BA.Backend.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Autentica un usuario y genera un token JWT.
    /// Este es el ÚNICO endpoint público de autenticación.
    /// </summary>
    /// <param name="email">Correo electrónico del usuario</param>
    /// <param name="password">Contraseña del usuario</param>
    /// <param name="tenantSlug">Identificador único del tenant (empresa/organización)</param>
    /// <remarks>
    /// Ejemplo de request:
    /// POST /api/v1/auth/login
    /// {
    ///   "email": "usuario@empresa.com",
    ///   "password": "micontraseña123",
    ///   "tenantSlug": "empresa-xyz"
    /// }
    /// 
    /// Respuesta exitosa (200):
    /// {
    ///   "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    ///   "expiresAt": "2026-03-20T10:30:00Z",
    ///   "userFullName": "Juan Pérez",
    ///   "role": "Cliente",
    ///   "userId": "550e8400-e29b-41d4-a716-446655440000",
    ///   "tenantId": "550e8400-e29b-41d4-a716-446655440001",
    ///   "sessionReplaced": false,
    ///   "redirectTo": "/cliente/dashboard"
    /// }
    /// </remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest(new ErrorResponse { Message = "El cuerpo de la solicitud es requerido" });

        // Calcular DeviceFingerprint a partir de User-Agent y Accept-Language
        var userAgent = Request.Headers.UserAgent.FirstOrDefault() ?? "unknown";
        var acceptLanguage = Request.Headers.AcceptLanguage.FirstOrDefault() ?? "unknown";
        var deviceFingerprint = ComputeDeviceFingerprint(userAgent, acceptLanguage);

        var command = new LoginCommand(
            Email: request.Email?.Trim() ?? string.Empty,
            Password: request.Password ?? string.Empty,
            TenantSlug: request.TenantSlug?.Trim() ?? string.Empty,
            DeviceFingerprint: deviceFingerprint
        );

        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Intento de login fallido: {ex.Message}");
            return Unauthorized(new ErrorResponse { Message = "Credenciales inválidas" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado durante el login");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ErrorResponse { Message = "Error al procesando la solicitud de autenticación" });
        }
    }

    /// <summary>
    /// Calcula el fingerprint del dispositivo usando SHA256.
    /// Combinación de User-Agent + Accept-Language
    /// </summary>
    private static string ComputeDeviceFingerprint(string userAgent, string acceptLanguage)
    {
        var combined = $"{userAgent}:{acceptLanguage}";
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}

/// <summary>
/// Request DTO para login
/// </summary>
public record LoginRequestDto(
    string Email,
    string Password,
    string TenantSlug
);

/// <summary>
/// Response DTO para errores
/// </summary>
public record ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string? Code { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}
