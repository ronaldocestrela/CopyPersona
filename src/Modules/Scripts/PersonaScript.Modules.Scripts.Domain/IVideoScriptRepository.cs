namespace PersonaScript.Modules.Scripts.Domain;

public interface IVideoScriptRepository
{
    Task AddAsync(VideoScript script, CancellationToken cancellationToken = default);
    Task<VideoScript?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VideoScript>> ListByTenantIdAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VideoScript>> ListByStatusAsync(VideoScriptStatus status, CancellationToken cancellationToken = default);
    void Update(VideoScript script);
    Task UpdateAsync(VideoScript script, CancellationToken cancellationToken = default);
    void Delete(VideoScript script);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
