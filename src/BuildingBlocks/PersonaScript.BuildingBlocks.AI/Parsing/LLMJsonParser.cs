using System.Text.Json;
using System.Text.RegularExpressions;
using PersonaScript.BuildingBlocks.AI.Errors;
using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.BuildingBlocks.AI.Parsing;

public interface ILLMJsonParser
{
    Result<T> Parse<T>(string rawContent);
}

public sealed class LLMJsonParser : ILLMJsonParser
{
    private static readonly JsonSerializerOptions DefaultSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public Result<T> Parse<T>(string rawContent)
    {
        if (string.IsNullOrWhiteSpace(rawContent))
        {
            return Result.Failure<T>(LLMErrors.InvalidJsonResponse("O conteúdo retornado pela LLM está vazio ou nulo."));
        }

        string cleanJson = ExtractJsonContent(rawContent);

        try
        {
            var result = JsonSerializer.Deserialize<T>(cleanJson, DefaultSerializerOptions);
            if (result is null)
            {
                return Result.Failure<T>(LLMErrors.InvalidJsonResponse("A desserialização retornou um objeto nulo."));
            }

            return Result.Success(result);
        }
        catch (JsonException ex)
        {
            return Result.Failure<T>(LLMErrors.InvalidJsonResponse($"Erro de sintaxe JSON: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Result.Failure<T>(LLMErrors.InvalidJsonResponse($"Erro inesperado na desserialização: {ex.Message}"));
        }
    }

    private static string ExtractJsonContent(string text)
    {
        text = text.Trim();

        // Se o texto estiver encapsulado em bloco de código Markdown ```json ... ``` ou ``` ... ```
        var match = Regex.Match(text, @"```(?:json)?\s*([\s\S]*?)\s*```", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            text = match.Groups[1].Value.Trim();
        }

        // Tentar identificar os limites de JSON ({...} ou [...])
        int firstBrace = text.IndexOf('{');
        int lastBrace = text.LastIndexOf('}');

        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            return text.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        int firstBracket = text.IndexOf('[');
        int lastBracket = text.LastIndexOf(']');

        if (firstBracket >= 0 && lastBracket > firstBracket)
        {
            return text.Substring(firstBracket, lastBracket - firstBracket + 1);
        }

        return text;
    }
}
