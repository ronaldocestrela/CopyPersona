using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.Modules.Personas.Domain;

public static class DomainErrors
{
    public static class Personas
    {
        public static readonly Error TenantIdInvalido = Error.Validation(
            "Personas.TenantIdInvalido",
            "O ID do tenant informado é inválido."
        );

        public static readonly Error AnamneseIdInvalida = Error.Validation(
            "Personas.AnamneseIdInvalida",
            "O ID da Anamnese informada é inválido."
        );

        public static readonly Error AnamneseNaoEncontrada = Error.NotFound(
            "Personas.AnamneseNaoEncontrada",
            "A Anamnese necessária para a geração da Persona não foi encontrada."
        );

        public static readonly Error AnamneseNaoConcluida = Error.Validation(
            "Personas.AnamneseNaoConcluida",
            "A Anamnese precisa estar concluída para gerar o Diagnóstico de Posicionamento."
        );

        public static readonly Error DiagnosticoNaoEncontrado = Error.NotFound(
            "Personas.DiagnosticoNaoEncontrado",
            "Nenhum Diagnóstico de Posicionamento foi encontrado para o tenant."
        );

        public static readonly Error PercentualPilaresInvalido = Error.Validation(
            "Personas.PercentualPilaresInvalido",
            "A soma dos percentuais dos pilares de conteúdo deve ser exatamente 100%."
        );

        public static readonly Error FalhaGeracaoLLM = new(
            "Personas.FalhaGeracaoLLM",
            "Falha ao gerar o Diagnóstico de Posicionamento com a Inteligência Artificial."
        );
    }
}
