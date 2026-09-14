namespace PersonaScript.Server.Observability.Alerts;

public enum AlertChannelType
{
    Slack,
    Teams
}

public sealed class AlertOptions
{
    public const string SectionName = "Alerts";

    public bool Enabled { get; set; } = true;
    public string? WebhookUrl { get; set; }
    public AlertChannelType ChannelType { get; set; } = AlertChannelType.Slack;
    public string EnvironmentName { get; set; } = "Development";
}
