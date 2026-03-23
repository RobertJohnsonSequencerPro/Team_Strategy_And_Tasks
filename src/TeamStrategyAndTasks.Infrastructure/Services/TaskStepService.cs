using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Exceptions;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Data;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class TaskStepService(
    AppDbContext db,
    ICalendarInviteService calendarInviteService,
    ILogger<TaskStepService> logger) : ITaskStepService
{
    public async Task<IReadOnlyList<TaskStep>> GetByTaskAsync(Guid taskId, CancellationToken ct = default) =>
        await db.TaskSteps
            .Where(s => s.WorkTaskId == taskId)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.CreatedAt)
            .ToListAsync(ct);

    public async Task<TaskStep> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var step = await db.TaskSteps
            .Include(s => s.WorkTask)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
        return step ?? throw new NotFoundException(nameof(TaskStep), id);
    }

    public async Task<TaskStep> CreateAsync(Guid taskId, CreateTaskStepRequest request, CancellationToken ct = default)
    {
        var task = await db.WorkTasks.FirstOrDefaultAsync(t => t.Id == taskId, ct)
            ?? throw new NotFoundException(nameof(WorkTask), taskId);

        var step = new TaskStep
        {
            WorkTaskId = taskId,
            Title = request.Title,
            Description = request.Description,
            AssigneeId = request.AssigneeId,
            DueDate = request.DueDate,
            DisplayOrder = request.DisplayOrder
        };
        db.TaskSteps.Add(step);
        await db.SaveChangesAsync(ct);

        SendCalendarInviteIfAssigned(step, task.Title);

        return step;
    }

    public async Task<TaskStep> UpdateAsync(Guid id, UpdateTaskStepRequest request, CancellationToken ct = default)
    {
        var step = await GetByIdAsync(id, ct);
        var taskTitle = step.WorkTask.Title;

        var prevAssigneeId = step.AssigneeId;
        var prevDueDate = step.DueDate;

        step.Title = request.Title;
        step.Description = request.Description;
        step.AssigneeId = request.AssigneeId;
        step.DueDate = request.DueDate;
        step.DisplayOrder = request.DisplayOrder;
        step.Status = request.Status;

        await db.SaveChangesAsync(ct);

        // Re-send calendar invite when assignee or due date changes
        if (step.AssigneeId.HasValue && step.DueDate.HasValue
            && (step.AssigneeId != prevAssigneeId || step.DueDate != prevDueDate))
        {
            SendCalendarInviteIfAssigned(step, taskTitle);
        }

        return step;
    }

    public async Task CompleteAsync(Guid id, CancellationToken ct = default)
    {
        var step = await db.TaskSteps.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException(nameof(TaskStep), id);
        step.Status = NodeStatus.Done;
        step.CompletionDate = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var step = await db.TaskSteps.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException(nameof(TaskStep), id);
        db.TaskSteps.Remove(step);
        await db.SaveChangesAsync(ct);
    }

    public async Task ReorderAsync(Guid taskId, IReadOnlyList<Guid> orderedIds, CancellationToken ct = default)
    {
        var steps = await db.TaskSteps
            .Where(s => s.WorkTaskId == taskId)
            .ToListAsync(ct);

        for (var i = 0; i < orderedIds.Count; i++)
        {
            var step = steps.FirstOrDefault(s => s.Id == orderedIds[i]);
            if (step is not null)
                step.DisplayOrder = i;
        }

        await db.SaveChangesAsync(ct);
    }

    private void SendCalendarInviteIfAssigned(TaskStep step, string taskTitle)
    {
        if (!step.AssigneeId.HasValue || !step.DueDate.HasValue)
            return;

        var ics = calendarInviteService.GenerateIcs(step, taskTitle, null, null);

        // TODO(Phase 3): Resolve assignee email via UserManager and send via IEmailSender
        logger.LogInformation(
            "CalendarInvite: step {StepId} '{StepTitle}' due {DueDate} assigned to {AssigneeId} — ICS ready ({Bytes} bytes). " +
            "Deliver via /api/steps/{StepId}/calendar.ics",
            step.Id, step.Title, step.DueDate, step.AssigneeId, ics.Length, step.Id);
    }
}
