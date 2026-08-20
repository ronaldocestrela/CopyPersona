using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.Modules.Scripts.Application.DTOs;

namespace PersonaScript.Modules.Scripts.Application.Commands.GenerateContentPlan;

public sealed record GenerateContentPlanCommand : ICommand<ContentPlanResultDto>, IQuotaProtectedCommand
{
    public QuotaResourceType QuotaResource => QuotaResourceType.AiAnalysis;
}

