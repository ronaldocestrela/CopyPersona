using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Application.DTOs;
using PersonaScript.Modules.Backoffice.Domain.Repositories;

namespace PersonaScript.Modules.Backoffice.Application.Queries.Prompts;

public record GetPromptHistoryQuery(string AgentName) : IQuery<IReadOnlyList<PromptTemplateDto>>;

public sealed class GetPromptHistoryQueryHandler : IQueryHandler<GetPromptHistoryQuery, IReadOnlyList<PromptTemplateDto>>
{
    private readonly IPromptTemplateRepository _promptRepository;

    public GetPromptHistoryQueryHandler(IPromptTemplateRepository promptRepository)
    {
        _promptRepository = promptRepository;
    }

    public async Task<Result<IReadOnlyList<PromptTemplateDto>>> Handle(GetPromptHistoryQuery query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.AgentName))
        {
            return Result.Failure<IReadOnlyList<PromptTemplateDto>>(Error.Validation("GetPromptHistory.AgentNameRequired", "O nome do agente é obrigatório."));
        }

        var versions = await _promptRepository.GetAllVersionsByAgentNameAsync(query.AgentName, cancellationToken);
        
        var dtos = versions.Select(p => new PromptTemplateDto(
            p.Id,
            p.AgentName,
            p.Version,
            p.SystemPrompt,
            p.UserPromptTemplate,
            p.IsActive,
            p.ParametersJson,
            p.Description,
            p.CreatedByAdminEmail,
            p.CreatedAt)).ToList();

        return Result.Success<IReadOnlyList<PromptTemplateDto>>(dtos);
    }
}
