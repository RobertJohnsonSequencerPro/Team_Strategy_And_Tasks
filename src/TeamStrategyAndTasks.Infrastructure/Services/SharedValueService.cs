using Microsoft.EntityFrameworkCore;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Exceptions;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Data;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class SharedValueService(IDbContextFactory<AppDbContext> dbFactory) : ISharedValueService
{
    public async Task<IReadOnlyList<SharedValue>> GetAllActiveAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.SharedValues
            .AsNoTracking()
            .Where(v => !v.IsArchived)
            .OrderBy(v => v.DisplayOrder)
            .ThenBy(v => v.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SharedValue>> GetAllAdminAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.SharedValues
            .AsNoTracking()
            .OrderBy(v => v.IsArchived)
            .ThenBy(v => v.DisplayOrder)
            .ThenBy(v => v.Name)
            .ToListAsync(ct);
    }

    public async Task<SharedValue> CreateAsync(CreateSharedValueRequest request, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var nextOrder = await db.SharedValues
            .Where(v => !v.IsArchived)
            .Select(v => (int?)v.DisplayOrder)
            .MaxAsync(ct) ?? 0;

        var entity = new SharedValue
        {
            Name = request.Name.Trim(),
            Definition = request.Definition.Trim(),
            DisplayOrder = nextOrder + 1
        };

        db.SharedValues.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<SharedValue> UpdateAsync(Guid id, UpdateSharedValueRequest request, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var entity = await db.SharedValues.FindAsync([id], ct) ?? throw new NotFoundException(nameof(SharedValue), id);

        entity.Name = request.Name.Trim();
        entity.Definition = request.Definition.Trim();

        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task ArchiveAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var entity = await db.SharedValues.FindAsync([id], ct) ?? throw new NotFoundException(nameof(SharedValue), id);

        if (entity.IsArchived) return;

        entity.IsArchived = true;

        await NormalizeDisplayOrderAsync(db, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task MoveUpAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var rows = await db.SharedValues
            .Where(v => !v.IsArchived)
            .OrderBy(v => v.DisplayOrder)
            .ThenBy(v => v.Name)
            .ToListAsync(ct);

        var index = rows.FindIndex(v => v.Id == id);
        if (index <= 0) return;

        (rows[index - 1].DisplayOrder, rows[index].DisplayOrder) =
            (rows[index].DisplayOrder, rows[index - 1].DisplayOrder);

        await db.SaveChangesAsync(ct);
        await NormalizeDisplayOrderAsync(db, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task MoveDownAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var rows = await db.SharedValues
            .Where(v => !v.IsArchived)
            .OrderBy(v => v.DisplayOrder)
            .ThenBy(v => v.Name)
            .ToListAsync(ct);

        var index = rows.FindIndex(v => v.Id == id);
        if (index < 0 || index >= rows.Count - 1) return;

        (rows[index + 1].DisplayOrder, rows[index].DisplayOrder) =
            (rows[index].DisplayOrder, rows[index + 1].DisplayOrder);

        await db.SaveChangesAsync(ct);
        await NormalizeDisplayOrderAsync(db, ct);
        await db.SaveChangesAsync(ct);
    }

    private static async Task NormalizeDisplayOrderAsync(AppDbContext db, CancellationToken ct)
    {
        var activeRows = await db.SharedValues
            .Where(v => !v.IsArchived)
            .OrderBy(v => v.DisplayOrder)
            .ThenBy(v => v.Name)
            .ToListAsync(ct);

        for (var i = 0; i < activeRows.Count; i++)
            activeRows[i].DisplayOrder = i + 1;
    }
}
