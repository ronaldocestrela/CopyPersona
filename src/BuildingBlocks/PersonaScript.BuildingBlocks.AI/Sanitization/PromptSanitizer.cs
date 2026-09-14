using System.Text.RegularExpressions;
using PersonaScript.BuildingBlocks.AI.Abstractions;
using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.BuildingBlocks.AI.Sanitization;

public partial class PromptSanitizer : IPromptSanitizer
{
    // Regex para remover delimitadores estruturais comuns usados em ataques de LLM
    [GeneratedRegex(@"(<\/?(?:system|instruction|user|assistant|untrusted_user_content)[^>]*>|\[\/?INST\]|<\/?(?:\|im_start\||\|im_end\|)>|<<\/?SYS>>|```(?:system|instruction))", RegexOptions.IgnoreCase)]
    private static partial Regex DelimiterPattern();

    // Regex para detectar padrões adversariais de injeção direta de prompt
    [GeneratedRegex(@"(?i)\b(?:ignore|disregard|forget|override)\s+(?:all\s+)?(?:previous|earlier|above|prior)\s+(?:instructions|prompts|rules|guidelines)\b")]
    private static partial Regex IgnorePreviousInstructionsPattern();

    [GeneratedRegex(@"(?i)\b(?:system\s+prompt\s+override|system\s+instructions\s+override)\b")]
    private static partial Regex SystemOverridePattern();

    [GeneratedRegex(@"(?i)\b(?:you\s+are\s+now\s+(?:dan|unfiltered|jailbroken|evil|unrestricted)|act\s+as\s+dan|dan\s+mode)\b")]
    private static partial Regex JailbreakPersonaPattern();

    [GeneratedRegex(@"(?i)\b(?:bypass|disable)\s+(?:ethical|safety|content)\s+(?:guidelines|filters|protocols|rules)\b")]
    private static partial Regex BypassSafetyPattern();

    [GeneratedRegex(@"(?i)\b(?:reveal|print|output|display|show)\s+(?:your\s+)?(?:initial\s+|secret\s+)?(?:system\s+prompt|previous\s+instructions)\b")]
    private static partial Regex RevealPromptPattern();

    public string Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // 1. Remove caracteres invisíveis, de controle e zero-width (exceto tab, LF, CR)
        var cleaned = CleanControlAndInvisibleChars(input);

        // 2. Remove delimitadores estruturais de prompt
        cleaned = DelimiterPattern().Replace(cleaned, string.Empty);

        return cleaned.Trim();
    }

    public bool ContainsInjectionAttempt(string? input, out string? matchedPattern)
    {
        matchedPattern = null;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var normalized = CleanControlAndInvisibleChars(input);

        var match = IgnorePreviousInstructionsPattern().Match(normalized);
        if (match.Success)
        {
            matchedPattern = match.Value;
            return true;
        }

        match = SystemOverridePattern().Match(normalized);
        if (match.Success)
        {
            matchedPattern = match.Value;
            return true;
        }

        match = JailbreakPersonaPattern().Match(normalized);
        if (match.Success)
        {
            matchedPattern = match.Value;
            return true;
        }

        match = BypassSafetyPattern().Match(normalized);
        if (match.Success)
        {
            matchedPattern = match.Value;
            return true;
        }

        match = RevealPromptPattern().Match(normalized);
        if (match.Success)
        {
            matchedPattern = match.Value;
            return true;
        }

        return false;
    }

    public string EncapsulateBoundary(string? input, string sectionName)
    {
        var sanitized = Sanitize(input);
        // Neutraliza qualquer tentativa de fechar precocemente a tag delimitadora
        sanitized = sanitized.Replace("</untrusted_user_content>", "[tag-escaped]");

        return $"<untrusted_user_content section=\"{sectionName}\">\n{sanitized}\n</untrusted_user_content>";
    }

    public Result<string> SanitizeOrReject(string? input, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Result.Success(string.Empty);

        if (ContainsInjectionAttempt(input, out var matchedPattern))
        {
            return Result.Failure<string>(Error.Validation(
                "Prompt.InjectionDetected",
                $"Tentativa de injeção de prompt adversarial detectada no campo '{fieldName}' (Padrão: '{matchedPattern}')."));
        }

        return Result.Success(Sanitize(input));
    }

    private static string CleanControlAndInvisibleChars(string source)
    {
        Span<char> buffer = source.Length <= 1024 ? stackalloc char[source.Length] : new char[source.Length];
        int idx = 0;

        foreach (var c in source)
        {
            // Pula zero-width spaces e BOMs
            if (c == '\u200B' || c == '\u200C' || c == '\u200D' || c == '\uFEFF' || c == '\u00AD')
                continue;

            // Pula caracteres de controle ASCII (0 a 31), exceto \t, \n, \r
            if (char.IsControl(c) && c != '\t' && c != '\n' && c != '\r')
                continue;

            buffer[idx++] = c;
        }

        return new string(buffer[..idx]);
    }
}
