namespace PersonaScript.Server.Services;

public interface IQuotaNotifierService
{
    event Action<string, string>? OnQuotaExceeded;
    void NotifyQuotaExceeded(string resourceName, string message);
}

public sealed class QuotaNotifierService : IQuotaNotifierService
{
    public event Action<string, string>? OnQuotaExceeded;

    public void NotifyQuotaExceeded(string resourceName, string message)
    {
        OnQuotaExceeded?.Invoke(resourceName, message);
    }
}
