namespace PersonaScript.Modules.Billing.Application.Options;

public class StripeOptions
{
    public const string SectionName = "Stripe";

    public string ApiKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string SuccessUrl { get; set; } = "http://localhost:5000/assinatura/sucesso";
    public string CancelUrl { get; set; } = "http://localhost:5000/assinatura/cancelado";
    public string ReturnUrl { get; set; } = "http://localhost:5000/minha-conta/assinatura";

    public string BasicPriceId { get; set; } = string.Empty;
    public string ProPriceId { get; set; } = string.Empty;
    public string ReferencePriceId { get; set; } = string.Empty;
}
