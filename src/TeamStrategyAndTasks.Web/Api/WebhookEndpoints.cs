using System.Security.Claims;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Interfaces;

namespace TeamStrategyAndTasks.Web.Api;

public static class WebhookEndpoints
{
    public static IEndpointRouteBuilder MapApiWebhookEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/webhooks")
            .WithTags("Webhooks")
            .RequireAuthorization("ApiBearer");

        group.MapGet("/", async (IWebhookService svc) =>
        {
            var items = await svc.GetAllAsync();
            return Results.Ok(items.Select(ToResponse));
        })
        .WithSummary("List all webhook subscriptions")
        .Produces<IEnumerable<WebhookSubscriptionResponse>>();

        group.MapGet("/{id:guid}", async (Guid id, IWebhookService svc) =>
        {
            var item = await svc.GetByIdAsync(id);
            return Results.Ok(ToResponse(item));
        })
        .WithSummary("Get a single webhook subscription")
        .Produces<WebhookSubscriptionResponse>()
        .Produces(404);

        group.MapPost("/", async (
            CreateWebhookSubscriptionRequest req,
            ClaimsPrincipal user,
            IWebhookService svc) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var created = await svc.CreateAsync(req, userId);
            return Results.Created($"/api/webhooks/{created.Id}", ToResponse(created));
        })
        .WithSummary("Create a new webhook subscription")
        .Produces<WebhookSubscriptionResponse>(201)
        .Produces(400);

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateWebhookSubscriptionRequest req,
            IWebhookService svc) =>
        {
            var updated = await svc.UpdateAsync(id, req);
            return Results.Ok(ToResponse(updated));
        })
        .WithSummary("Update a webhook subscription")
        .Produces<WebhookSubscriptionResponse>()
        .Produces(404);

        group.MapDelete("/{id:guid}", async (Guid id, IWebhookService svc) =>
        {
            await svc.DeleteAsync(id);
            return Results.NoContent();
        })
        .WithSummary("Delete a webhook subscription")
        .Produces(204)
        .Produces(404);

        return app;
    }

    private static WebhookSubscriptionResponse ToResponse(WebhookSubscription w) => new(
        w.Id,
        w.Description,
        w.Url,
        w.EventFilter,
        w.Secret is not null,   // never expose the secret itself via API
        w.IsActive,
        w.CreatedAt,
        w.CreatedByUserId);
}
