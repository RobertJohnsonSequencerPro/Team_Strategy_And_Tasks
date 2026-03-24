using System.Security.Claims;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Interfaces;

namespace TeamStrategyAndTasks.Web.Api;

public static class TaskEndpoints
{
    public static IEndpointRouteBuilder MapApiTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/tasks")
            .WithTags("Tasks")
            .RequireAuthorization("ApiBearer");

        group.MapGet("/", async (ITaskService svc) =>
        {
            var items = await svc.GetAllAsync();
            return Results.Ok(items.Select(ToResponse));
        })
        .WithSummary("List all tasks")
        .Produces<IEnumerable<TaskItemResponse>>();

        group.MapGet("/{id:guid}", async (Guid id, ITaskService svc) =>
        {
            var item = await svc.GetByIdAsync(id);
            return Results.Ok(ToResponse(item));
        })
        .WithSummary("Get a single task by ID")
        .Produces<TaskItemResponse>()
        .Produces(404);

        group.MapPost("/", async (
            CreateTaskRequest req,
            ClaimsPrincipal user,
            ITaskService svc) =>
        {
            var userId = GetUserId(user);
            var created = await svc.CreateAsync(req, userId);
            return Results.Created($"/api/tasks/{created.Id}", ToResponse(created));
        })
        .WithSummary("Create a new task")
        .Produces<TaskItemResponse>(201)
        .Produces(400);

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateTaskRequest req,
            ClaimsPrincipal user,
            ITaskService svc) =>
        {
            var userId = GetUserId(user);
            var updated = await svc.UpdateAsync(id, req, userId);
            return Results.Ok(ToResponse(updated));
        })
        .WithSummary("Update a task")
        .Produces<TaskItemResponse>()
        .Produces(404);

        group.MapPost("/{id:guid}/complete", async (
            Guid id,
            ITaskService svc) =>
        {
            await svc.CompleteAsync(id);
            return Results.NoContent();
        })
        .WithSummary("Mark a task as complete")
        .Produces(204)
        .Produces(404);

        group.MapDelete("/{id:guid}", async (Guid id, ITaskService svc) =>
        {
            await svc.ArchiveAsync(id);
            return Results.NoContent();
        })
        .WithSummary("Archive (soft-delete) a task")
        .Produces(204)
        .Produces(404);

        return app;
    }

    internal static TaskItemResponse ToResponse(WorkTask t) => new(
        t.Id,
        t.Title,
        t.Description,
        t.Status.ToString(),
        t.TargetDate,
        t.CompletionDate,
        t.EstimatedEffort,
        t.ActualEffort,
        t.CreatedAt,
        t.UpdatedAt,
        t.OwnerId,
        t.AssigneeId,
        t.TeamId,
        t.IsArchived,
        t.InitiativeWorkTasks.Select(iwt => iwt.InitiativeId).ToList());

    private static Guid GetUserId(ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID claim missing"));
}
