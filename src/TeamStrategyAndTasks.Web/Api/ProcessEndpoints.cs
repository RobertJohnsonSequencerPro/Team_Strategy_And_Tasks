using System.Security.Claims;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Interfaces;

namespace TeamStrategyAndTasks.Web.Api;

public static class ProcessEndpoints
{
    public static IEndpointRouteBuilder MapApiProcessEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/processes")
            .WithTags("Processes")
            .RequireAuthorization("ApiBearer");

        group.MapGet("/", async (IProcessService svc) =>
        {
            var items = await svc.GetAllAsync();
            return Results.Ok(items.Select(ToResponse));
        })
        .WithSummary("List all business processes")
        .Produces<IEnumerable<ProcessResponse>>();

        group.MapGet("/{id:guid}", async (Guid id, IProcessService svc) =>
        {
            var item = await svc.GetByIdAsync(id);
            return Results.Ok(ToResponse(item));
        })
        .WithSummary("Get a single process by ID")
        .Produces<ProcessResponse>()
        .Produces(404);

        group.MapPost("/", async (
            CreateProcessRequest req,
            ClaimsPrincipal user,
            IProcessService svc) =>
        {
            var userId = GetUserId(user);
            var created = await svc.CreateAsync(req, userId);
            return Results.Created($"/api/processes/{created.Id}", ToResponse(created));
        })
        .WithSummary("Create a new business process")
        .Produces<ProcessResponse>(201)
        .Produces(400);

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateProcessRequest req,
            ClaimsPrincipal user,
            IProcessService svc) =>
        {
            var userId = GetUserId(user);
            var updated = await svc.UpdateAsync(id, req, userId);
            return Results.Ok(ToResponse(updated));
        })
        .WithSummary("Update a business process")
        .Produces<ProcessResponse>()
        .Produces(404);

        group.MapDelete("/{id:guid}", async (Guid id, IProcessService svc) =>
        {
            await svc.ArchiveAsync(id);
            return Results.NoContent();
        })
        .WithSummary("Archive (soft-delete) a business process")
        .Produces(204)
        .Produces(404);

        return app;
    }

    internal static ProcessResponse ToResponse(BusinessProcess p) => new(
        p.Id,
        p.Title,
        p.Description,
        p.Status.ToString(),
        p.SuccessMetric,
        p.TargetValue,
        p.TargetDate,
        p.CreatedAt,
        p.UpdatedAt,
        p.OwnerId,
        p.TeamId,
        p.IsArchived,
        p.ObjectiveProcesses.Select(op => op.ObjectiveId).ToList(),
        p.ProcessInitiatives.Select(pi => pi.InitiativeId).ToList());

    private static Guid GetUserId(ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID claim missing"));
}
