using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Scripts.Application.DTOs;
using PersonaScript.Modules.Scripts.Domain;

namespace PersonaScript.Modules.Scripts.Application.Queries.ListVideoScripts;

public sealed class ListVideoScriptsQueryHandler : IQueryHandler<ListVideoScriptsQuery, IReadOnlyList<VideoScriptDto>>
{
    private readonly IVideoScriptRepository _repository;
    private readonly ITenantContext _tenantContext;

    public ListVideoScriptsQueryHandler(IVideoScriptRepository repository, ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<IReadOnlyList<VideoScriptDto>>> Handle(ListVideoScriptsQuery query, CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId.Value == Guid.Empty)
        {
            return Result.Failure<IReadOnlyList<VideoScriptDto>>(DomainErrors.Scripts.TenantIdInvalido);
        }

        IReadOnlyList<VideoScript> scripts;
        if (query.Status.HasValue)
        {
            scripts = await _repository.ListByStatusAsync(query.Status.Value, cancellationToken);
        }
        else
        {
            scripts = await _repository.ListByTenantIdAsync(cancellationToken);
        }

        var filtered = scripts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            filtered = filtered.Where(s =>
                s.Tema.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                s.Gancho.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                s.PilarConteudo.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.PilarConteudo))
        {
            filtered = filtered.Where(s =>
                s.PilarConteudo.Equals(query.PilarConteudo.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        scripts = filtered.ToList();

        var dtos = scripts.Select(s => new VideoScriptDto(
            s.Id,
            s.TenantId,
            s.AnamneseId,
            s.PersonaDiagnosisId,
            s.Tema,
            s.PilarConteudo,
            s.Objetivo,
            s.Gancho,
            s.Retencao,
            s.ChamadaParaAcao,
            s.LegendaSugerida,
            s.DicasGravacao,
            s.TomVozAplicado,
            s.Status,
            s.FeedbackRating,
            s.FeedbackNotes,
            s.FeedbackAt,
            s.GeradoEm,
            s.AtualizadoEm
        )).ToList();

        return Result.Success<IReadOnlyList<VideoScriptDto>>(dtos);
    }
}
