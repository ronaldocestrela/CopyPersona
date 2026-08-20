using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.Modules.Billing.Domain;

public static class DomainErrors
{
    public static class Plan
    {
        public static readonly Error InvalidName = Error.Validation(
            "Billing.Plan.InvalidName",
            "O nome do plano é obrigatório e não pode ser vazio.");

        public static readonly Error NotFound = Error.NotFound(
            "Billing.Plan.NotFound",
            "O plano especificado não foi encontrado.");
    }

    public static class Subscription
    {
        public static readonly Error NotFound = Error.NotFound(
            "Billing.Subscription.NotFound",
            "Assinatura não encontrada para o tenant.");

        public static readonly Error Inactive = Error.Validation(
            "Billing.Subscription.Inactive",
            "A assinatura do tenant está inativa ou cancelada.");

        public static readonly Error InvalidStatus = Error.Validation(
            "Billing.Subscription.InvalidStatus",
            "Transição de status inválida para a assinatura.");
    }

    public static class UsageQuota
    {
        public static readonly Error ScriptLimitExceeded = Error.Validation(
            "Billing.UsageQuota.ScriptLimitExceeded",
            "Você atingiu o limite mensal de geração de roteiros do seu plano.");

        public static readonly Error PersonaLimitExceeded = Error.Validation(
            "Billing.UsageQuota.PersonaLimitExceeded",
            "Você atingiu o limite de personas ativas permitidas no seu plano.");

        public static readonly Error AiAnalysisLimitExceeded = Error.Validation(
            "Billing.UsageQuota.AiAnalysisLimitExceeded",
            "Você atingiu o limite mensal de análises de Inteligência Artificial.");

        public static readonly Error NotFound = Error.NotFound(
            "Billing.UsageQuota.NotFound",
            "Cota de consumo não encontrada para a assinatura.");
    }
}
