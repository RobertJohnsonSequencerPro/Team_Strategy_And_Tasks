using Microsoft.EntityFrameworkCore;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Exceptions;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Data;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class TaskService(AppDbContext db) : ITaskService
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

    public async Task<WorkTask> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken ct = default)
    {
        var task = await GetByIdAsync(id, ct);
        task.Title = request.Title;
        task.Description = request.Description;
        task.AssigneeId = request.AssigneeId;
        task.EstimatedEffort = request.EstimatedEffort;
        task.ActualEffort = request.ActualEffort;
        task.TargetDate = request.TargetDate;
        task.Status = request.Status;
        await db.SaveChangesAsync(ct);
        return task;
    }

    public async Task CompleteAsync(Guid id, CancellationToken ct = default)
    {
        var task = await GetByIdAsync(id, ct);
        task.Status = NodeStatus.Done;
        task.CompletionDate = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task ArchiveAsync(Guid id, CancellationToken ct = default)
    {
        var task = await GetByIdAsync(id, ct);
        task.IsArchived = true;
        await db.SaveChangesAsync(ct);
    }
}
