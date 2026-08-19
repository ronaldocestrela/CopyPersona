using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Application.Queries.GetFullAnamnese;
using PersonaScript.Modules.Anamnese.Domain;
using PersonaScript.Modules.Personas.Domain;
using PersonaScript.Modules.Scripts.Application.Services;
using PersonaScript.Modules.Scripts.Domain;

namespace PersonaScript.Modules.Scripts.Application.Commands.GenerateVideoScript;

public sealed class GenerateVideoScriptCommandHandler : ICommandHandler<GenerateVideoScriptCommand, Guid>
{
    private readonly IVideoScriptRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly IQueryHandler<GetFullAnamneseQuery, FullAnamneseDto> _getFullAnamneseQueryHandler;
    private readonly IPersonaDiagnosisRepository _personaDiagnosisRepository;
    private readonly IVideoScriptGenerator _generator;

    public GenerateVideoScriptCommandHandler(
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

    public async Task<Result<Guid>> Handle(GenerateVideoScriptCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId.Value;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<Guid>(PersonaScript.Modules.Scripts.Domain.DomainErrors.Scripts.TenantIdInvalido);
        }

        var anamneseResult = await _getFullAnamneseQueryHandler.Handle(new GetFullAnamneseQuery(), cancellationToken);
        if (anamneseResult.IsFailure || anamneseResult.Value is null)
        {
            return Result.Failure<Guid>(PersonaScript.Modules.Scripts.Domain.DomainErrors.Scripts.AnamneseOuDiagnosticoNaoEncontrado);
        }

        var anamnese = anamneseResult.Value;
        if (anamnese.Status.Status != AnamneseStatus.Concluido)
        {
            return Result.Failure<Guid>(PersonaScript.Modules.Scripts.Domain.DomainErrors.Scripts.AnamneseOuDiagnosticoNaoEncontrado);
        }

        var diagnosis = await _personaDiagnosisRepository.GetByTenantIdAsync(cancellationToken);

        var generatorResult = await _generator.GenerateAsync(
            anamnese,
            diagnosis,
            command.Tema,
            command.PilarConteudo,
            command.Objetivo,
            command.TomDesejado,
            command.InstrucoesAdicionais,
            cancellationToken);

        if (generatorResult.IsFailure || generatorResult.Value is null)
        {
            return Result.Failure<Guid>(PersonaScript.Modules.Scripts.Domain.DomainErrors.Scripts.FalhaGeracaoLLM);
        }

        var dto = generatorResult.Value;

        var scriptResult = VideoScript.Create(
            tenantId,
            anamnese.Status.Id,
            diagnosis?.Id,
            command.Tema,
            command.PilarConteudo,
            command.Objetivo,
            dto.Gancho,
            dto.Retencao,
            dto.ChamadaParaAcao,
            dto.LegendaSugerida,
            dto.DicasGravacao,
            dto.TomVozAplicado);

        if (scriptResult.IsFailure || scriptResult.Value is null)
        {
            return Result.Failure<Guid>(scriptResult.Error);
        }

        var script = scriptResult.Value;
        await _repository.AddAsync(script, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success(script.Id);
    }
}
