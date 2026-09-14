using Microsoft.Extensions.Caching.Memory;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;

namespace PersonaScript.Modules.Backoffice.Infrastructure.Repositories;

public sealed class CachedPromptTemplateRepository : IPromptTemplateRepository
{
    private readonly IPromptTemplateRepository _innerRepository;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan DefaultSlidingExpiration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan DefaultAbsoluteExpiration = TimeSpan.FromHours(2);

    private const string AllActiveCacheKey = "prompt:all_active";

    public CachedPromptTemplateRepository(
        IPromptTemplateRepository innerRepository,
        IMemoryCache cache)
    {
        _innerRepository = innerRepository;
        _cache = cache;
    }

    public async Task<PromptTemplate?> GetActiveByAgentNameAsync(string agentName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(agentName))
            return null;

        var key = GetActiveKey(agentName);
        return await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.SlidingExpiration = DefaultSlidingExpiration;
            entry.AbsoluteExpirationRelativeToNow = DefaultAbsoluteExpiration;
            return await _innerRepository.GetActiveByAgentNameAsync(agentName, cancellationToken);
        });
    }

    public async Task<IReadOnlyList<PromptTemplate>> GetAllActivePromptsAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(AllActiveCacheKey, async entry =>
        {
            entry.SlidingExpiration = DefaultSlidingExpiration;
            entry.AbsoluteExpirationRelativeToNow = DefaultAbsoluteExpiration;
            return await _innerRepository.GetAllActivePromptsAsync(cancellationToken);
        }) ?? Array.Empty<PromptTemplate>();
    }

    public Task<IReadOnlyList<PromptTemplate>> GetAllVersionsByAgentNameAsync(string agentName, CancellationToken cancellationToken = default)
    {
        return _innerRepository.GetAllVersionsByAgentNameAsync(agentName, cancellationToken);
    }

    public Task<PromptTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _innerRepository.GetByIdAsync(id, cancellationToken);
    }

    public Task<int> GetLatestVersionNumberAsync(string agentName, CancellationToken cancellationToken = default)
    {
        return _innerRepository.GetLatestVersionNumberAsync(agentName, cancellationToken);
    }

    public async Task AddAsync(PromptTemplate template, CancellationToken cancellationToken = default)
    {
        await _innerRepository.AddAsync(template, cancellationToken);
        InvalidateCache(template.AgentName);
    }

    public async Task UpdateAsync(PromptTemplate template, CancellationToken cancellationToken = default)
    {
        await _innerRepository.UpdateAsync(template, cancellationToken);
        InvalidateCache(template.AgentName);
    }

    private void InvalidateCache(string agentName)
    {
        _cache.Remove(GetActiveKey(agentName));
        _cache.Remove(AllActiveCacheKey);
    }

    private static string GetActiveKey(string agentName) =>
        $"prompt:active:{agentName.Trim().ToLowerInvariant()}";
}
