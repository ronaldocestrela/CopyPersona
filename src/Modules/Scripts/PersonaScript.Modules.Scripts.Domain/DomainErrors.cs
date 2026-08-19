using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.Modules.Scripts.Domain;

public static class DomainErrors
{
    public static class Scripts
    {
        public static readonly Error TenantIdInvalido = Error.Validation(
            "Scripts.TenantIdInvalido",
            "O ID do tenant é obrigatório e não pode ser vazio.");

        public static readonly Error TemaInvalido = Error.Validation(
            "Scripts.TemaInvalido",
            "O tema do roteiro é obrigatório.");

        public static readonly Error ConteudoObrigatorioInvalido = Error.Validation(
            "Scripts.ConteudoObrigatorioInvalido",
            "Os blocos de Gancho, Retenção e CTA são obrigatórios para a geração do roteiro.");

        public static readonly Error StatusTransicaoInvalida = Error.Validation(
            "Scripts.StatusTransicaoInvalida",
            "Transição de status inválida para o roteiro.");

        public static readonly Error ScriptNaoEncontrado = Error.NotFound(
            "Scripts.ScriptNaoEncontrado",
            "O roteiro de vídeo solicitado não foi encontrado.");

        public static readonly Error FalhaGeracaoLLM = new(
            "Scripts.FalhaGeracaoLLM",
            "Não foi possível gerar o roteiro com a Inteligência Artificial.");

        public static readonly Error AnamneseOuDiagnosticoNaoEncontrado = Error.NotFound(
            "Scripts.AnamneseOuDiagnosticoNaoEncontrado",
            "Para gerar conteúdos ou planos estratégicos, é necessário ter a Anamnese concluída.");

        public static readonly Error StoryPlanNaoEncontrado = Error.NotFound(
            "Scripts.StoryPlanNaoEncontrado",
            "O Plano de Stories solicitado não foi encontrado.");

        public static readonly Error StoryPlanInvalido = Error.Validation(
            "Scripts.StoryPlanInvalido",
            "O Plano de Stories deve conter blocos horários e diretrizes válidos.");

        public static readonly Error NinetyDayCalendarNaoEncontrado = Error.NotFound(
            "Scripts.NinetyDayCalendarNaoEncontrado",
            "O Calendário Editorial de 90 Dias não foi encontrado.");

        public static readonly Error NinetyDayCalendarInvalido = Error.Validation(
            "Scripts.NinetyDayCalendarInvalido",
            "O Calendário Editorial de 90 Dias deve conter semanas com planejamentos válidos.");
    }
}
