using System.Security.Claims;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Interfaces;

namespace TeamStrategyAndTasks.Web.Api;

public static class InitiativeEndpoints
{
    public static IEndpointRouteBuilder MapApiInitiativeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/initiatives")
            .WithTags("Initiatives")
            .RequireAuthorization("ApiBearer");

        group.MapGet("/", async (IInitiativeService svc) =>
        {
            var items = await svc.GetAllAsync();
            return Results.Ok(items.Select(ToResponse));
        })
        .WithSummary("List all initiatives")
        .Produces<IEnumerable<InitiativeResponse>>();

        group.MapGet("/{id:guid}", async (Guid id, IInitiativeService svc) =>
        {
            var item = await svc.GetByIdAsync(id);
            return Results.Ok(ToResponse(item));
        })
        .WithSummary("Get a single initiative by ID")
        .Produces<InitiativeResponse>()
        .Produces(404);

        group.MapPost("/", async (
            CreateInitiativeRequest req,
            ClaimsPrincipal user,
            IInitiativeService svc) =>
        {
            var userId = GetUserId(user);
            var created = await svc.CreateAsync(req, userId);
            return Results.Created($"/api/initiatives/{created.Id}", ToResponse(created));
        })
        .WithSummary("Create a new initiative")
        .Produces<InitiativeResponse>(201)
        .Produces(400);

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateInitiativeRequest req,
            ClaimsPrincipal user,
            IInitiativeService svc) =>
        {
            var userId = GetUserId(user);
            var updated = await svc.UpdateAsync(id, req, userId);
            return Results.Ok(ToResponse(updated));
        })
        .WithSummary("Update an initiative")
        .Produces<InitiativeResponse>()
        .Produces(404);

        group.MapDelete("/{id:guid}", async (Guid id, IInitiativeService svc) =>
        {
            await svc.ArchiveAsync(id);
            return Results.NoContent();
        })
        .WithSummary("Archive (soft-delete) an initiative")
        .Produces(204)
        .Produces(404);

        return app;
    }

    internal static InitiativeResponse ToResponse(Initiative i) => new(
        i.Id,
        i.Title,
        i.Description,
        i.Status.ToString(),
        i.TargetDate,
        i.CreatedAt,
        i.UpdatedAt,
        i.OwnerId,
        i.TeamId,
        i.IsArchived,
        i.ProcessInitiatives.Select(pi => pi.ProcessId).ToList(),
        i.InitiativeWorkTasks.Select(iwt => iwt.WorkTaskId).ToList());

    private static Guid GetUserId(ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID claim missing"));
}
