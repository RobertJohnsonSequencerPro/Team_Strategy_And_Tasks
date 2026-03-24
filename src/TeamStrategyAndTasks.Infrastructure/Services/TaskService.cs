using Microsoft.EntityFrameworkCore;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Exceptions;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Data;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class TaskService(AppDbContext db, IProgressWriteBackService writeBack, IAuditService audit) : ITaskService
{
    public async Task<IReadOnlyList<WorkTask>> GetAllAsync(CancellationToken ct = default) =>
        await db.WorkTasks
            .Where(t => !t.IsArchived)
            .Include(t => t.InitiativeWorkTasks).ThenInclude(it => it.Initiative)
            .OrderBy(t => t.Title)
            .ToListAsync(ct);

    public async Task<WorkTask> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var task = await db.WorkTasks
            .Include(t => t.InitiativeWorkTasks).ThenInclude(it => it.Initiative)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
        return task ?? throw new NotFoundException(nameof(WorkTask), id);
    }

    public async Task<WorkTask> CreateAsync(CreateTaskRequest request, Guid ownerId, CancellationToken ct = default)
    {
        var task = new WorkTask
        {
            Title = request.Title,
            Description = request.Description,
            OwnerId = ownerId,
            AssigneeId = request.AssigneeId,
            EstimatedEffort = request.EstimatedEffort,
            TargetDate = request.TargetDate
        };
        db.WorkTasks.Add(task);
        await db.SaveChangesAsync(ct);
        return task;
    }

    public async Task<WorkTask> UpdateAsync(Guid id, UpdateTaskRequest request, Guid performedByUserId, CancellationToken ct = default)
    {
        var task = await GetByIdAsync(id, ct);
        var logs = new List<(string Field, string? Old, string? New)>();
        if (task.Title != request.Title) logs.Add(("Title", task.Title, request.Title));
        if (task.Description != request.Description) logs.Add(("Description", task.Description, request.Description));
        if (task.AssigneeId != request.AssigneeId) logs.Add(("AssigneeId", task.AssigneeId?.ToString(), request.AssigneeId?.ToString()));
        if (task.EstimatedEffort != request.EstimatedEffort) logs.Add(("EstimatedEffort", task.EstimatedEffort?.ToString(), request.EstimatedEffort?.ToString()));
        if (task.ActualEffort != request.ActualEffort) logs.Add(("ActualEffort", task.ActualEffort?.ToString(), request.ActualEffort?.ToString()));
        if (task.TargetDate != request.TargetDate) logs.Add(("TargetDate", task.TargetDate?.ToString("o"), request.TargetDate?.ToString("o")));
        if (task.Status != request.Status) logs.Add(("Status", task.Status.ToString(), request.Status.ToString()));

        task.Title = request.Title;
        task.Description = request.Description;
        task.AssigneeId = request.AssigneeId;
        task.EstimatedEffort = request.EstimatedEffort;
        task.ActualEffort = request.ActualEffort;
        task.TargetDate = request.TargetDate;
        task.Status = request.Status;
        await db.SaveChangesAsync(ct);
        await writeBack.RecalculateFromTaskAsync(id, ct);

        foreach (var (field, old, next) in logs)
            await audit.LogAsync(NodeType.Task, id, performedByUserId, field, old, next, ct);

        return task;
    }

    public async Task CompleteAsync(Guid id, CancellationToken ct = default)
    {
        var task = await GetByIdAsync(id, ct);
        task.Status = NodeStatus.Done;
        task.CompletionDate = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        await writeBack.RecalculateFromTaskAsync(id, ct);
    }

    public async Task ArchiveAsync(Guid id, CancellationToken ct = default)
    {
        var task = await GetByIdAsync(id, ct);
        task.IsArchived = true;
        await db.SaveChangesAsync(ct);
    }
}
