namespace PersonaScript.Modules.Identity.Application.Abstractions;

public sealed record JwtTokenResult(
    string AccessToken,
    string TokenType,
    int ExpiresInSeconds,
    Guid UserId,
    Guid TenantId);
