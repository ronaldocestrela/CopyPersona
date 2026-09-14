using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.BuildingBlocks.AI.Abstractions;

public interface IPromptSanitizer
{
    /// <summary>
    /// Limpa caracteres de controle invisíveis, normaliza unicode e remove delimitadores de prompt de sistema (ex: &lt;system&gt;, [INST], &lt;|im_start|&gt;).
    /// </summary>
    string Sanitize(string? input);

    /// <summary>
    /// Verifica se o texto fornecido contém tentativas conhecidas de Prompt Injection ou Jailbreak adversarial.
    /// </summary>
    bool ContainsInjectionAttempt(string? input, out string? matchedPattern);

    /// <summary>
    /// Encapsula o conteúdo dentro de tags estruturadas de isolamento de contexto (Prompt Boundary Isolation).
    /// </summary>
    string EncapsulateBoundary(string? input, string sectionName);

    /// <summary>
    /// Sanitiza o input ou retorna Result.Failure caso uma tentativa clara de injeção adversarial seja detectada.
    /// </summary>
    Result<string> SanitizeOrReject(string? input, string fieldName);
}
