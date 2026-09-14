using Microsoft.Extensions.Caching.Memory;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;

namespace PersonaScript.Modules.Backoffice.Infrastructure.Repositories;

public sealed class CachedCouncilRuleRepository : ICouncilRuleRepository
{
    private readonly ICouncilRuleRepository _innerRepository;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan DefaultSlidingExpiration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan DefaultAbsoluteExpiration = TimeSpan.FromHours(2);

    private const string AllActiveCacheKey = "council:rules:all_active";

    public CachedCouncilRuleRepository(
        ICouncilRuleRepository innerRepository,
        IMemoryCache cache)
    {
        _innerRepository = innerRepository;
        _cache = cache;
    }

    public async Task<CouncilRule?> GetByAcronymAsync(string councilAcronym, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(councilAcronym))
            return null;

        var key = GetAcronymKey(councilAcronym);
        return await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.SlidingExpiration = DefaultSlidingExpiration;
            entry.AbsoluteExpirationRelativeToNow = DefaultAbsoluteExpiration;
            return await _innerRepository.GetByAcronymAsync(councilAcronym, cancellationToken);
        });
    }

    public async Task<IReadOnlyList<CouncilRule>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(AllActiveCacheKey, async entry =>
        {
            entry.SlidingExpiration = DefaultSlidingExpiration;
            entry.AbsoluteExpirationRelativeToNow = DefaultAbsoluteExpiration;
            return await _innerRepository.GetAllActiveAsync(cancellationToken);
        }) ?? Array.Empty<CouncilRule>();
    }

    public Task<IReadOnlyList<CouncilRule>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _innerRepository.GetAllAsync(cancellationToken);
    }

    public Task<CouncilRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _innerRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task AddAsync(CouncilRule rule, CancellationToken cancellationToken = default)
    {
        await _innerRepository.AddAsync(rule, cancellationToken);
        InvalidateCache(rule.CouncilAcronym);
    }

    public async Task UpdateAsync(CouncilRule rule, CancellationToken cancellationToken = default)
    {
        await _innerRepository.UpdateAsync(rule, cancellationToken);
        InvalidateCache(rule.CouncilAcronym);
    }

    private void InvalidateCache(string councilAcronym)
    {
        _cache.Remove(GetAcronymKey(councilAcronym));
        _cache.Remove(AllActiveCacheKey);
    }

    private static string GetAcronymKey(string councilAcronym) =>
        $"council:rule:{councilAcronym.Trim().ToUpperInvariant()}";
}
