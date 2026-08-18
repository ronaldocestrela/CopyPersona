namespace PersonaScript.Modules.Identity.Application.Abstractions;

public interface IEmailSender
{
    Task SendWelcomeEmailAsync(string toEmail, string name, CancellationToken cancellationToken = default);

    Task SendPasswordResetEmailAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default);
}
