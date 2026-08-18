namespace PersonaScript.Modules.Anamnese.Domain.ValueObjects;

public sealed record Etapa4SeuPaciente(
    string PerfilDemograficoPsicografico,
    string MaioresMedos,
    string MaioresDesejos,
    string PerguntasFrequentes,
    string MitosInformacoesErradas,
    CanalOrigemEnum CanalOrigem
);
