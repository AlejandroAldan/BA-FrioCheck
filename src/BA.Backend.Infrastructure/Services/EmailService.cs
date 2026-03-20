using BA.Backend.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace BA.Backend.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendPasswordResetEmailAsync(
        string email,
        string resetLink,
        string userFullName,
        CancellationToken cancellationToken = default)
    {
        var emailSettings = _configuration.GetSection("Email");
        var host = emailSettings["Host"] ?? "smtp.gmail.com";
        var port = int.Parse(emailSettings["Port"] ?? "587");
        var username = emailSettings["Username"] ?? "";
        var password = emailSettings["Password"] ?? "";
        var fromName = emailSettings["FromName"] ?? "BA Backend";

        var subject = "Recuperar contraseña - BA Backend";
        var body = GeneratePasswordResetEmailBody(userFullName, resetLink);

        await SendEmailAsync(email, subject, body, host, port, username, password, fromName, cancellationToken);
    }

    public async Task SendWelcomeEmailAsync(
        string email,
        string userName,
        CancellationToken cancellationToken = default)
    {
        var emailSettings = _configuration.GetSection("Email");
        var host = emailSettings["Host"] ?? "smtp.gmail.com";
        var port = int.Parse(emailSettings["Port"] ?? "587");
        var username = emailSettings["Username"] ?? "";
        var password = emailSettings["Password"] ?? "";
        var fromName = emailSettings["FromName"] ?? "BA Backend";

        var subject = "Bienvenido a BA Backend";
        var body = GenerateWelcomeEmailBody(userName);

        await SendEmailAsync(email, subject, body, host, port, username, password, fromName, cancellationToken);
    }

    private async Task SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        string host,
        int port,
        string username,
        string password,
        string fromName,
        CancellationToken cancellationToken)
    {
        try
        {
            using (var smtpClient = new SmtpClient(host, port))
            {
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential(username, password);
                smtpClient.Timeout = 10000;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(username, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            // Log the error - in production, use proper logging
            Console.WriteLine($"Error sending email to {toEmail}: {ex.Message}");
            throw;
        }
    }

    private string GeneratePasswordResetEmailBody(string userFullName, string resetLink)
    {
        return $@"
<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .header {{ color: #333; text-align: center; margin-bottom: 20px; }}
        .content {{ color: #666; line-height: 1.6; }}
        .button {{ display: inline-block; background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px; margin-top: 20px; }}
        .footer {{ margin-top: 20px; font-size: 12px; color: #999; text-align: center; }}
    </style>
</head>
<body>
    <div class=""container"">
        <h1 class=""header"">Recuperar Contraseña</h1>
        <div class=""content"">
            <p>Hola {userFullName},</p>
            <p>Has solicitado recuperar tu contraseña. Haz clic en el enlace a continuación para establecer una nueva contraseña. Este enlace es válido por 15 minutos.</p>
            <a href=""{resetLink}"" class=""button"">Recuperar Contraseña</a>
            <p>Si no solicitaste este cambio, ignora este correo.</p>
            <p>Saludos,<br>El equipo de BA Backend</p>
        </div>
        <div class=""footer"">
            <p>&copy; 2026 BA Backend. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateWelcomeEmailBody(string userName)
    {
        return $@"
<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .header {{ color: #333; text-align: center; margin-bottom: 20px; }}
        .content {{ color: #666; line-height: 1.6; }}
        .footer {{ margin-top: 20px; font-size: 12px; color: #999; text-align: center; }}
    </style>
</head>
<body>
    <div class=""container"">
        <h1 class=""header"">¡Bienvenido a BA Backend!</h1>
        <div class=""content"">
            <p>Hola {userName},</p>
            <p>Tu cuenta ha sido creada exitosamente. Ya estás listo para comenzar.</p>
            <p>Si tienes alguna pregunta, no dudes en contactarnos.</p>
            <p>Saludos,<br>El equipo de BA Backend</p>
        </div>
        <div class=""footer"">
            <p>&copy; 2026 BA Backend. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
    }
}
