using MediatR;

namespace BA.Backend.Application.Auth.Commands;

public record ResetPasswordCommand(
    string Token,
    string NewPassword,
    string ConfirmPassword
) : IRequest<Unit>;
