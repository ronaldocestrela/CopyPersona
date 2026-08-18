using Microsoft.Extensions.Logging;
using PersonaScript.Modules.Identity.Application.Abstractions;

namespace PersonaScript.Modules.Identity.Infrastructure.Emails;

public sealed class FakeEmailSender(ILogger<FakeEmailSender> logger) : IEmailSender
{
    public List<(string ToEmail, string Subject, string Body)> SentEmails { get; } = [];

    public Task SendWelcomeEmailAsync(string toEmail, string name, CancellationToken cancellationToken = default)
    {
        var subject = "Bem-vindo ao PersonaScript AI!";
        var body = $"Welcome {name} ({toEmail})";
        SentEmails.Add((toEmail, subject, body));
        logger.LogInformation("[FakeEmailSender] Boas-vindas registradas para {ToEmail}", toEmail);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default)
    {
        var subject = "Redefinição de Senha — PersonaScript AI";
        var body = $"Reset link: {resetLink}";
        SentEmails.Add((toEmail, subject, body));
        logger.LogInformation("[FakeEmailSender] Reset de senha registrado para {ToEmail}: {ResetLink}", toEmail, resetLink);
        return Task.CompletedTask;
    }
}
