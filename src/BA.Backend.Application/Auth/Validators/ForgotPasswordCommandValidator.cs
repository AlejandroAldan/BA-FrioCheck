using BA.Backend.Application.Auth.Commands;
using FluentValidation;

namespace BA.Backend.Application.Auth.Validators;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email es requerido")
            .EmailAddress().WithMessage("Email debe ser válido");

        RuleFor(x => x.TenantSlug)
            .NotEmpty().WithMessage("TenantSlug es requerido");
    }
}
