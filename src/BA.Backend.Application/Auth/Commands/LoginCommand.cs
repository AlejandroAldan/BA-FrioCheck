using BA.Backend.Application.Auth.DTOs;
using MediatR;

namespace BA.Backend.Application.Auth.Commands;

public record LoginCommand(
    string Email,
    string Password,
    string TenantSlug,
    string DeviceFingerprint
) : IRequest<LoginResponseDto>;
