using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Exceptions;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Data;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class WebhookService(
    IDbContextFactory<AppDbContext> dbFactory,
    IHttpClientFactory httpFactory,
    ILogger<WebhookService> logger) : IWebhookService
{
    // ── Subscription management ───────────────────────────────────────────────

    public async Task<IReadOnlyList<WebhookSubscription>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.WebhookSubscriptions.OrderBy(w => w.Description).ToListAsync(ct);
    }

    public async Task<WebhookSubscription> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var sub = await db.WebhookSubscriptions.FindAsync([id], ct);
        return sub ?? throw new NotFoundException(nameof(WebhookSubscription), id);
    }

    public async Task<WebhookSubscription> CreateAsync(
        CreateWebhookSubscriptionRequest request, Guid userId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var sub = new WebhookSubscription
        {
            Description = request.Description,
            Url = request.Url,
            EventFilter = string.IsNullOrWhiteSpace(request.EventFilter) ? "*" : request.EventFilter,
            Secret = string.IsNullOrWhiteSpace(request.Secret) ? null : request.Secret,
            CreatedByUserId = userId,
            IsActive = true
        };
        db.WebhookSubscriptions.Add(sub);
        await db.SaveChangesAsync(ct);
        return sub;
    }

    public async Task<WebhookSubscription> UpdateAsync(
        Guid id, UpdateWebhookSubscriptionRequest request, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var sub = await db.WebhookSubscriptions.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(WebhookSubscription), id);
        sub.Description = request.Description;
        sub.Url = request.Url;
        sub.EventFilter = string.IsNullOrWhiteSpace(request.EventFilter) ? "*" : request.EventFilter;
        sub.Secret = string.IsNullOrWhiteSpace(request.Secret) ? null : request.Secret;
        sub.IsActive = request.IsActive;
        await db.SaveChangesAsync(ct);
        return sub;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var sub = await db.WebhookSubscriptions.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(WebhookSubscription), id);
        db.WebhookSubscriptions.Remove(sub);
        await db.SaveChangesAsync(ct);
    }

    // ── Event dispatch ────────────────────────────────────────────────────────

    public async Task FireAsync(
        WebhookEventType eventType,
        NodeType nodeType,
        Guid nodeId,
        string nodeTitle,
        string? oldStatus,
        string? newStatus,
        Guid? changedByUserId,
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var subscribers = await db.WebhookSubscriptions
            .Where(w => w.IsActive)
            .ToListAsync(ct);

        if (subscribers.Count == 0) return;

        var payload = new WebhookPayload(
            eventType.ToString(),
            nodeType.ToString(),
            nodeId,
            nodeTitle,
            oldStatus,
            newStatus,
            changedByUserId,
            DateTimeOffset.UtcNow);

        foreach (var sub in subscribers)
        {
            if (!Matches(sub.EventFilter, eventType)) continue;
            _ = Task.Run(() => DeliverAsync(sub, payload), CancellationToken.None);
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static bool Matches(string filter, WebhookEventType eventType)
    {
        if (string.IsNullOrWhiteSpace(filter) || filter.Trim() == "*") return true;
        return filter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(f => f.Equals(eventType.ToString(), StringComparison.OrdinalIgnoreCase));
    }

    private async Task DeliverAsync(WebhookSubscription sub, WebhookPayload payload)
    {
        try
        {
            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var http = httpFactory.CreateClient("WebhookClient");

            var request = new HttpRequestMessage(HttpMethod.Post, sub.Url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            if (!string.IsNullOrWhiteSpace(sub.Secret))
                request.Headers.Add("X-Webhook-Signature", ComputeSignature(sub.Secret, json));

            request.Headers.Add("X-Webhook-Event", payload.EventType);
            request.Headers.Add("X-Webhook-Subscription", sub.Id.ToString());

            var response = await http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                logger.LogWarning("Webhook {SubId} to {Url} returned {Status}", sub.Id, sub.Url, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Webhook delivery failed for subscription {SubId} → {Url}", sub.Id, sub.Url);
        }
    }

    private static string ComputeSignature(string secret, string json)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var dataBytes = Encoding.UTF8.GetBytes(json);
        var hash = HMACSHA256.HashData(keyBytes, dataBytes);
        return "sha256=" + Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
}

/// <summary>JSON payload sent to each subscriber.</summary>
internal record WebhookPayload(
    string EventType,
    string NodeType,
    Guid NodeId,
    string NodeTitle,
    string? OldStatus,
    string? NewStatus,
    Guid? ChangedByUserId,
    DateTimeOffset OccurredAt);
