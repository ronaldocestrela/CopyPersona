namespace PersonaScript.Modules.Backoffice.Application.DTOs;

public record TenantSummaryDto(
    Guid TenantId,
    string FullName,
    string Email,
    string Role,
    string PlanName,
    string SubscriptionStatus,
    DateTimeOffset CreatedAt,
    bool IsFrozen,
    string? FreezeReason,
    int ScriptsGeneratedCount,
    int ScriptsLimit,
    int AiAnalysesCount,
    int AiAnalysesLimit);

public record AnamneseInfoDto(
    string Profissao,
    string Especialidade,
    string Nicho,
    string PublicoAlvo,
    string TomDeVoz,
    string PrincipalObjetivo,
    DateTime CompletedAt);

public record TenantDetailsDto(
    TenantSummaryDto Summary,
    AnamneseInfoDto? Anamnese,
    int DiagnosesCount,
    int ScriptsCount,
    IReadOnlyList<AuditLogDto> AuditHistory);

public record AuditLogDto(
    Guid Id,
    string ActionType,
    Guid AdminUserId,
    string AdminEmail,
    Guid TargetTenantId,
    string TargetUserEmail,
    string DetailsJson,
    DateTimeOffset Timestamp);
