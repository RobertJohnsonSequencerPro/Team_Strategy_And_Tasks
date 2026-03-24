using System.Security.Claims;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Interfaces;

namespace TeamStrategyAndTasks.Web.Api;

public static class ObjectiveEndpoints
{
    public static IEndpointRouteBuilder MapApiObjectiveEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/objectives")
            .WithTags("Objectives")
            .RequireAuthorization("ApiBearer");

        group.MapGet("/", async (IObjectiveService svc) =>
        {
            var items = await svc.GetAllAsync();
            return Results.Ok(items.Select(ToResponse));
        })
        .WithSummary("List all objectives")
        .Produces<IEnumerable<ObjectiveResponse>>();

        group.MapGet("/{id:guid}", async (Guid id, IObjectiveService svc) =>
        {
            var item = await svc.GetByIdAsync(id);
            return Results.Ok(ToResponse(item));
        })
        .WithSummary("Get a single objective by ID")
        .Produces<ObjectiveResponse>()
        .Produces(404);

        group.MapPost("/", async (
            CreateObjectiveRequest req,
            ClaimsPrincipal user,
            IObjectiveService svc) =>
        {
            var userId = GetUserId(user);
            var created = await svc.CreateAsync(req, userId);
            return Results.Created($"/api/objectives/{created.Id}", ToResponse(created));
        })
        .WithSummary("Create a new objective")
        .Produces<ObjectiveResponse>(201)
        .Produces(400);

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateObjectiveRequest req,
            ClaimsPrincipal user,
            IObjectiveService svc) =>
        {
            var userId = GetUserId(user);
            var updated = await svc.UpdateAsync(id, req, userId);
            return Results.Ok(ToResponse(updated));
        })
        .WithSummary("Update an objective")
        .Produces<ObjectiveResponse>()
        .Produces(404);

        group.MapDelete("/{id:guid}", async (
            Guid id,
            IObjectiveService svc) =>
        {
            await svc.ArchiveAsync(id);
            return Results.NoContent();
        })
        .WithSummary("Archive (soft-delete) an objective")
        .Produces(204)
        .Produces(404);

        return app;
    }

    internal static ObjectiveResponse ToResponse(Objective o) => new(
        o.Id,
        o.Title,
        o.Description,
        o.Status.ToString(),
        o.SuccessMetric,
        o.TargetValue,
        o.TargetDate,
        o.CreatedAt,
        o.UpdatedAt,
        o.OwnerId,
        o.TeamId,
        o.IsArchived,
        o.ObjectiveProcesses.Select(op => op.ProcessId).ToList());

    private static Guid GetUserId(ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID claim missing"));
}
