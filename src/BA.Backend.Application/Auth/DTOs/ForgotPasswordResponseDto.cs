namespace BA.Backend.Application.Auth.DTOs;

public class ForgotPasswordResponseDto
{
    public string Message { get; set; } = "Si el email existe en nuestro sistema, recibirás un enlace para restablecer tu contraseña.";
}
