using PersonaScript.BuildingBlocks.CQRS;

namespace PersonaScript.Modules.Personas.Application.Commands.GeneratePersonaDiagnosis;

public sealed record GeneratePersonaDiagnosisCommand(string? Feedback = null) : ICommand<Guid>, IQuotaProtectedCommand
{
    public QuotaResourceType QuotaResource => QuotaResourceType.AiAnalysis;
}

