namespace PersonaScript.Server.Observability.Alerts;

public record PaymentWebhookFailureAlert(
    string EventType,
    string? CustomerId,
    string? ErrorCode,
    string ErrorMessage,
    DateTime TimestampUtc);

public record LLMFailureAlert(
    string ProviderType,
    string Model,
    string? ErrorCode,
    string ErrorMessage,
    long LatencyMs,
    bool AllProvidersFailed,
    DateTime TimestampUtc);

public record SystemCriticalAlert(
    string Component,
    string Title,
    string Message,
    string? StackTrace,
    DateTime TimestampUtc);

public interface IOperationalAlertService
{
    Task<bool> NotifyPaymentWebhookFailureAsync(PaymentWebhookFailureAlert alert, CancellationToken cancellationToken = default);
    Task<bool> NotifyLLMQuotaOrFailureAsync(LLMFailureAlert alert, CancellationToken cancellationToken = default);
    Task<bool> NotifySystemCriticalErrorAsync(SystemCriticalAlert alert, CancellationToken cancellationToken = default);
}
