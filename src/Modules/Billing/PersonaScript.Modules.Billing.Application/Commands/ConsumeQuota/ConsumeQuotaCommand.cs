using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Billing.Domain;

namespace PersonaScript.Modules.Billing.Application.Commands.ConsumeQuota;

public record ConsumeQuotaCommand(
    QuotaResourceType ResourceType,
    int Quantity = 1,
    string? SourceCommand = null) : ICommand<Guid>;

public sealed class ConsumeQuotaCommandHandler(
    ITenantContext tenantContext,
    IUsageQuotaRepository quotaRepository,
    IQuotaTransactionRepository transactionRepository)
    : ICommandHandler<ConsumeQuotaCommand, Guid>
{
    public async Task<Result<Guid>> Handle(ConsumeQuotaCommand command, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId.Value;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<Guid>(Error.Unauthorized("Billing.TenantIdInvalid", "Tenant não autenticado."));
        }

        var quota = await quotaRepository.GetByTenantIdAsync(tenantId, cancellationToken);
        if (quota == null)
        {
            return Result.Failure<Guid>(DomainErrors.UsageQuota.NotFound);
        }

        Result<QuotaTransaction> consumeResult = command.ResourceType switch
        {
            QuotaResourceType.ScriptGeneration => quota.ConsumeScript(command.SourceCommand, command.Quantity),
            QuotaResourceType.AiAnalysis => quota.ConsumeAiAnalysis(command.SourceCommand, command.Quantity),
            _ => Result.Failure<QuotaTransaction>(Error.Validation("Billing.UnsupportedResource", "Tipo de recurso não suportado para consumo direto."))
        };

        if (consumeResult.IsFailure)
        {
            return Result.Failure<Guid>(consumeResult.Error);
        }

        quotaRepository.Update(quota);
        await transactionRepository.AddAsync(consumeResult.Value, cancellationToken);

        return Result.Success(consumeResult.Value.Id);
    }
}
