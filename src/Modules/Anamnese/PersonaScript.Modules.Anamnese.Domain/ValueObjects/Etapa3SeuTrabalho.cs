namespace PersonaScript.Modules.Anamnese.Domain.ValueObjects;

public sealed record Etapa3SeuTrabalho(
    string ProcedimentoMaster,
    string ProcedimentoLucrativo,
    string ProcedimentoPreferido,
    string DiferencialAtendimento,
    string PorQueEscolhemVoce,
    string CriticaAosPares
);
