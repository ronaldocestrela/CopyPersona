using PersonaScript.BuildingBlocks.Domain;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Scripts.Domain.ValueObjects;

namespace PersonaScript.Modules.Scripts.Domain;

public sealed class StoryPlan : BaseEntity, IMustHaveTenant
{
    private readonly List<StoryBlock> _blocosHorarios = new();

    private StoryPlan() { } // EF Core constructor

    private StoryPlan(
        Guid tenantId,
        Guid anamneseId,
        Guid? personaDiagnosisId,
        string frequenciaDiariaRecomendada,
        IEnumerable<StoryBlock> blocosHorarios,
        string diretrizesHumanizacao)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        AnamneseId = anamneseId;
        PersonaDiagnosisId = personaDiagnosisId;
        FrequenciaDiariaRecomendada = frequenciaDiariaRecomendada;
        _blocosHorarios.AddRange(blocosHorarios);
        DiretrizesHumanizacao = diretrizesHumanizacao;
        GeradoEm = DateTimeOffset.UtcNow;
    }

    public Guid TenantId { get; private set; }
    public Guid AnamneseId { get; private set; }
    public Guid? PersonaDiagnosisId { get; private set; }
    public string FrequenciaDiariaRecomendada { get; private set; } = string.Empty;
    public IReadOnlyCollection<StoryBlock> BlocosHorarios => _blocosHorarios.AsReadOnly();
    public string DiretrizesHumanizacao { get; private set; } = string.Empty;
    public DateTimeOffset GeradoEm { get; private set; }
    public DateTimeOffset? AtualizadoEm { get; private set; }

    public void SetTenantId(Guid tenantId)
    {
        TenantId = tenantId;
    }

    public static Result<StoryPlan> Create(
        Guid tenantId,
        Guid anamneseId,
        Guid? personaDiagnosisId,
        string frequenciaDiariaRecomendada,
        IEnumerable<StoryBlock>? blocosHorarios,
        string diretrizesHumanizacao)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<StoryPlan>(DomainErrors.Scripts.TenantIdInvalido);
        }

        var blocosList = blocosHorarios?.ToList() ?? new List<StoryBlock>();
        if (blocosList.Count == 0 || string.IsNullOrWhiteSpace(frequenciaDiariaRecomendada))
        {
            return Result.Failure<StoryPlan>(DomainErrors.Scripts.StoryPlanInvalido);
        }

        var plan = new StoryPlan(
            tenantId,
            anamneseId,
            personaDiagnosisId,
            frequenciaDiariaRecomendada.Trim(),
            blocosList,
            diretrizesHumanizacao?.Trim() ?? string.Empty);

        return Result.Success(plan);
    }
}
