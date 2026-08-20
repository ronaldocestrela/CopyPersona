using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Application.DTOs;
using PersonaScript.Modules.Backoffice.Domain.Repositories;

namespace PersonaScript.Modules.Backoffice.Application.Queries.Prompts;

public record GetPromptTemplatesQuery() : IQuery<IReadOnlyList<PromptTemplateDto>>;

public sealed class GetPromptTemplatesQueryHandler : IQueryHandler<GetPromptTemplatesQuery, IReadOnlyList<PromptTemplateDto>>
{
    private readonly IPromptTemplateRepository _promptRepository;

    public GetPromptTemplatesQueryHandler(IPromptTemplateRepository promptRepository)
    {
        _promptRepository = promptRepository;
    }

    public async Task<Result<IReadOnlyList<PromptTemplateDto>>> Handle(GetPromptTemplatesQuery query, CancellationToken cancellationToken)
    {
        var activePrompts = await _promptRepository.GetAllActivePromptsAsync(cancellationToken);
        
        var dtos = activePrompts.Select(p => new PromptTemplateDto(
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
