namespace PersonaScript.Modules.Billing.Domain;

public enum PlanType
{
    Basic = 1,
    Pro = 2,
    Reference = 3
}

public enum SubscriptionStatus
{
    Trialing = 1,
    Active = 2,
    PastDue = 3,
    Canceled = 4
}

public enum QuotaResourceType
{
    ScriptGeneration = 1,
    PersonaCreation = 2,
    AiAnalysis = 3
}
