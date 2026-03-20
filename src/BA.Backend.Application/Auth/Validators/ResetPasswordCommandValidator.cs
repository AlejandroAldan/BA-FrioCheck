using BA.Backend.Application.Auth.Commands;
using FluentValidation;

namespace BA.Backend.Application.Auth.Validators;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token es requerido");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Nueva contraseña es requerida")
            .MinimumLength(8).WithMessage("Contraseña debe tener al menos 8 caracteres")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
            .WithMessage("Contraseña debe contener mayúscula, minúscula, número y carácter especial");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirmación de contraseña es requerida")
            .Equal(x => x.NewPassword).WithMessage("Las contraseñas no coinciden");
    }
}
