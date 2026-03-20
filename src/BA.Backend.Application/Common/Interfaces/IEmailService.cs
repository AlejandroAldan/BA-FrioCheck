namespace BA.Backend.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string resetLink, string userFullName, CancellationToken cancellationToken = default);
    Task SendWelcomeEmailAsync(string email, string userName, CancellationToken cancellationToken = default);
}
