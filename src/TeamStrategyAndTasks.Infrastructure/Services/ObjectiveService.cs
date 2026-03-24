using Microsoft.EntityFrameworkCore;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Exceptions;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Data;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class ObjectiveService(AppDbContext db, IAuditService audit, IWebhookService webhooks) : IObjectiveService
{
    public async Task<IReadOnlyList<Objective>> GetAllAsync(CancellationToken ct = default) =>
        await db.Objectives
            .Where(o => !o.IsArchived)
            .Include(o => o.ObjectiveProcesses).ThenInclude(op => op.Process)
            .OrderBy(o => o.Title)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Objective>> GetFullHierarchyAsync(CancellationToken ct = default) =>
        await db.Objectives
            .Where(o => !o.IsArchived)
            .Include(o => o.ObjectiveProcesses)
                .ThenInclude(op => op.Process)
                    .ThenInclude(p => p.ProcessInitiatives)
                        .ThenInclude(pi => pi.Initiative)
                            .ThenInclude(i => i.InitiativeWorkTasks)
                                .ThenInclude(iwt => iwt.WorkTask)
            .OrderBy(o => o.Title)
            .ToListAsync(ct);

    public async Task<Objective> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var obj = await db.Objectives
            .Include(o => o.ObjectiveProcesses).ThenInclude(op => op.Process)
            .Include(o => o.Team)
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
        await webhooks.FireAsync(WebhookEventType.NodeCreated, NodeType.Objective, objective.Id, objective.Title, null, objective.Status.ToString(), ownerId, ct);
        return objective;
    }

    public async Task<Objective> UpdateAsync(Guid id, UpdateObjectiveRequest request, Guid performedByUserId, CancellationToken ct = default)
    {
        var objective = await GetByIdAsync(id, ct);
        var logs = new List<(string Field, string? Old, string? New)>();
        if (objective.Title != request.Title) logs.Add(("Title", objective.Title, request.Title));
        if (objective.Description != request.Description) logs.Add(("Description", objective.Description, request.Description));
        if (objective.SuccessMetric != request.SuccessMetric) logs.Add(("SuccessMetric", objective.SuccessMetric, request.SuccessMetric));
        if (objective.TargetValue != request.TargetValue) logs.Add(("TargetValue", objective.TargetValue, request.TargetValue));
        if (objective.TargetDate != request.TargetDate) logs.Add(("TargetDate", objective.TargetDate?.ToString("o"), request.TargetDate?.ToString("o")));
        if (objective.Status != request.Status) logs.Add(("Status", objective.Status.ToString(), request.Status.ToString()));

        objective.Title = request.Title;
        objective.Description = request.Description;
        objective.SuccessMetric = request.SuccessMetric;
        objective.TargetValue = request.TargetValue;
        objective.TargetDate = request.TargetDate;
        objective.Status = request.Status;
        await db.SaveChangesAsync(ct);

        foreach (var (field, old, next) in logs)
            await audit.LogAsync(NodeType.Objective, id, performedByUserId, field, old, next, ct);

        var statusLog = logs.FirstOrDefault(l => l.Field == "Status");
        if (statusLog != default)
            await webhooks.FireAsync(WebhookEventType.StatusChanged, NodeType.Objective, id, objective.Title, statusLog.Old, statusLog.New, performedByUserId, ct);

        return objective;
    }

    public async Task ArchiveAsync(Guid id, CancellationToken ct = default)
    {
        var objective = await GetByIdAsync(id, ct);
        objective.IsArchived = true;
        await db.SaveChangesAsync(ct);
        await webhooks.FireAsync(WebhookEventType.NodeArchived, NodeType.Objective, id, objective.Title, null, null, null, ct);
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

    public async Task SetResponsibleTeamAsync(Guid id, Guid? teamId, Guid performedByUserId, CancellationToken ct = default)
    {
        var objective = await GetByIdAsync(id, ct);
        if (objective.TeamId == teamId) return;
        var old = objective.TeamId?.ToString();
        objective.TeamId = teamId;
        await db.SaveChangesAsync(ct);
        await audit.LogAsync(NodeType.Objective, id, performedByUserId, "TeamId", old, teamId?.ToString(), ct);
    }
}
