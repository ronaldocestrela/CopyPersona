using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PersonaScript.Modules.Billing.Domain;

namespace PersonaScript.Modules.Billing.Infrastructure.BackgroundServices;

public class MonthlyQuotaResetBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<MonthlyQuotaResetBackgroundService> logger,
    TimeSpan? checkInterval = null) : BackgroundService
{
    private readonly TimeSpan _checkInterval = checkInterval ?? TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Serviço de reset mensal de quotas iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessQuotaResetsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro no processamento do reset mensal de quotas.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    public async Task<int> ProcessQuotaResetsAsync(CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var quotaRepository = scope.ServiceProvider.GetRequiredService<IUsageQuotaRepository>();
        var subscriptionRepository = scope.ServiceProvider.GetRequiredService<ISubscriptionRepository>();
        var planRepository = scope.ServiceProvider.GetRequiredService<IPlanRepository>();

        var expiredQuotas = await quotaRepository.GetExpiredQuotasAsync(DateTime.UtcNow, cancellationToken);
        if (expiredQuotas.Count == 0)
        {
            return 0;
        }

        int resetCount = 0;
        foreach (var quota in expiredQuotas)
        {
            try
            {
                Plan? plan = null;
                var subscription = await subscriptionRepository.GetByTenantIdAsync(quota.TenantId, cancellationToken);
                if (subscription != null)
                {
                    plan = await planRepository.GetByIdAsync(subscription.PlanId, cancellationToken);
                }

                if (plan == null)
                {
                    plan = await planRepository.GetByTypeAsync(PlanType.Basic, cancellationToken);
                }

                int scriptsLimit = plan?.MaxScriptsPerMonth ?? quota.ScriptsLimit;
                int personasLimit = plan?.MaxActivePersonas ?? quota.ActivePersonasLimit;
                int aiAnalysesLimit = plan?.MaxAiAnalysesPerMonth ?? quota.AiAnalysesLimit;



                var newStart = DateTime.UtcNow;
                var newEnd = newStart.AddMonths(1);

                quota.ResetMonthlyQuota(newStart, newEnd, scriptsLimit, personasLimit, aiAnalysesLimit);
                quotaRepository.Update(quota);
                resetCount++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha ao resetar quota do tenant {TenantId}.", quota.TenantId);
            }
        }

        logger.LogInformation("Reset mensal concluído para {ResetCount} quota(s).", resetCount);
        return resetCount;
    }
}
