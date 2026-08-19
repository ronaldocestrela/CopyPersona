using PersonaScript.BuildingBlocks.Domain;
using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.Modules.Scripts.Domain;

public sealed class VideoScript : BaseEntity, IMustHaveTenant
{
    private VideoScript() { } // EF Core constructor

    private VideoScript(
        Guid tenantId,
        Guid anamneseId,
        Guid? personaDiagnosisId,
        string tema,
        string pilarConteudo,
        string objetivo,
        string gancho,
        string retencao,
        string chamadaParaAcao,
        string legendaSugerida,
        string dicasGravacao,
        string tomVozAplicado)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        AnamneseId = anamneseId;
        PersonaDiagnosisId = personaDiagnosisId;
        Tema = tema;
        PilarConteudo = pilarConteudo;
        Objetivo = objetivo;
        Gancho = gancho;
        Retencao = retencao;
        ChamadaParaAcao = chamadaParaAcao;
        LegendaSugerida = legendaSugerida;
        DicasGravacao = dicasGravacao;
        TomVozAplicado = tomVozAplicado;
        Status = VideoScriptStatus.Draft;
        GeradoEm = DateTimeOffset.UtcNow;
    }

    public Guid TenantId { get; private set; }
    public Guid AnamneseId { get; private set; }
    public Guid? PersonaDiagnosisId { get; private set; }
    public string Tema { get; private set; } = string.Empty;
    public string PilarConteudo { get; private set; } = string.Empty;
    public string Objetivo { get; private set; } = string.Empty;

    // Blocos obrigatórios do Roteiro (Gancho, Retenção, CTA)
    public string Gancho { get; private set; } = string.Empty;
    public string Retencao { get; private set; } = string.Empty;
    public string ChamadaParaAcao { get; private set; } = string.Empty;

    // Complementos
    public string LegendaSugerida { get; private set; } = string.Empty;
    public string DicasGravacao { get; private set; } = string.Empty;
    public string TomVozAplicado { get; private set; } = string.Empty;

    public VideoScriptStatus Status { get; private set; }
    public ScriptFeedbackRating FeedbackRating { get; private set; } = ScriptFeedbackRating.None;
    public string? FeedbackNotes { get; private set; }
    public DateTimeOffset? FeedbackAt { get; private set; }
    public DateTimeOffset GeradoEm { get; private set; }
    public DateTimeOffset? AtualizadoEm { get; private set; }

    public void SetTenantId(Guid tenantId)
    {
        TenantId = tenantId;
    }

    public Result RegisterFeedback(ScriptFeedbackRating rating, string? notes = null)
    {
        FeedbackRating = rating;
        FeedbackNotes = notes?.Trim();
        FeedbackAt = DateTimeOffset.UtcNow;
        AtualizadoEm = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public static Result<VideoScript> Create(
        Guid tenantId,
        Guid anamneseId,
        Guid? personaDiagnosisId,
        string tema,
        string pilarConteudo,
        string objetivo,
        string gancho,
        string retencao,
        string chamadaParaAcao,
        string legendaSugerida,
        string dicasGravacao,
        string tomVozAplicado)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<VideoScript>(DomainErrors.Scripts.TenantIdInvalido);
        }

        if (string.IsNullOrWhiteSpace(tema))
        {
            return Result.Failure<VideoScript>(DomainErrors.Scripts.TemaInvalido);
        }

        if (string.IsNullOrWhiteSpace(gancho) || string.IsNullOrWhiteSpace(retencao) || string.IsNullOrWhiteSpace(chamadaParaAcao))
        {
            return Result.Failure<VideoScript>(DomainErrors.Scripts.ConteudoObrigatorioInvalido);
        }

        var script = new VideoScript(
            tenantId,
            anamneseId,
            personaDiagnosisId,
            tema.Trim(),
            pilarConteudo?.Trim() ?? string.Empty,
            objetivo?.Trim() ?? string.Empty,
            gancho.Trim(),
            retencao.Trim(),
            chamadaParaAcao.Trim(),
            legendaSugerida?.Trim() ?? string.Empty,
            dicasGravacao?.Trim() ?? string.Empty,
            tomVozAplicado?.Trim() ?? string.Empty);

        return Result.Success(script);
    }

    public Result UpdateStatus(VideoScriptStatus newStatus)
    {
        // Transições válidas
        // Draft -> Approved -> Recorded -> Published
        // Permite reabrir de Approved para Draft se necessário ajustar
        if (Status == VideoScriptStatus.Published && newStatus != VideoScriptStatus.Published)
        {
            return Result.Failure(DomainErrors.Scripts.StatusTransicaoInvalida);
        }

        Status = newStatus;
        AtualizadoEm = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result UpdateContent(
        string tema,
        string pilarConteudo,
        string objetivo,
        string gancho,
        string retencao,
        string chamadaParaAcao,
        string legendaSugerida,
        string dicasGravacao,
        string tomVozAplicado)
    {
        if (string.IsNullOrWhiteSpace(tema))
        {
            return Result.Failure(DomainErrors.Scripts.TemaInvalido);
        }

        if (string.IsNullOrWhiteSpace(gancho) || string.IsNullOrWhiteSpace(retencao) || string.IsNullOrWhiteSpace(chamadaParaAcao))
        {
            return Result.Failure(DomainErrors.Scripts.ConteudoObrigatorioInvalido);
        }

        Tema = tema.Trim();
        PilarConteudo = pilarConteudo?.Trim() ?? string.Empty;
        Objetivo = objetivo?.Trim() ?? string.Empty;
        Gancho = gancho.Trim();
        Retencao = retencao.Trim();
        ChamadaParaAcao = chamadaParaAcao.Trim();
        LegendaSugerida = legendaSugerida?.Trim() ?? string.Empty;
        DicasGravacao = dicasGravacao?.Trim() ?? string.Empty;
        TomVozAplicado = tomVozAplicado?.Trim() ?? string.Empty;
        AtualizadoEm = DateTimeOffset.UtcNow;

        return Result.Success();
    }
}
