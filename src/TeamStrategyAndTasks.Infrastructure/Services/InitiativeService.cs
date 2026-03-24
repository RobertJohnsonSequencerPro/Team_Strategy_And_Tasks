using Microsoft.EntityFrameworkCore;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Exceptions;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Data;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class InitiativeService(IDbContextFactory<AppDbContext> dbFactory, IProgressWriteBackService writeBack, IAuditService audit, IWebhookService webhooks) : IInitiativeService
{
    public async Task<IReadOnlyList<Initiative>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.Initiatives
            .AsNoTracking()
            .AsSplitQuery()
            .Where(i => !i.IsArchived)
            .Include(i => i.ProcessInitiatives.Where(pi => !pi.Process.IsArchived)).ThenInclude(pi => pi.Process)
            .Include(i => i.InitiativeWorkTasks.Where(it => !it.WorkTask.IsArchived)).ThenInclude(it => it.WorkTask)
            .OrderBy(i => i.Title)
            .ToListAsync(ct);
    }

    public async Task<Initiative> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var initiative = await db.Initiatives
            .AsNoTracking()
            .AsSplitQuery()
            .Where(i => !i.IsArchived)
            .Include(i => i.ProcessInitiatives.Where(pi => !pi.Process.IsArchived)).ThenInclude(pi => pi.Process)
            .Include(i => i.InitiativeWorkTasks.Where(it => !it.WorkTask.IsArchived)).ThenInclude(it => it.WorkTask)
            .Include(i => i.Team)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
        return initiative ?? throw new NotFoundException(nameof(Initiative), id);
    }

    public async Task<Initiative> CreateAsync(CreateInitiativeRequest request, Guid ownerId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var initiative = new Initiative
        {
            Title = request.Title,
            Description = request.Description,
            OwnerId = ownerId,
            TargetDate = request.TargetDate
        };
        db.Initiatives.Add(initiative);
        await db.SaveChangesAsync(ct);
        await webhooks.FireAsync(WebhookEventType.NodeCreated, NodeType.Initiative, initiative.Id, initiative.Title, null, initiative.Status.ToString(), ownerId, ct);
        return initiative;
    }

    public async Task<Initiative> UpdateAsync(Guid id, UpdateInitiativeRequest request, Guid performedByUserId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var initiative = await db.Initiatives.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(Initiative), id);
        var logs = new List<(string Field, string? Old, string? New)>();
        if (initiative.Title != request.Title) logs.Add(("Title", initiative.Title, request.Title));
        if (initiative.Description != request.Description) logs.Add(("Description", initiative.Description, request.Description));
        if (initiative.TargetDate != request.TargetDate) logs.Add(("TargetDate", initiative.TargetDate?.ToString("o"), request.TargetDate?.ToString("o")));
        if (initiative.Status != request.Status) logs.Add(("Status", initiative.Status.ToString(), request.Status.ToString()));

        initiative.Title = request.Title;
        initiative.Description = request.Description;
        initiative.TargetDate = request.TargetDate;
        initiative.Status = request.Status;
        await db.SaveChangesAsync(ct);

        foreach (var (field, old, next) in logs)
            await audit.LogAsync(NodeType.Initiative, id, performedByUserId, field, old, next, ct);

        var statusLog = logs.FirstOrDefault(l => l.Field == "Status");
        if (statusLog != default)
            await webhooks.FireAsync(WebhookEventType.StatusChanged, NodeType.Initiative, id, initiative.Title, statusLog.Old, statusLog.New, performedByUserId, ct);

        return initiative;
    }

    public async Task ArchiveAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var initiative = await db.Initiatives.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(Initiative), id);
        initiative.IsArchived = true;
        await db.SaveChangesAsync(ct);
        await webhooks.FireAsync(WebhookEventType.NodeArchived, NodeType.Initiative, id, initiative.Title, null, null, null, ct);
    }

    public async Task LinkTaskAsync(Guid initiativeId, Guid taskId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var alreadyLinked = await db.InitiativeWorkTasks
            .AnyAsync(it => it.InitiativeId == initiativeId && it.WorkTaskId == taskId, ct);
        if (alreadyLinked) return;

        db.InitiativeWorkTasks.Add(new InitiativeWorkTask { InitiativeId = initiativeId, WorkTaskId = taskId });
        await db.SaveChangesAsync(ct);
        await writeBack.RecalculateFromInitiativeAsync(initiativeId, ct);
    }

    public async Task UnlinkTaskAsync(Guid initiativeId, Guid taskId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var link = await db.InitiativeWorkTasks
            .FirstOrDefaultAsync(it => it.InitiativeId == initiativeId && it.WorkTaskId == taskId, ct);
        if (link is not null)
        {
            db.InitiativeWorkTasks.Remove(link);
            await db.SaveChangesAsync(ct);
            await writeBack.RecalculateFromInitiativeAsync(initiativeId, ct);
        }
    }

    public async Task SetResponsibleTeamAsync(Guid id, Guid? teamId, Guid performedByUserId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var initiative = await db.Initiatives.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(Initiative), id);
        if (initiative.TeamId == teamId) return;
        var old = initiative.TeamId?.ToString();
        initiative.TeamId = teamId;
        await db.SaveChangesAsync(ct);
        await audit.LogAsync(NodeType.Initiative, id, performedByUserId, "TeamId", old, teamId?.ToString(), ct);
    }
}
