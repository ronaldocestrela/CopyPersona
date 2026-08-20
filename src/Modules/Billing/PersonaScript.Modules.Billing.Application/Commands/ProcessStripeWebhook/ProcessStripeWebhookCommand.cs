using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Billing.Domain;

namespace PersonaScript.Modules.Billing.Application.Commands.ProcessStripeWebhook;

public record ProcessStripeWebhookCommand(
    string EventId,
    string EventType,
    string StripeCustomerId,
    string? StripeSubscriptionId = null,
    string? StripePriceId = null,
    DateTime? PeriodStart = null,
    DateTime? PeriodEnd = null,
    Guid? TenantIdMetadata = null) : ICommand;

public class ProcessStripeWebhookCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    IPlanRepository planRepository,
    IUsageQuotaRepository usageQuotaRepository,
    IProcessedStripeEventRepository processedEventRepository) : ICommandHandler<ProcessStripeWebhookCommand>
{
    public async Task<Result> Handle(ProcessStripeWebhookCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.EventId))
        {
            return Result.Failure(Error.Validation("ProcessStripeWebhook.EventIdRequired", "O Id do evento é obrigatório."));
        }

        // 1. Verificação de Idempotência
        var existingEvent = await processedEventRepository.GetByIdAsync(command.EventId, cancellationToken);
        if (existingEvent is not null)
        {
            // Evento já processado de forma idempotente
            return Result.Success();
        }

        // 2. Busca Assinatura local correspondente (via TenantId metadata, StripeSubscriptionId ou StripeCustomerId)
        Subscription? subscription = null;
        if (command.TenantIdMetadata.HasValue && command.TenantIdMetadata.Value != Guid.Empty)
        {
            subscription = await subscriptionRepository.GetByTenantIdAsync(command.TenantIdMetadata.Value, cancellationToken);
        }

        if (subscription is null && !string.IsNullOrWhiteSpace(command.StripeSubscriptionId))
        {
            subscription = await subscriptionRepository.GetByStripeSubscriptionIdAsync(command.StripeSubscriptionId, cancellationToken);
        }

        Guid? tenantId = subscription?.TenantId ?? command.TenantIdMetadata;

        // 3. Processamento conforme o evento do Stripe
        switch (command.EventType)
        {
            case "customer.subscription.created":
            case "customer.subscription.updated":
                if (subscription is not null)
                {
                    Plan? plan = null;
                    if (!string.IsNullOrWhiteSpace(command.StripePriceId))
                    {
                        var allActivePlans = await planRepository.GetAllActiveAsync(cancellationToken);
                        plan = allActivePlans.FirstOrDefault(p => p.StripePriceId == command.StripePriceId);
                    }

                    plan ??= await planRepository.GetByIdAsync(subscription.PlanId, cancellationToken);

                    var pStart = command.PeriodStart ?? DateTime.UtcNow;
                    var pEnd = command.PeriodEnd ?? pStart.AddMonths(1);

                    if (plan is not null && plan.Id != subscription.PlanId)
                    {
                        subscription.ChangePlan(plan.Id, pStart, pEnd);
                    }

                    subscription.Activate(
                        command.StripeCustomerId,
                        command.StripeSubscriptionId ?? subscription.StripeSubscriptionId ?? string.Empty,
                        pStart,
                        pEnd);

                    subscriptionRepository.Update(subscription);

                    // Sincroniza / Atualiza cota de consumo
                    var usageQuota = await usageQuotaRepository.GetBySubscriptionIdAsync(subscription.Id, cancellationToken);
                    if (usageQuota is not null && plan is not null)
                    {
                        usageQuota.ResetMonthlyQuota(pStart, pEnd, plan.MaxScriptsPerMonth, plan.MaxActivePersonas, plan.MaxAiAnalysesPerMonth);
                        usageQuotaRepository.Update(usageQuota);
                    }
                }
                break;

            case "customer.subscription.deleted":
                if (subscription is not null)
                {
                    subscription.Cancel(immediate: true);
                    subscriptionRepository.Update(subscription);
                }
                break;

            case "invoice.payment_succeeded":
                if (subscription is not null)
                {
                    var pStart = command.PeriodStart ?? subscription.CurrentPeriodStart;
                    var pEnd = command.PeriodEnd ?? subscription.CurrentPeriodEnd;

                    subscription.Activate(
                        command.StripeCustomerId,
                        command.StripeSubscriptionId ?? subscription.StripeSubscriptionId ?? string.Empty,
                        pStart,
                        pEnd);

                    subscriptionRepository.Update(subscription);

                    var usageQuota = await usageQuotaRepository.GetBySubscriptionIdAsync(subscription.Id, cancellationToken);
                    var plan = await planRepository.GetByIdAsync(subscription.PlanId, cancellationToken);
                    if (usageQuota is not null && plan is not null)
                    {
                        usageQuota.ResetMonthlyQuota(pStart, pEnd, plan.MaxScriptsPerMonth, plan.MaxActivePersonas, plan.MaxAiAnalysesPerMonth);
                        usageQuotaRepository.Update(usageQuota);
                    }
                }
                break;

            case "invoice.payment_failed":
                if (subscription is not null)
                {
                    subscription.MarkPastDue();
                    subscriptionRepository.Update(subscription);
                }
                break;

            default:
                break;
        }

        // 4. Registra idempotência do evento
        var processedEventResult = ProcessedStripeEvent.Create(command.EventId, command.EventType, tenantId);
        if (processedEventResult.IsSuccess)
        {
            await processedEventRepository.AddAsync(processedEventResult.Value, cancellationToken);
        }

        return Result.Success();
    }
}
