using BA.Backend.Application.Auth.DTOs;
using MediatR;

namespace BA.Backend.Application.Auth.Commands;

public record ForgotPasswordCommand(
    string Email,
    string TenantSlug
) : IRequest<ForgotPasswordResponseDto>;
