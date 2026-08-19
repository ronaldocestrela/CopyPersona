using PersonaScript.BuildingBlocks.Domain;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Scripts.Domain.ValueObjects;

namespace PersonaScript.Modules.Scripts.Domain;

public sealed class NinetyDayCalendar : BaseEntity, IMustHaveTenant
{
    private readonly List<WeeklyEditorialPlan> _semanas = new();

    private NinetyDayCalendar() { } // EF Core constructor

    private NinetyDayCalendar(
        Guid tenantId,
        Guid anamneseId,
        Guid? personaDiagnosisId,
        string objetivoTrimestral,
        IEnumerable<WeeklyEditorialPlan> semanas)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        AnamneseId = anamneseId;
        PersonaDiagnosisId = personaDiagnosisId;
        ObjetivoTrimestral = objetivoTrimestral;
        _semanas.AddRange(semanas);
        GeradoEm = DateTimeOffset.UtcNow;
    }

    public Guid TenantId { get; private set; }
    public Guid AnamneseId { get; private set; }
    public Guid? PersonaDiagnosisId { get; private set; }
    public string ObjetivoTrimestral { get; private set; } = string.Empty;
    public IReadOnlyCollection<WeeklyEditorialPlan> Semanas => _semanas.AsReadOnly();
    public DateTimeOffset GeradoEm { get; private set; }
    public DateTimeOffset? AtualizadoEm { get; private set; }

    public void SetTenantId(Guid tenantId)
    {
        TenantId = tenantId;
    }

    public static Result<NinetyDayCalendar> Create(
        Guid tenantId,
        Guid anamneseId,
        Guid? personaDiagnosisId,
        string objetivoTrimestral,
        IEnumerable<WeeklyEditorialPlan>? semanas)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<NinetyDayCalendar>(DomainErrors.Scripts.TenantIdInvalido);
        }

        var semanasList = semanas?.ToList() ?? new List<WeeklyEditorialPlan>();
        if (semanasList.Count == 0 || string.IsNullOrWhiteSpace(objetivoTrimestral))
        {
            return Result.Failure<NinetyDayCalendar>(DomainErrors.Scripts.NinetyDayCalendarInvalido);
        }

        var calendar = new NinetyDayCalendar(
            tenantId,
            anamneseId,
            personaDiagnosisId,
            objetivoTrimestral.Trim(),
            semanasList);

        return Result.Success(calendar);
    }
}
