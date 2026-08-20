namespace PersonaScript.Modules.Backoffice.Domain.Repositories;

public interface IPromptTemplateRepository
{
    Task<PromptTemplate?> GetActiveByAgentNameAsync(string agentName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PromptTemplate>> GetAllVersionsByAgentNameAsync(string agentName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PromptTemplate>> GetAllActivePromptsAsync(CancellationToken cancellationToken = default);
    Task<PromptTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> GetLatestVersionNumberAsync(string agentName, CancellationToken cancellationToken = default);
    Task AddAsync(PromptTemplate template, CancellationToken cancellationToken = default);
    Task UpdateAsync(PromptTemplate template, CancellationToken cancellationToken = default);
}
