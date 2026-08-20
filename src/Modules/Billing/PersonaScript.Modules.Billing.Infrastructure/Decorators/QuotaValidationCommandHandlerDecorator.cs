using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Billing.Domain;

namespace PersonaScript.Modules.Billing.Infrastructure.Decorators;

public sealed class QuotaValidationCommandHandlerDecorator<TCommand, TResponse>(
    ICommandHandler<TCommand, TResponse> innerHandler,
    ITenantContext tenantContext,
    IUsageQuotaRepository quotaRepository,
    IQuotaTransactionRepository transactionRepository)
    : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>, IQuotaProtectedCommand
{
    public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId.Value;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<TResponse>(Error.Unauthorized("Billing.TenantIdInvalid", "Tenant não autenticado."));
        }

        var quota = await quotaRepository.GetByTenantIdAsync(tenantId, cancellationToken);
        if (quota == null)
        {
            return Result.Failure<TResponse>(DomainErrors.UsageQuota.NotFound);
        }

        Result checkResult = command.QuotaResource switch
        {
            QuotaResourceType.ScriptGeneration => quota.CanConsumeScript(command.QuotaQuantity),
            QuotaResourceType.AiAnalysis => quota.CanConsumeAiAnalysis(command.QuotaQuantity),
            QuotaResourceType.PersonaCreation => quota.CanCreatePersona(quota.ActivePersonasCount),
            _ => Result.Failure(Error.Validation("Billing.UnsupportedResource", "Tipo de recurso não suportado para validação de quota."))
        };

        if (checkResult.IsFailure)
        {
            return Result.Failure<TResponse>(checkResult.Error);
        }

        var result = await innerHandler.Handle(command, cancellationToken);
        if (result.IsFailure)
        {
            return result;
        }

        Result<QuotaTransaction> consumeResult = command.QuotaResource switch
        {
            QuotaResourceType.ScriptGeneration => quota.ConsumeScript(command.GetType().Name, command.QuotaQuantity),
            QuotaResourceType.AiAnalysis => quota.ConsumeAiAnalysis(command.GetType().Name, command.QuotaQuantity),
            _ => Result.Success<QuotaTransaction>(null!)
        };

        if (consumeResult.IsSuccess && consumeResult.Value != null)
        {
            quotaRepository.Update(quota);
            await transactionRepository.AddAsync(consumeResult.Value, cancellationToken);
        }

        return result;
    }
}
