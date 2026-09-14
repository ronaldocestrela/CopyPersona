using Microsoft.Extensions.Caching.Memory;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;

namespace PersonaScript.Modules.Backoffice.Infrastructure.Repositories;

public sealed class CachedForbiddenTermRepository : IForbiddenTermRepository
{
    private readonly IForbiddenTermRepository _innerRepository;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan DefaultSlidingExpiration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan DefaultAbsoluteExpiration = TimeSpan.FromHours(2);

    private const string AllActiveCacheKey = "forbidden_terms:all_active";

    public CachedForbiddenTermRepository(
        IForbiddenTermRepository innerRepository,
        IMemoryCache cache)
    {
        _innerRepository = innerRepository;
        _cache = cache;
    }

    public async Task<IReadOnlyList<ForbiddenTerm>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(AllActiveCacheKey, async entry =>
        {
            entry.SlidingExpiration = DefaultSlidingExpiration;
            entry.AbsoluteExpirationRelativeToNow = DefaultAbsoluteExpiration;
            return await _innerRepository.GetAllActiveAsync(cancellationToken);
        }) ?? Array.Empty<ForbiddenTerm>();
    }

    public Task<IReadOnlyList<ForbiddenTerm>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _innerRepository.GetAllAsync(cancellationToken);
    }

    public Task<ForbiddenTerm?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _innerRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task AddAsync(ForbiddenTerm term, CancellationToken cancellationToken = default)
    {
        await _innerRepository.AddAsync(term, cancellationToken);
        InvalidateCache();
    }

    public async Task UpdateAsync(ForbiddenTerm term, CancellationToken cancellationToken = default)
    {
        await _innerRepository.UpdateAsync(term, cancellationToken);
        InvalidateCache();
    }

    public async Task DeleteAsync(ForbiddenTerm term, CancellationToken cancellationToken = default)
    {
        await _innerRepository.DeleteAsync(term, cancellationToken);
        InvalidateCache();
    }

    private void InvalidateCache()
    {
        _cache.Remove(AllActiveCacheKey);
    }
}
