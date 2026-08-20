using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Domain;
using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.Modules.Billing.Domain;


public class UsageQuota : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public int ScriptsGeneratedCount { get; private set; }
    public int ActivePersonasCount { get; private set; }
    public int AiAnalysesCount { get; private set; }
    public int ScriptsLimit { get; private set; }
    public int ActivePersonasLimit { get; private set; }
    public int AiAnalysesLimit { get; private set; }
    public DateTime LastResetAt { get; private set; }

    private UsageQuota() { }

    public void SetTenantId(Guid tenantId)
    {
        TenantId = tenantId;
    }

    public static Result<UsageQuota> Create(
        Guid tenantId,
        Guid subscriptionId,
        DateTime periodStart,
        DateTime periodEnd,
        int scriptsLimit,
        int personasLimit,
        int aiAnalysesLimit)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<UsageQuota>(Error.Validation("UsageQuota.TenantIdRequired", "O TenantId é obrigatório."));
        }

        var now = DateTime.UtcNow;
        var quota = new UsageQuota
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SubscriptionId = subscriptionId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            ScriptsGeneratedCount = 0,
            ActivePersonasCount = 0,
            AiAnalysesCount = 0,
            ScriptsLimit = scriptsLimit,
            ActivePersonasLimit = personasLimit,
            AiAnalysesLimit = aiAnalysesLimit,
            LastResetAt = now
        };

        return Result.Success(quota);
    }

    public Result CanConsumeScript(int count = 1)
    {
        if (ScriptsGeneratedCount + count > ScriptsLimit)
        {
            return Result.Failure(DomainErrors.UsageQuota.ScriptLimitExceeded);
        }
        return Result.Success();
    }

    public Result<QuotaTransaction> ConsumeScript(string? sourceCommand = null, int count = 1)
    {
        var checkResult = CanConsumeScript(count);
        if (checkResult.IsFailure)
        {
            return Result.Failure<QuotaTransaction>(checkResult.Error);
        }

        ScriptsGeneratedCount += count;
        var transaction = QuotaTransaction.Record(
            TenantId,
            Id,
            QuotaResourceType.ScriptGeneration,
            count,
            $"Geração de {count} roteiro(s)",
            sourceCommand);

        return Result.Success(transaction);
    }

    public Result CanConsumeAiAnalysis(int count = 1)
    {
        if (AiAnalysesCount + count > AiAnalysesLimit)
        {
            return Result.Failure(DomainErrors.UsageQuota.AiAnalysisLimitExceeded);
        }
        return Result.Success();
    }

    public Result<QuotaTransaction> ConsumeAiAnalysis(string? sourceCommand = null, int count = 1)
    {
        var checkResult = CanConsumeAiAnalysis(count);
        if (checkResult.IsFailure)
        {
            return Result.Failure<QuotaTransaction>(checkResult.Error);
        }

        AiAnalysesCount += count;
        var transaction = QuotaTransaction.Record(
            TenantId,
            Id,
            QuotaResourceType.AiAnalysis,
            count,
            $"Execução de {count} análise(s) de IA",
            sourceCommand);

        return Result.Success(transaction);
    }

    public Result CanCreatePersona(int currentActivePersonasCount)
    {
        if (currentActivePersonasCount >= ActivePersonasLimit)
        {
            return Result.Failure(DomainErrors.UsageQuota.PersonaLimitExceeded);
        }
        return Result.Success();
    }

    public Result ResetMonthlyQuota(DateTime newPeriodStart, DateTime newPeriodEnd, int scriptsLimit, int personasLimit, int aiAnalysesLimit)
    {
        PeriodStart = newPeriodStart;
        PeriodEnd = newPeriodEnd;
        ScriptsGeneratedCount = 0;
        AiAnalysesCount = 0;
        ScriptsLimit = scriptsLimit;
        ActivePersonasLimit = personasLimit;
        AiAnalysesLimit = aiAnalysesLimit;
        LastResetAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result GrantExtraCredits(int extraScripts, int extraAiAnalyses, string reason)
    {
        if (extraScripts < 0 || extraAiAnalyses < 0 || (extraScripts == 0 && extraAiAnalyses == 0))
        {
            return Result.Failure(Error.Validation("UsageQuota.InvalidExtraCredits", "Informe um valor positivo para os créditos adicionais."));
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure(Error.Validation("UsageQuota.ReasonRequired", "O motivo da concessão de créditos é obrigatório."));
        }

        ScriptsLimit += extraScripts;
        AiAnalysesLimit += extraAiAnalyses;

        return Result.Success();
    }

    public Result OverrideLimits(int scriptsLimit, int personasLimit, int aiAnalysesLimit, string reason)
    {
        if (scriptsLimit < 0 || personasLimit < 0 || aiAnalysesLimit < 0)
        {
            return Result.Failure(Error.Validation("UsageQuota.InvalidLimits", "Os limites da quota não podem ser negativos."));
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure(Error.Validation("UsageQuota.ReasonRequired", "O motivo da alteração de quota é obrigatório."));
        }

        ScriptsLimit = scriptsLimit;
        ActivePersonasLimit = personasLimit;
        AiAnalysesLimit = aiAnalysesLimit;
        LastResetAt = DateTime.UtcNow;

        return Result.Success();
    }
}

