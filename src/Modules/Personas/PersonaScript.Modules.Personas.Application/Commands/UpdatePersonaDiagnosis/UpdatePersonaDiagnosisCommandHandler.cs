using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Personas.Domain;
using PersonaScript.Modules.Personas.Domain.ValueObjects;

namespace PersonaScript.Modules.Personas.Application.Commands.UpdatePersonaDiagnosis;

public sealed class UpdatePersonaDiagnosisCommandHandler : ICommandHandler<UpdatePersonaDiagnosisCommand, Guid>
{
    private readonly IPersonaDiagnosisRepository _repository;
    private readonly ITenantContext _tenantContext;

    public UpdatePersonaDiagnosisCommandHandler(
        IPersonaDiagnosisRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(UpdatePersonaDiagnosisCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId.Value;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<Guid>(DomainErrors.Personas.TenantIdInvalido);
        }

        var diagnosis = await _repository.GetByTenantIdAsync(cancellationToken);
        if (diagnosis is null)
        {
            return Result.Failure<Guid>(DomainErrors.Personas.DiagnosticoNaoEncontrado);
        }

        var identidade = new IdentidadeMarca(
            command.IdentidadeMarca.TomDeVoz,
            command.IdentidadeMarca.EstiloVisualSugerido,
            command.IdentidadeMarca.ArquetipoPrincipal,
            command.IdentidadeMarca.ArquetipoSecundario
        );

        var pilares = command.PilaresConteudo
            .Select(p => new PilarConteudo(p.Nome, p.Percentual, p.Descricao, p.ExemplosTopicos))
            .ToList();

        var restricoes = new MatrizRestricoes(
            command.MatrizRestricoes.TemasProibidos,
            command.MatrizRestricoes.PalavrasEvitar,
            command.MatrizRestricoes.DiretrizesInegociaveis,
            command.MatrizRestricoes.LimitesExposicao
        );

        var updateResult = diagnosis.Update(
            command.FrasePosicionamento,
            command.SintesePerfil,
            identidade,
            pilares,
            restricoes
        );

        if (updateResult.IsFailure)
        {
            return Result.Failure<Guid>(updateResult.Error);
        }

        _repository.Update(diagnosis);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success(diagnosis.Id);
    }
}
