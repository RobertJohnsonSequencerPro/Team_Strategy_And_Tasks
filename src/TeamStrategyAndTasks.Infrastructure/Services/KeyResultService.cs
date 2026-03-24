using Microsoft.AspNetCore.Identity;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Identity;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class KeyResultService(
    IDbContextFactory<AppDbContext> dbFactory,
    IAuditService audit,
    UserManager<ApplicationUser> users) : IKeyResultService
{
    // ── Queries ──────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<KeyResultDto>> GetForObjectiveAsync(
        Guid objectiveId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var krs = await db.KeyResults
            .Where(k => k.ObjectiveId == objectiveId)
            .OrderBy(k => k.Title)
            .ToListAsync(ct);

        var results = new List<KeyResultDto>(krs.Count);
        foreach (var kr in krs)
            results.Add(await ToDtoAsync(kr, ct));

        return results;
    }

    // ── Mutations ────────────────────────────────────────────────────────────

    public async Task<KeyResultDto> AddAsync(
        AddKeyResultRequest request, Guid performedByUserId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        if (request.TargetValue <= 0)
            throw new AppValidationException("TargetValue", "Target value must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new AppValidationException("Title", "Title is required.");

        if (!await db.Objectives.AnyAsync(o => o.Id == request.ObjectiveId, ct))
            throw new NotFoundException($"Objective {request.ObjectiveId} not found.");

        var kr = new KeyResult
        {
            ObjectiveId   = request.ObjectiveId,
            Title         = request.Title.Trim(),
            CurrentValue  = request.CurrentValue,
            TargetValue   = request.TargetValue,
            Unit          = request.Unit.Trim(),
            OwnerId       = request.OwnerId
        };

        db.KeyResults.Add(kr);
        await db.SaveChangesAsync(ct);

        await audit.LogAsync(NodeType.Objective, request.ObjectiveId, performedByUserId,
            "KeyResult.Added", null, kr.Title, ct);

        return await ToDtoAsync(kr, ct);
    }

    public async Task<KeyResultDto> UpdateAsync(
        Guid id, UpdateKeyResultRequest request, Guid performedByUserId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        if (request.TargetValue <= 0)
            throw new AppValidationException("TargetValue", "Target value must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new AppValidationException("Title", "Title is required.");

        var kr = await db.KeyResults.FindAsync([id], ct)
            ?? throw new NotFoundException($"Key Result {id} not found.");

        var oldProgress = ProgressPercent(kr);

        kr.Title        = request.Title.Trim();
        kr.CurrentValue = request.CurrentValue;
        kr.TargetValue  = request.TargetValue;
        kr.Unit         = request.Unit.Trim();
        kr.OwnerId      = request.OwnerId;

        await db.SaveChangesAsync(ct);

        await audit.LogAsync(NodeType.Objective, kr.ObjectiveId, performedByUserId,
            "KeyResult.Progress",
            $"{oldProgress:F1}%",
            $"{ProgressPercent(kr):F1}% ({kr.CurrentValue} / {kr.TargetValue} {kr.Unit})", ct);

        return await ToDtoAsync(kr, ct);
    }

    public async Task DeleteAsync(Guid id, Guid performedByUserId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var kr = await db.KeyResults.FindAsync([id], ct)
            ?? throw new NotFoundException($"Key Result {id} not found.");

        var objectiveId = kr.ObjectiveId;
        var title       = kr.Title;

        db.KeyResults.Remove(kr);
        await db.SaveChangesAsync(ct);

        await audit.LogAsync(NodeType.Objective, objectiveId, performedByUserId,
            "KeyResult.Removed", title, null, ct);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<KeyResultDto> ToDtoAsync(KeyResult kr, CancellationToken ct)
    {
        var owner = await users.FindByIdAsync(kr.OwnerId.ToString());
        var ownerName = owner?.DisplayName ?? owner?.UserName ?? kr.OwnerId.ToString();
        var pct = ProgressPercent(kr);
        return new KeyResultDto(
            kr.Id, kr.ObjectiveId, kr.Title,
            kr.CurrentValue, kr.TargetValue, kr.Unit,
            kr.OwnerId, ownerName,
            pct, ComputeStatus(pct),
            kr.UpdatedAt);
    }

    private static double ProgressPercent(KeyResult kr) =>
        kr.TargetValue <= 0 ? 0d : Math.Min(100d, (double)(kr.CurrentValue / kr.TargetValue) * 100d);

    private static KeyResultStatus ComputeStatus(double pct) => pct switch
    {
        >= 100 => KeyResultStatus.Complete,
        >= 70  => KeyResultStatus.OnTrack,
        _      => KeyResultStatus.AtRisk
    };
}
