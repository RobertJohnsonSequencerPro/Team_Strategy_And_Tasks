using Microsoft.EntityFrameworkCore;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Exceptions;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Data;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class ObjectiveService(AppDbContext db) : IObjectiveService
{
    public async Task<IReadOnlyList<Objective>> GetAllAsync(CancellationToken ct = default) =>
        await db.Objectives
            .Where(o => !o.IsArchived)
            .Include(o => o.ObjectiveProcesses).ThenInclude(op => op.Process)
            .OrderBy(o => o.Title)
            .ToListAsync(ct);

    public async Task<Objective> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var obj = await db.Objectives
            .Include(o => o.ObjectiveProcesses).ThenInclude(op => op.Process)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
        return obj ?? throw new NotFoundException(nameof(Objective), id);
    }

    public async Task<Objective> CreateAsync(CreateObjectiveRequest request, Guid ownerId, CancellationToken ct = default)
    {
        var objective = new Objective
        {
            Title = request.Title,
            Description = request.Description,
            OwnerId = ownerId,
            SuccessMetric = request.SuccessMetric,
            TargetValue = request.TargetValue,
            TargetDate = request.TargetDate
        };
        db.Objectives.Add(objective);
        await db.SaveChangesAsync(ct);
        return objective;
    }

    public async Task<Objective> UpdateAsync(Guid id, UpdateObjectiveRequest request, CancellationToken ct = default)
    {
        var objective = await GetByIdAsync(id, ct);
        objective.Title = request.Title;
        objective.Description = request.Description;
        objective.SuccessMetric = request.SuccessMetric;
        objective.TargetValue = request.TargetValue;
        objective.TargetDate = request.TargetDate;
        objective.Status = request.Status;
        await db.SaveChangesAsync(ct);
        return objective;
    }

    public async Task ArchiveAsync(Guid id, CancellationToken ct = default)
    {
        var objective = await GetByIdAsync(id, ct);
        objective.IsArchived = true;
        await db.SaveChangesAsync(ct);
    }

    public async Task LinkProcessAsync(Guid objectiveId, Guid processId, CancellationToken ct = default)
    {
        var alreadyLinked = await db.ObjectiveProcesses
            .AnyAsync(op => op.ObjectiveId == objectiveId && op.ProcessId == processId, ct);
        if (alreadyLinked) return;

        db.ObjectiveProcesses.Add(new ObjectiveProcess { ObjectiveId = objectiveId, ProcessId = processId });
        await db.SaveChangesAsync(ct);
    }

    public async Task UnlinkProcessAsync(Guid objectiveId, Guid processId, CancellationToken ct = default)
    {
        var link = await db.ObjectiveProcesses
            .FirstOrDefaultAsync(op => op.ObjectiveId == objectiveId && op.ProcessId == processId, ct);
        if (link is not null)
        {
            db.ObjectiveProcesses.Remove(link);
            await db.SaveChangesAsync(ct);
        }
    }
}
