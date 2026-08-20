namespace PersonaScript.BuildingBlocks.Results;

public sealed record Error(string Code, string Message)
{
    public static Error None => new(string.Empty, string.Empty);

    public static Error Validation(string code, string message) => new(code, message);

    public static Error NotFound(string code, string message) => new(code, message);

    public static Error Unauthorized(string code, string message) => new(code, message);

    public static Error Failure(string code, string message) => new(code, message);
}
