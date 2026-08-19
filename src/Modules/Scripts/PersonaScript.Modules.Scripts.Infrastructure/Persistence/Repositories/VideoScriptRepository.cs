using Microsoft.EntityFrameworkCore;
using PersonaScript.Modules.Scripts.Domain;

namespace PersonaScript.Modules.Scripts.Infrastructure.Persistence.Repositories;

public sealed class VideoScriptRepository : IVideoScriptRepository
{
    private readonly ScriptsDbContext _context;

    public VideoScriptRepository(ScriptsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(VideoScript script, CancellationToken cancellationToken = default)
    {
        await _context.VideoScripts.AddAsync(script, cancellationToken);
    }

    public async Task<VideoScript?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.VideoScripts
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<VideoScript>> ListByTenantIdAsync(CancellationToken cancellationToken = default)
    {
        return await _context.VideoScripts
            .OrderByDescending(s => s.GeradoEm)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<VideoScript>> ListByStatusAsync(VideoScriptStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.VideoScripts
            .Where(s => s.Status == status)
            .OrderByDescending(s => s.GeradoEm)
            .ToListAsync(cancellationToken);
    }

    public void Update(VideoScript script)
    {
        _context.VideoScripts.Update(script);
    }

    public async Task UpdateAsync(VideoScript script, CancellationToken cancellationToken = default)
    {
        _context.VideoScripts.Update(script);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Delete(VideoScript script)
    {
        _context.VideoScripts.Remove(script);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
