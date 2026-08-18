using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.Modules.Anamnese.Domain;

public static class DomainErrors
{
    public static class Anamnese
    {
        public static readonly Error TenantIdInvalido = new(
            "Anamnese.TenantIdInvalido",
            "O ID do tenant é obrigatório e não pode ser vazio.");

        public static readonly Error EtapaInvalida = new(
            "Anamnese.EtapaInvalida",
            "A etapa informada é inválida ou nula.");

        public static readonly Error JaConcluida = new(
            "Anamnese.JaConcluida",
            "A anamnese já foi concluída e não pode ser modificada.");

        public static readonly Error EtapasIncompletas = new(
            "Anamnese.EtapasIncompletas",
            "Não é possível concluir a anamnese sem preencher todas as 10 etapas.");

        public static readonly Error NaoEncontrada = new(
            "Anamnese.NaoEncontrada",
            "Nenhuma anamnese foi encontrada para este tenant.");
    }
}
