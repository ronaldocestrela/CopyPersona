using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PersonaScript.Modules.Identity.Application.Abstractions;

namespace PersonaScript.Modules.Identity.Infrastructure.Emails;

public sealed class ResendEmailSender : IEmailSender
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly ILogger<ResendEmailSender> _logger;

    public ResendEmailSender(HttpClient httpClient, IConfiguration configuration, ILogger<ResendEmailSender> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["Resend:ApiKey"] ?? string.Empty;
        _fromEmail = configuration["Resend:FromEmail"] ?? "PersonaScript AI <onboarding@resend.dev>";
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string name, CancellationToken cancellationToken = default)
    {
        var subject = "Bem-vindo ao PersonaScript AI!";
        var htmlContent = $@"
            <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;"">
                <h2>Olá, {name}!</h2>
                <p>Seja muito bem-vindo ao <strong>PersonaScript AI</strong>.</p>
                <p>Sua conta foi criada com sucesso. Agora você tem acesso aos nossos agentes de inteligência artificial para otimizar seu posicionamento e criação de conteúdo.</p>
                <br/>
                <p>Atenciosamente,<br/>Equipe PersonaScript AI</p>
            </div>";

        await SendEmailAsync(toEmail, subject, htmlContent, cancellationToken);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default)
    {
        var subject = "Redefinição de Senha — PersonaScript AI";
        var htmlContent = $@"
            <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;"">
                <h2>Recuperação de Senha</h2>
                <p>Recebemos uma solicitação para redefinir a senha da sua conta PersonaScript AI.</p>
                <p>Para criar uma nova senha, clique no botão abaixo ou copie e cole o link no seu navegador:</p>
                <p style=""margin: 24px 0;"">
                    <a href=""{resetLink}"" style=""background-color: #6366f1; color: white; padding: 12px 24px; border-radius: 6px; text-decoration: none; font-weight: bold;"">Redefinir Senha</a>
                </p>
                <p><small>Link: <a href=""{resetLink}"">{resetLink}</a></small></p>
                <p>Se você não solicitou a alteração de senha, por favor ignore este e-mail.</p>
                <br/>
                <p>Atenciosamente,<br/>Equipe PersonaScript AI</p>
            </div>";

        await SendEmailAsync(toEmail, subject, htmlContent, cancellationToken);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlContent, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("Resend ApiKey não configurada. E-mail para {ToEmail} não foi enviado via API.", toEmail);
            return;
        }

        var payload = new ResendEmailRequest
        {
            From = _fromEmail,
            To = [toEmail],
            Subject = subject,
            Html = htmlContent
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        try
        {
            var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Falha ao enviar e-mail via Resend para {ToEmail}. Status: {StatusCode}. Resposta: {ErrorBody}",
                    toEmail, response.StatusCode, errorBody);
            }
            else
            {
                _logger.LogInformation("E-mail '{Subject}' enviado com sucesso via Resend para {ToEmail}", subject, toEmail);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro de comunicação com a API do Resend ao enviar e-mail para {ToEmail}", toEmail);
        }
    }

    private sealed class ResendEmailRequest
    {
        [JsonPropertyName("from")]
        public string From { get; set; } = string.Empty;

        [JsonPropertyName("to")]
        public string[] To { get; set; } = [];

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("html")]
        public string Html { get; set; } = string.Empty;
    }
}
