using Microsoft.EntityFrameworkCore;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;
using PersonaScript.Modules.Backoffice.Infrastructure.Persistence;

namespace PersonaScript.Modules.Backoffice.Infrastructure.Repositories;

public sealed class PromptTemplateRepository : IPromptTemplateRepository
{
    private readonly BackofficeDbContext _dbContext;

    public PromptTemplateRepository(BackofficeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PromptTemplate?> GetActiveByAgentNameAsync(string agentName, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PromptTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.AgentName == agentName && p.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyList<PromptTemplate>> GetAllVersionsByAgentNameAsync(string agentName, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PromptTemplates
            .AsNoTracking()
            .Where(p => p.AgentName == agentName)
            .OrderByDescending(p => p.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PromptTemplate>> GetAllActivePromptsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.PromptTemplates
            .AsNoTracking()
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<PromptTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PromptTemplates
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<int> GetLatestVersionNumberAsync(string agentName, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PromptTemplates
            .Where(p => p.AgentName == agentName)
            .Select(p => (int?)p.Version)
            .MaxAsync(cancellationToken) ?? 0;
    }

    public async Task AddAsync(PromptTemplate template, CancellationToken cancellationToken = default)
    {
        await _dbContext.PromptTemplates.AddAsync(template, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(PromptTemplate template, CancellationToken cancellationToken = default)
    {
        _dbContext.PromptTemplates.Update(template);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
