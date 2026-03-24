using Microsoft.EntityFrameworkCore;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Data;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class ProgressWriteBackService(
    IDbContextFactory<AppDbContext> dbFactory,
    INotificationService notifications) : IProgressWriteBackService
{
    public async Task RecalculateFromTaskAsync(Guid taskId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var parentInitiativeIds = await db.InitiativeWorkTasks
            .Where(it => it.WorkTaskId == taskId)
            .Select(it => it.InitiativeId)
            .ToListAsync(ct);

        foreach (var id in parentInitiativeIds)
            await RecalculateInitiativeAsync(db, id, ct);
    }

    public async Task RecalculateFromInitiativeAsync(Guid initiativeId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        await RecalculateInitiativeAsync(db, initiativeId, ct);
    }

    // ── Private cascade methods ──────────────────────────────────────────────

    private async Task RecalculateInitiativeAsync(AppDbContext db, Guid id, CancellationToken ct)
    {
        var initiative = await db.Initiatives
            .Include(i => i.InitiativeWorkTasks).ThenInclude(it => it.WorkTask)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

        if (initiative is null) return;

        if (IsAutoStatus(initiative.Status))
        {
            var tasks = initiative.InitiativeWorkTasks
                .Where(it => it.WorkTask is not null && !it.WorkTask.IsArchived)
                .Select(it => it.WorkTask!)
                .ToList();

            var newStatus = StatusFromProgress(ComputeProgress(tasks));
            if (newStatus != initiative.Status)
            {
                initiative.Status = newStatus;
                await db.SaveChangesAsync(ct);
            }
        }

        // Always cascade up — process progress may have shifted even if initiative status bucket didn't change
        var parentProcessIds = await db.ProcessInitiatives
            .Where(pi => pi.InitiativeId == id)
            .Select(pi => pi.ProcessId)
            .ToListAsync(ct);

        foreach (var pid in parentProcessIds)
            await RecalculateProcessAsync(db, pid, ct);
    }

    private async Task RecalculateProcessAsync(AppDbContext db, Guid id, CancellationToken ct)
    {
        var process = await db.BusinessProcesses
            .Include(p => p.ProcessInitiatives).ThenInclude(pi => pi.Initiative)
                .ThenInclude(i => i.InitiativeWorkTasks).ThenInclude(it => it.WorkTask)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (process is null) return;

        if (IsAutoStatus(process.Status))
        {
            var newStatus = StatusFromProgress(ComputeProcessProgress(process));
            if (newStatus != process.Status)
            {
                process.Status = newStatus;
                await db.SaveChangesAsync(ct);
            }
        }

        var parentObjectiveIds = await db.ObjectiveProcesses
            .Where(op => op.ProcessId == id)
            .Select(op => op.ObjectiveId)
            .ToListAsync(ct);

        foreach (var oid in parentObjectiveIds)
            await RecalculateObjectiveAsync(db, oid, ct);
    }

    private async Task RecalculateObjectiveAsync(AppDbContext db, Guid id, CancellationToken ct)
    {
        var objective = await db.Objectives
            .Include(o => o.ObjectiveProcesses).ThenInclude(op => op.Process)
                .ThenInclude(p => p.ProcessInitiatives).ThenInclude(pi => pi.Initiative)
                    .ThenInclude(i => i.InitiativeWorkTasks).ThenInclude(it => it.WorkTask)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (objective is null) return;

        if (IsAutoStatus(objective.Status))
        {
            var newStatus = StatusFromProgress(ComputeObjectiveProgress(objective));
            if (newStatus != objective.Status)
            {
                objective.Status = newStatus;
                await db.SaveChangesAsync(ct);
            }
        }
    }

    // ── Progress / status helpers ────────────────────────────────────────────

    private static bool IsAutoStatus(NodeStatus s) =>
        s is NodeStatus.NotStarted or NodeStatus.Active or NodeStatus.InProgress
            or NodeStatus.OnTrack or NodeStatus.Complete;

    private static int? ComputeProgress(IEnumerable<WorkTask> tasks)
    {
        var active = tasks.Where(t => !t.IsArchived).ToList();
        if (active.Count == 0) return null;
        return (int)Math.Round(active.Average(t =>
            t.Status is NodeStatus.Done or NodeStatus.Complete ? 100.0 : 0.0));
    }

    private static int? ComputeProcessProgress(BusinessProcess process)
    {
        var initiatives = process.ProcessInitiatives
            .Where(pi => pi.Initiative is not null && !pi.Initiative.IsArchived)
            .Select(pi => pi.Initiative!)
            .ToList();
        if (initiatives.Count == 0) return null;

        var scores = initiatives
            .Select(i => ComputeProgress(
                i.InitiativeWorkTasks.Where(it => it.WorkTask is not null).Select(it => it.WorkTask!)))
            .OfType<int>()
            .ToList();

        return scores.Count == 0 ? null : (int)Math.Round(scores.Average());
    }

    private static int? ComputeObjectiveProgress(Objective objective)
    {
        var processes = objective.ObjectiveProcesses
            .Where(op => op.Process is not null && !op.Process.IsArchived)
            .Select(op => op.Process!)
            .ToList();
        if (processes.Count == 0) return null;

        var scores = processes.Select(ComputeProcessProgress).OfType<int>().ToList();
        return scores.Count == 0 ? null : (int)Math.Round(scores.Average());
    }

    private static NodeStatus StatusFromProgress(int? pct) => pct switch
    {
        null or 0 => NodeStatus.NotStarted,
        100       => NodeStatus.Complete,
        >= 50     => NodeStatus.OnTrack,
        _         => NodeStatus.Active
    };

    // ── Milestone missed detection ────────────────────────────────────────────

    public async Task CheckMissedMilestonesAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var now = DateTimeOffset.UtcNow;

        var overdue = await db.Milestones
            .Where(m => m.Status == MilestoneStatus.Pending && m.DueDate < now)
            .Include(m => m.Initiative)
            .ToListAsync(ct);

        foreach (var milestone in overdue)
        {
            milestone.Status = MilestoneStatus.Missed;
            await db.SaveChangesAsync(ct);

            // Notify the Initiative owner
            await notifications.CreateAsync(
                milestone.Initiative.OwnerId,
                $"Milestone \u201c{milestone.Title}\u201d on initiative \u201c{milestone.Initiative.Title}\u201d has been marked Missed (due {milestone.DueDate:yyyy-MM-dd}).",
                NodeType.Initiative,
                milestone.InitiativeId,
                ct);
        }
    }
}
