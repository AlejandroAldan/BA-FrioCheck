using BA.Backend.Application.Auth.Commands;
using FluentValidation;

namespace BA.Backend.Application.Auth.Validators;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El correo electrónico es requerido")
            .EmailAddress()
            .WithMessage("El formato del correo electrónico es inválido");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("La contraseña es requerida")
            .MinimumLength(8)
            .WithMessage("La contraseña debe tener al menos 8 caracteres");

        RuleFor(x => x.TenantSlug)
            .NotEmpty()
            .WithMessage("El tenant es requerido");

        RuleFor(x => x.DeviceFingerprint)
            .NotEmpty()
            .WithMessage("El dispositivo no se pudo identificar");
    }
}
