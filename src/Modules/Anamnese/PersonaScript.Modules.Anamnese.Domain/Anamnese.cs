using PersonaScript.BuildingBlocks.Domain;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Anamnese.Domain.ValueObjects;

namespace PersonaScript.Modules.Anamnese.Domain;

public sealed class Anamnese : BaseEntity, IMustHaveTenant
{
    private Anamnese() { } // Constructor for EF Core

    private Anamnese(Guid tenantId)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Status = AnamneseStatus.Rascunho;
        EtapaAtual = 1;
        PercentualConclusao = 0;
        CriadoEm = DateTimeOffset.UtcNow;
    }

    public Guid TenantId { get; private set; }
    public AnamneseStatus Status { get; private set; }
    public int EtapaAtual { get; private set; }
    public int PercentualConclusao { get; private set; }
    public DateTimeOffset CriadoEm { get; private set; }
    public DateTimeOffset? AtualizadoEm { get; private set; }
    public DateTimeOffset? ConcluidoEm { get; private set; }

    public Etapa1QuemEVoce? Etapa1 { get; private set; }
    public Etapa2SuaHistoria? Etapa2 { get; private set; }
    public Etapa3SeuTrabalho? Etapa3 { get; private set; }
    public Etapa4SeuPaciente? Etapa4 { get; private set; }
    public Etapa5SuasReferencias? Etapa5 { get; private set; }
    public Etapa6LimitesExposicao? Etapa6 { get; private set; }
    public Etapa7SeuConhecimento? Etapa7 { get; private set; }
    public Etapa8SeuJeito? Etapa8 { get; private set; }
    public Etapa9RotinaCapacidade? Etapa9 { get; private set; }
    public Etapa10Objetivos? Etapa10 { get; private set; }

    public void SetTenantId(Guid tenantId)
    {
        TenantId = tenantId;
    }

    public static Result<Anamnese> Create(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<Anamnese>(DomainErrors.Anamnese.TenantIdInvalido);
        }

        return Result.Success(new Anamnese(tenantId));
    }

    public Result UpdateEtapa1(Etapa1QuemEVoce etapa)
    {
        if (Status == AnamneseStatus.Concluido)
            return Result.Failure(DomainErrors.Anamnese.JaConcluida);

        Etapa1 = etapa;
        RecalcularProgresso();
        return Result.Success();
    }

    public Result UpdateEtapa2(Etapa2SuaHistoria etapa)
    {
        if (Status == AnamneseStatus.Concluido)
            return Result.Failure(DomainErrors.Anamnese.JaConcluida);

        Etapa2 = etapa;
        RecalcularProgresso();
        return Result.Success();
    }

    public Result UpdateEtapa3(Etapa3SeuTrabalho etapa)
    {
        if (Status == AnamneseStatus.Concluido)
            return Result.Failure(DomainErrors.Anamnese.JaConcluida);

        Etapa3 = etapa;
        RecalcularProgresso();
        return Result.Success();
    }

    public Result UpdateEtapa4(Etapa4SeuPaciente etapa)
    {
        if (Status == AnamneseStatus.Concluido)
            return Result.Failure(DomainErrors.Anamnese.JaConcluida);

        Etapa4 = etapa;
        RecalcularProgresso();
        return Result.Success();
    }

    public Result UpdateEtapa5(Etapa5SuasReferencias etapa)
    {
        if (Status == AnamneseStatus.Concluido)
            return Result.Failure(DomainErrors.Anamnese.JaConcluida);

        Etapa5 = etapa;
        RecalcularProgresso();
        return Result.Success();
    }

    public Result UpdateEtapa6(Etapa6LimitesExposicao etapa)
    {
        if (Status == AnamneseStatus.Concluido)
            return Result.Failure(DomainErrors.Anamnese.JaConcluida);

        Etapa6 = etapa;
        RecalcularProgresso();
        return Result.Success();
    }

    public Result UpdateEtapa7(Etapa7SeuConhecimento etapa)
    {
        if (Status == AnamneseStatus.Concluido)
            return Result.Failure(DomainErrors.Anamnese.JaConcluida);

        Etapa7 = etapa;
        RecalcularProgresso();
        return Result.Success();
    }

    public Result UpdateEtapa8(Etapa8SeuJeito etapa)
    {
        if (Status == AnamneseStatus.Concluido)
            return Result.Failure(DomainErrors.Anamnese.JaConcluida);

        Etapa8 = etapa;
        RecalcularProgresso();
        return Result.Success();
    }

    public Result UpdateEtapa9(Etapa9RotinaCapacidade etapa)
    {
        if (Status == AnamneseStatus.Concluido)
            return Result.Failure(DomainErrors.Anamnese.JaConcluida);

        Etapa9 = etapa;
        RecalcularProgresso();
        return Result.Success();
    }

    public Result UpdateEtapa10(Etapa10Objetivos etapa)
    {
        if (Status == AnamneseStatus.Concluido)
            return Result.Failure(DomainErrors.Anamnese.JaConcluida);

        Etapa10 = etapa;
        RecalcularProgresso();
        return Result.Success();
    }

    public Result Concluir()
    {
        if (Status == AnamneseStatus.Concluido)
            return Result.Success();

        if (Etapa1 is null || Etapa2 is null || Etapa3 is null || Etapa4 is null || Etapa5 is null ||
            Etapa6 is null || Etapa7 is null || Etapa8 is null || Etapa9 is null || Etapa10 is null)
        {
            return Result.Failure(DomainErrors.Anamnese.EtapasIncompletas);
        }

        Status = AnamneseStatus.Concluido;
        PercentualConclusao = 100;
        EtapaAtual = 10;
        ConcluidoEm = DateTimeOffset.UtcNow;
        AtualizadoEm = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    private void RecalcularProgresso()
    {
        AtualizadoEm = DateTimeOffset.UtcNow;
        int preenchidas = 0;
        if (Etapa1 is not null) preenchidas++;
        if (Etapa2 is not null) preenchidas++;
        if (Etapa3 is not null) preenchidas++;
        if (Etapa4 is not null) preenchidas++;
        if (Etapa5 is not null) preenchidas++;
        if (Etapa6 is not null) preenchidas++;
        if (Etapa7 is not null) preenchidas++;
        if (Etapa8 is not null) preenchidas++;
        if (Etapa9 is not null) preenchidas++;
        if (Etapa10 is not null) preenchidas++;

        PercentualConclusao = preenchidas * 10;

        if (Etapa10 is not null) EtapaAtual = 10;
        else if (Etapa9 is not null) EtapaAtual = 10;
        else if (Etapa8 is not null) EtapaAtual = 9;
        else if (Etapa7 is not null) EtapaAtual = 8;
        else if (Etapa6 is not null) EtapaAtual = 7;
        else if (Etapa5 is not null) EtapaAtual = 6;
        else if (Etapa4 is not null) EtapaAtual = 5;
        else if (Etapa3 is not null) EtapaAtual = 4;
        else if (Etapa2 is not null) EtapaAtual = 3;
        else if (Etapa1 is not null) EtapaAtual = 2;
        else EtapaAtual = 1;
    }
}
