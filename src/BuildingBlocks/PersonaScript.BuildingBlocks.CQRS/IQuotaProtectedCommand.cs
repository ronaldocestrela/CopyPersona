namespace PersonaScript.BuildingBlocks.CQRS;

public enum QuotaResourceType
{
    ScriptGeneration = 1,
    PersonaCreation = 2,
    AiAnalysis = 3
}

public interface IQuotaProtectedCommand
{
    QuotaResourceType QuotaResource { get; }
    int QuotaQuantity => 1;
}
