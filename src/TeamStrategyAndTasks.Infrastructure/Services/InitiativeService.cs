using Microsoft.EntityFrameworkCore;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Exceptions;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Data;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class InitiativeService(AppDbContext db, IProgressWriteBackService writeBack) : IInitiativeService
{
    public async Task<IReadOnlyList<Initiative>> GetAllAsync(CancellationToken ct = default) =>
        await db.Initiatives
            .Where(i => !i.IsArchived)
            .Include(i => i.ProcessInitiatives).ThenInclude(pi => pi.Process)
            .Include(i => i.InitiativeWorkTasks).ThenInclude(it => it.WorkTask)
            .OrderBy(i => i.Title)
            .ToListAsync(ct);

    public async Task<Initiative> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var initiative = await db.Initiatives
            .Include(i => i.ProcessInitiatives).ThenInclude(pi => pi.Process)
            .Include(i => i.InitiativeWorkTasks).ThenInclude(it => it.WorkTask)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
        return initiative ?? throw new NotFoundException(nameof(Initiative), id);
    }

    public async Task<Initiative> CreateAsync(CreateInitiativeRequest request, Guid ownerId, CancellationToken ct = default)
    {
        var initiative = new Initiative
        {
            Title = request.Title,
            Description = request.Description,
            OwnerId = ownerId,
            TargetDate = request.TargetDate
        };
        db.Initiatives.Add(initiative);
        await db.SaveChangesAsync(ct);
        return initiative;
    }

    public async Task<Initiative> UpdateAsync(Guid id, UpdateInitiativeRequest request, CancellationToken ct = default)
    {
        var initiative = await GetByIdAsync(id, ct);
        initiative.Title = request.Title;
        initiative.Description = request.Description;
        initiative.TargetDate = request.TargetDate;
        initiative.Status = request.Status;
        await db.SaveChangesAsync(ct);
        return initiative;
    }

    public async Task ArchiveAsync(Guid id, CancellationToken ct = default)
    {
        var initiative = await GetByIdAsync(id, ct);
        initiative.IsArchived = true;
        await db.SaveChangesAsync(ct);
    }

    public async Task LinkTaskAsync(Guid initiativeId, Guid taskId, CancellationToken ct = default)
    {
        var alreadyLinked = await db.InitiativeWorkTasks
            .AnyAsync(it => it.InitiativeId == initiativeId && it.WorkTaskId == taskId, ct);
        if (alreadyLinked) return;

        db.InitiativeWorkTasks.Add(new InitiativeWorkTask { InitiativeId = initiativeId, WorkTaskId = taskId });
        await db.SaveChangesAsync(ct);
        await writeBack.RecalculateFromInitiativeAsync(initiativeId, ct);
    }

    public async Task UnlinkTaskAsync(Guid initiativeId, Guid taskId, CancellationToken ct = default)
    {
        var link = await db.InitiativeWorkTasks
            .FirstOrDefaultAsync(it => it.InitiativeId == initiativeId && it.WorkTaskId == taskId, ct);
        if (link is not null)
        {
            db.InitiativeWorkTasks.Remove(link);
            await db.SaveChangesAsync(ct);
            await writeBack.RecalculateFromInitiativeAsync(initiativeId, ct);
        }
    }
}
