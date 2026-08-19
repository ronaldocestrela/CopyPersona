using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Scripts.Application.DTOs;
using PersonaScript.Modules.Scripts.Domain;

namespace PersonaScript.Modules.Scripts.Application.Queries.GetVideoScriptById;

public sealed class GetVideoScriptByIdQueryHandler : IQueryHandler<GetVideoScriptByIdQuery, VideoScriptDto>
{
    private readonly IVideoScriptRepository _repository;
    private readonly ITenantContext _tenantContext;

    public GetVideoScriptByIdQueryHandler(IVideoScriptRepository repository, ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<VideoScriptDto>> Handle(GetVideoScriptByIdQuery query, CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId.Value == Guid.Empty)
        {
            return Result.Failure<VideoScriptDto>(DomainErrors.Scripts.TenantIdInvalido);
        }

        var script = await _repository.GetByIdAsync(query.ScriptId, cancellationToken);
        if (script is null)
        {
            return Result.Failure<VideoScriptDto>(DomainErrors.Scripts.ScriptNaoEncontrado);
        }

        var dto = new VideoScriptDto(
            script.Id,
            script.TenantId,
            script.AnamneseId,
            script.PersonaDiagnosisId,
            script.Tema,
            script.PilarConteudo,
            script.Objetivo,
            script.Gancho,
            script.Retencao,
            script.ChamadaParaAcao,
            script.LegendaSugerida,
            script.DicasGravacao,
            script.TomVozAplicado,
            script.Status,
            script.GeradoEm,
            script.AtualizadoEm);

        return Result.Success(dto);
    }
}
