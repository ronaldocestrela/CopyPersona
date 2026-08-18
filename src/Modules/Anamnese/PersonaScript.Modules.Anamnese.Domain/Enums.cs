namespace PersonaScript.Modules.Anamnese.Domain;

public enum AnamneseStatus
{
    Rascunho = 0,
    Concluido = 1
}

public enum MomentoAtualEnum
{
    IniciandoAgenda = 1,
    AgendaRazoavel = 2,
    AgendaCheiaCobrarMais = 3,
    ReferenciaExpansao = 4
}

public enum CanalOrigemEnum
{
    Indicacao = 1,
    Instagram = 2,
    Google = 3,
    ConvenioParcerias = 4,
    PassamEmFrente = 5,
    Outro = 6
}

public enum NivelConfortoCameraEnum
{
    SuperAVontade = 1,
    NaoNatural = 2,
    VergonhaMasTopoAprender = 3,
    PrefiroEvitar = 4
}

public enum ArquetipoComunicacaoEnum
{
    Professor = 1,
    Amigo = 2,
    Autoridade = 3,
    Descomplicador = 4,
    Inspirador = 5
}

public enum ResultadoPrioritarioEnum
{
    MaisSeguidoresVisibilidade = 1,
    MaisAutoridadeRespeito = 2,
    MaisPacientesAgenda = 3,
    PacientesMelhoresTicketAlto = 4
}
