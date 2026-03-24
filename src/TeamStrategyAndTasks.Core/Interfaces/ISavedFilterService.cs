namespace TeamStrategyAndTasks.Core.Interfaces;

public interface ISavedFilterService
{
    Task<IReadOnlyList<SavedFilter>> GetForUserAsync(Guid userId, string pageKey, CancellationToken ct = default);

    /// <summary>Creates or updates (by name) a saved filter for the user on the given page.</summary>
    Task<SavedFilter> SaveAsync(Guid userId, string pageKey, string name, string filterJson, CancellationToken ct = default);

    Task DeleteAsync(Guid filterId, Guid userId, CancellationToken ct = default);
}
