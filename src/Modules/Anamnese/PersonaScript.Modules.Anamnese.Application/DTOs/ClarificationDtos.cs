namespace PersonaScript.Modules.Anamnese.Application.DTOs;

public record ClarificationItemDto(
    string QuestionId,
    string FieldName,
    string CurrentAnswer,
    string ReasonVague,
    string SuggestionTitle,
    string SuggestionPrompt,
    string ExampleAnswer
);

public record ClarificationAnalysisResultDto(
    bool IsVague,
    List<ClarificationItemDto> Items
);
