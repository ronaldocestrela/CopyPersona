using System.Diagnostics;
using System.Text.Json;
using PersonaScript.BuildingBlocks.AI.Abstractions;
using PersonaScript.BuildingBlocks.AI.Models;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Application.Abstractions;
using PersonaScript.Modules.Backoffice.Application.DTOs;
using PersonaScript.Modules.Backoffice.Domain.Enums;

namespace PersonaScript.Modules.Backoffice.Application.Commands.Prompts;

public record TestPromptPlaygroundCommand(
    string AgentName,
    string SystemPrompt,
    string UserPromptTemplate,
    string ParametersJson,
    string TestVariablesJson) : ICommand<TestPromptResultDto>;

public sealed class TestPromptPlaygroundCommandHandler : ICommandHandler<TestPromptPlaygroundCommand, TestPromptResultDto>
{
    private readonly ILLMProvider _llmProvider;
    private readonly ILLMTelemetryService? _telemetryService;

    public TestPromptPlaygroundCommandHandler(ILLMProvider llmProvider, ILLMTelemetryService? telemetryService = null)
    {
        _llmProvider = llmProvider;
        _telemetryService = telemetryService;
    }

    public async Task<Result<TestPromptResultDto>> Handle(TestPromptPlaygroundCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.SystemPrompt))
        {
            return Result.Failure<TestPromptResultDto>(Error.Validation("TestPromptPlayground.SystemPromptRequired", "O System Prompt é obrigatório."));
        }

        if (string.IsNullOrWhiteSpace(command.UserPromptTemplate))
        {
            return Result.Failure<TestPromptResultDto>(Error.Validation("TestPromptPlayground.UserPromptTemplateRequired", "O User Prompt Template é obrigatório."));
        }

        // Renderiza as variáveis no User Prompt Template
        var renderedUserPrompt = RenderVariables(command.UserPromptTemplate, command.TestVariablesJson);

        // Extrai parâmetros
        double temperature = 0.5;
        int maxTokens = 2000;
        bool responseFormatJson = true;

        try
        {
            if (!string.IsNullOrWhiteSpace(command.ParametersJson))
            {
                using var jsonDoc = JsonDocument.Parse(command.ParametersJson);
                var root = jsonDoc.RootElement;
                if (root.TryGetProperty("Temperature", out var tempProp) && tempProp.TryGetDouble(out var t))
                    temperature = t;
                if (root.TryGetProperty("MaxTokens", out var maxTokensProp) && maxTokensProp.TryGetInt32(out var m))
                    maxTokens = m;
                if (root.TryGetProperty("ResponseFormatJson", out var jsonProp))
                    responseFormatJson = jsonProp.GetBoolean();
            }
        }
        catch
        {
            // Fallback para valores padrão se o JSON de parâmetros for inválido
        }

        var request = new LLMRequest
        {
            SystemPrompt = command.SystemPrompt,
            UserPrompt = renderedUserPrompt,
            Temperature = temperature,
            MaxTokens = maxTokens,
            ResponseFormatJson = responseFormatJson
        };

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var responseResult = await _llmProvider.CompleteAsync(request, cancellationToken);
            stopwatch.Stop();

            if (responseResult.IsFailure)
            {
                if (_telemetryService != null)
                {
                    await _telemetryService.RecordExecutionAsync(
                        Guid.Empty,
                        command.AgentName ?? "PlaygroundTest",
                        request.Model,
                        "Playground",
                        0, 0,
                        stopwatch.ElapsedMilliseconds,
                        AgentExecutionStatus.Failure,
                        responseResult.Error.Message,
                        cancellationToken);
                }

                var failureResult = new TestPromptResultDto(
                    Success: false,
                    ResponseContent: string.Empty,
                    LatencyMs: stopwatch.ElapsedMilliseconds,
                    PromptTokens: 0,
                    CompletionTokens: 0,
                    ErrorMessage: responseResult.Error.Message);

                return Result.Success(failureResult);
            }

            var response = responseResult.Value;
            if (_telemetryService != null)
            {
                await _telemetryService.RecordExecutionAsync(
                    Guid.Empty,
                    command.AgentName ?? "PlaygroundTest",
                    response.ModelUsed,
                    response.ProviderType.ToString(),
                    response.PromptTokens,
                    response.CompletionTokens,
                    stopwatch.ElapsedMilliseconds > 0 ? stopwatch.ElapsedMilliseconds : response.LatencyMs,
                    AgentExecutionStatus.Success,
                    null,
                    cancellationToken);
            }

            var result = new TestPromptResultDto(
                Success: true,
                ResponseContent: response.Content,
                LatencyMs: stopwatch.ElapsedMilliseconds > 0 ? stopwatch.ElapsedMilliseconds : response.LatencyMs,
                PromptTokens: response.PromptTokens,
                CompletionTokens: response.CompletionTokens);

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            var result = new TestPromptResultDto(
                Success: false,
                ResponseContent: string.Empty,
                LatencyMs: stopwatch.ElapsedMilliseconds,
                PromptTokens: 0,
                CompletionTokens: 0,
                ErrorMessage: ex.Message);

            return Result.Success(result);
        }
    }

    private static string RenderVariables(string template, string variablesJson)
    {
        if (string.IsNullOrWhiteSpace(variablesJson))
            return template;

        var rendered = template;
        try
        {
            using var jsonDoc = JsonDocument.Parse(variablesJson);
            foreach (var prop in jsonDoc.RootElement.EnumerateObject())
            {
                var placeholder = "{{" + prop.Name + "}}";
                var val = prop.Value.ValueKind == JsonValueKind.String
                    ? prop.Value.GetString() ?? ""
                    : prop.Value.GetRawText();
                rendered = rendered.Replace(placeholder, val, StringComparison.OrdinalIgnoreCase);
            }
        }
        catch
        {
            // Mantém template sem substituição se o JSON for inválido
        }

        return rendered;
    }
}
