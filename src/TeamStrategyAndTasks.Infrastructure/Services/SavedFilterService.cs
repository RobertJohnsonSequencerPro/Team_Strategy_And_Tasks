using TeamStrategyAndTasks.Core.Interfaces;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class SavedFilterService(AppDbContext db) : ISavedFilterService
{
    public async Task<IReadOnlyList<SavedFilter>> GetForUserAsync(
        Guid userId, string pageKey, CancellationToken ct = default)
        => await db.SavedFilters
            .Where(f => f.UserId == userId && f.PageKey == pageKey)
            .OrderBy(f => f.Name)
            .ToListAsync(ct);

    public async Task<SavedFilter> SaveAsync(
        Guid userId, string pageKey, string name, string filterJson, CancellationToken ct = default)
    {
        // Upsert: same user + page + name → overwrite the JSON
        var existing = await db.SavedFilters
            .FirstOrDefaultAsync(f => f.UserId == userId && f.PageKey == pageKey && f.Name == name, ct);

        if (existing is not null)
        {
            existing.FilterJson = filterJson;
        }
        else
        {
            existing = new SavedFilter
            {
                UserId = userId,
                PageKey = pageKey,
                Name = name,
                FilterJson = filterJson
            };
            db.SavedFilters.Add(existing);
        }

        await db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task DeleteAsync(Guid filterId, Guid userId, CancellationToken ct = default)
    {
        var filter = await db.SavedFilters.FindAsync([filterId], ct)
            ?? throw new NotFoundException("Saved filter not found.");
        if (filter.UserId != userId) throw new ForbiddenException();
        db.SavedFilters.Remove(filter);
        await db.SaveChangesAsync(ct);
    }
}
