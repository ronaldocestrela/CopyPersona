using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Application.Queries.GetFullAnamnese;
using PersonaScript.Modules.Anamnese.Domain;
using PersonaScript.Modules.Personas.Application.Services;
using PersonaScript.Modules.Personas.Domain;
using PersonaScript.Modules.Personas.Domain.ValueObjects;

namespace PersonaScript.Modules.Personas.Application.Commands.GeneratePersonaDiagnosis;

public sealed class GeneratePersonaDiagnosisCommandHandler : ICommandHandler<GeneratePersonaDiagnosisCommand, Guid>
{
    private readonly IPersonaDiagnosisRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly IQueryHandler<GetFullAnamneseQuery, FullAnamneseDto> _getFullAnamneseQueryHandler;
    private readonly IPersonaDiagnosisGenerator _generator;

    public GeneratePersonaDiagnosisCommandHandler(
        IPersonaDiagnosisRepository repository,
        ITenantContext tenantContext,
        IQueryHandler<GetFullAnamneseQuery, FullAnamneseDto> getFullAnamneseQueryHandler,
        IPersonaDiagnosisGenerator generator)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _getFullAnamneseQueryHandler = getFullAnamneseQueryHandler;
        _generator = generator;
    }

    public async Task<Result<Guid>> Handle(GeneratePersonaDiagnosisCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId.Value;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<Guid>(PersonaScript.Modules.Personas.Domain.DomainErrors.Personas.TenantIdInvalido);
        }

        var anamneseResult = await _getFullAnamneseQueryHandler.Handle(new GetFullAnamneseQuery(), cancellationToken);
        if (anamneseResult.IsFailure || anamneseResult.Value is null)
        {
            return Result.Failure<Guid>(PersonaScript.Modules.Personas.Domain.DomainErrors.Personas.AnamneseNaoEncontrada);
        }

        var anamnese = anamneseResult.Value;
        if (anamnese.Status.Status != AnamneseStatus.Concluido)
        {
            return Result.Failure<Guid>(PersonaScript.Modules.Personas.Domain.DomainErrors.Personas.AnamneseNaoConcluida);
        }

        var generatorResult = await _generator.GenerateAsync(anamnese, cancellationToken);
        if (generatorResult.IsFailure || generatorResult.Value is null)
        {
            return Result.Failure<Guid>(PersonaScript.Modules.Personas.Domain.DomainErrors.Personas.FalhaGeracaoLLM);
        }

        var dto = generatorResult.Value;

        var identidade = new IdentidadeMarca(
            dto.TomDeVoz,
            dto.EstiloVisualSugerido,
            dto.ArquetipoPrincipal,
            dto.ArquetipoSecundario
        );

        var pilares = dto.PilaresConteudo
            .Select(p => new PilarConteudo(p.Nome, p.Percentual, p.Descricao, p.ExemplosTopicos))
            .ToList();

        var restricoes = new MatrizRestricoes(
            dto.TemasProibidos,
            dto.PalavrasEvitar,
            dto.DiretrizesInegociaveis,
            dto.LimitesExposicao
        );

        var existingDiagnosis = await _repository.GetByTenantIdAsync(cancellationToken);
        if (existingDiagnosis is not null)
        {
            var updateResult = existingDiagnosis.Update(
                dto.FrasePosicionamento,
                dto.SintesePerfil,
                identidade,
                pilares,
                restricoes
            );

            if (updateResult.IsFailure)
            {
                return Result.Failure<Guid>(updateResult.Error);
            }

            _repository.Update(existingDiagnosis);
            await _repository.SaveChangesAsync(cancellationToken);
            return Result.Success(existingDiagnosis.Id);
        }

        var createResult = PersonaDiagnosis.Create(
            tenantId,
            anamnese.Status.Id,
            dto.FrasePosicionamento,
            dto.SintesePerfil,
            identidade,
            pilares,
            restricoes
        );

        if (createResult.IsFailure || createResult.Value is null)
        {
            return Result.Failure<Guid>(createResult.Error);
        }

        var diagnosis = createResult.Value;
        await _repository.AddAsync(diagnosis, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success(diagnosis.Id);
    }
}
