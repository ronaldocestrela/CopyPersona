using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Application.Queries.GetFullAnamnese;
using PersonaScript.Modules.Personas.Domain;
using PersonaScript.Modules.Scripts.Application.Services;
using PersonaScript.Modules.Scripts.Domain;
using ScriptDomainErrors = PersonaScript.Modules.Scripts.Domain.DomainErrors;

namespace PersonaScript.Modules.Scripts.Application.Commands.RegenerateVideoScript;

public sealed class RegenerateVideoScriptCommandHandler : ICommandHandler<RegenerateVideoScriptCommand, Guid>
{
    private readonly IVideoScriptRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly IQueryHandler<GetFullAnamneseQuery, FullAnamneseDto> _getFullAnamneseQueryHandler;
    private readonly IPersonaDiagnosisRepository _personaDiagnosisRepository;
    private readonly IVideoScriptGenerator _generator;

    public RegenerateVideoScriptCommandHandler(
        IVideoScriptRepository repository,
        ITenantContext tenantContext,
        IQueryHandler<GetFullAnamneseQuery, FullAnamneseDto> getFullAnamneseQueryHandler,
        IPersonaDiagnosisRepository personaDiagnosisRepository,
        IVideoScriptGenerator generator)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _getFullAnamneseQueryHandler = getFullAnamneseQueryHandler;
        _personaDiagnosisRepository = personaDiagnosisRepository;
        _generator = generator;
    }

    public async Task<Result<Guid>> Handle(RegenerateVideoScriptCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId.Value;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<Guid>(ScriptDomainErrors.Scripts.TenantIdInvalido);
        }

        var script = await _repository.GetByIdAsync(command.TargetScriptId, cancellationToken);
        if (script is null)
        {
            return Result.Failure<Guid>(ScriptDomainErrors.Scripts.ScriptNaoEncontrado);
        }

        var anamneseResult = await _getFullAnamneseQueryHandler.Handle(new GetFullAnamneseQuery(), cancellationToken);
        if (anamneseResult.IsFailure || anamneseResult.Value is null)
        {
            return Result.Failure<Guid>(ScriptDomainErrors.Scripts.AnamneseOuDiagnosticoNaoEncontrado);
        }

        var anamnese = anamneseResult.Value;
        var diagnosis = await _personaDiagnosisRepository.GetByTenantIdAsync(cancellationToken);

        var instrucoesComFeedback = string.IsNullOrWhiteSpace(command.FeedbackNotes)
            ? "Regere o roteiro com melhorias de engajamento mantendo o tom de voz."
            : $"Ajustes solicitados pelo usuário: {command.FeedbackNotes}";

        var generatorResult = await _generator.GenerateAsync(
            anamnese,
            diagnosis,
            script.Tema,
            script.PilarConteudo,
            script.Objetivo,
            script.TomVozAplicado,
            instrucoesComFeedback,
            cancellationToken);

        if (generatorResult.IsFailure || generatorResult.Value is null)
        {
            return Result.Failure<Guid>(ScriptDomainErrors.Scripts.FalhaGeracaoLLM);
        }

        var dto = generatorResult.Value;

        var updateResult = script.UpdateContent(
            script.Tema,
            script.PilarConteudo,
            script.Objetivo,
            dto.Gancho,
            dto.Retencao,
            dto.ChamadaParaAcao,
            dto.LegendaSugerida,
            dto.DicasGravacao,
            dto.TomVozAplicado);

        if (updateResult.IsFailure)
        {
            return Result.Failure<Guid>(updateResult.Error);
        }

        script.RegisterFeedback(ScriptFeedbackRating.NeedsAdjustment, command.FeedbackNotes);

        await _repository.UpdateAsync(script, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success(script.Id);
    }
}
