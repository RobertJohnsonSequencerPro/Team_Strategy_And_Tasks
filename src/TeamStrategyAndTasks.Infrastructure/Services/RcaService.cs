using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Exceptions;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Data;
using TeamStrategyAndTasks.Infrastructure.Identity;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class RcaService(
    IDbContextFactory<AppDbContext> dbFactory,
    UserManager<ApplicationUser> users) : IRcaService
{
    private static readonly RcaCaseStatus[] StatusWorkflow =
    [
        RcaCaseStatus.Drafting,
        RcaCaseStatus.InReview,
        RcaCaseStatus.Approved,
        RcaCaseStatus.Archived
    ];

    // ── List ─────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<RcaCaseSummaryDto>> GetAllAsync(
        bool showArchived = false,
        string? search = null,
        RcaType? typeFilter = null,
        string? processArea = null,
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var query = db.RcaCases.AsNoTracking();

        if (!showArchived) query = query.Where(r => !r.IsArchived);
        if (typeFilter.HasValue) query = query.Where(r => r.RcaType == typeFilter.Value);
        if (!string.IsNullOrWhiteSpace(processArea))
            query = query.Where(r => r.ProcessArea == processArea);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(r =>
                r.Title.ToLower().Contains(term) ||
                (r.RootCauseSummary != null && r.RootCauseSummary.ToLower().Contains(term)) ||
                (r.ProcessArea != null && r.ProcessArea.ToLower().Contains(term)) ||
                r.Tags.Any(t => t.Tag.ToLower().Contains(term)));
        }

        var cases = await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id, r.Title, r.RcaType, r.Status, r.ProcessArea, r.PartFamily,
                r.RootCauseSummary, r.LinkedCapaCaseId, r.LinkedFindingId,
                r.InitiatedById, r.ApprovedById, r.ApprovedAt, r.IsArchived,
                r.CreatedAt, r.UpdatedAt,
                NodeCount = r.FiveWhyNodes.Count + r.IshikawaCauses.Count,
                Tags = r.Tags.Select(t => t.Tag).ToList()
            })
            .ToListAsync(ct);

        if (cases.Count == 0) return [];

        var allUserIds = cases
            .SelectMany(c => new[] { c.InitiatedById, c.ApprovedById }
                .Where(id => id.HasValue)
                .Select(id => id!.Value))
            .Distinct()
            .ToList();

        var userNames = allUserIds.Count > 0
            ? await db.Users.AsNoTracking()
                .Where(u => allUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? u.Id.ToString(), ct)
            : new Dictionary<Guid, string>();

        return cases.Select(c => new RcaCaseSummaryDto(
            c.Id, c.Title, c.RcaType, c.Status, c.ProcessArea, c.PartFamily,
            c.RootCauseSummary, c.LinkedCapaCaseId, c.LinkedFindingId,
            c.InitiatedById,
            c.InitiatedById.HasValue ? userNames.GetValueOrDefault(c.InitiatedById.Value) : null,
            c.ApprovedById,
            c.ApprovedById.HasValue ? userNames.GetValueOrDefault(c.ApprovedById.Value) : null,
            c.ApprovedAt, c.IsArchived, c.NodeCount,
            c.Tags, c.CreatedAt, c.UpdatedAt)).ToList();
    }

    // ── Detail ───────────────────────────────────────────────────────────────

    public async Task<RcaCaseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var rca = await db.RcaCases
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new
            {
                r.Id, r.Title, r.ProblemStatement, r.RcaType, r.Status,
                r.ProcessArea, r.PartFamily, r.RootCauseSummary,
                r.LinkedCapaCaseId, r.LinkedFindingId,
                r.InitiatedById, r.ApprovedById, r.ApprovedAt, r.IsArchived,
                r.CreatedAt, r.UpdatedAt,
                Tags = r.Tags.Select(t => t.Tag).ToList(),
                Nodes = r.FiveWhyNodes
                    .OrderBy(n => n.DisplayOrder)
                    .Select(n => new
                    {
                        n.Id, n.RcaCaseId, n.ParentId, n.DisplayOrder,
                        n.WhyQuestion, n.BecauseAnswer, n.IsRootCause
                    }).ToList(),
                Causes = r.IshikawaCauses
                    .OrderBy(c => c.Category)
                    .ThenBy(c => c.DisplayOrder)
                    .Select(c => new
                    {
                        c.Id, c.RcaCaseId, c.Category, c.ParentCauseId,
                        c.DisplayOrder, c.CauseText, c.IsRootCause
                    }).ToList()
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(RcaCase), id);

        var allUserIds = new HashSet<Guid>();
        if (rca.InitiatedById.HasValue) allUserIds.Add(rca.InitiatedById.Value);
        if (rca.ApprovedById.HasValue)  allUserIds.Add(rca.ApprovedById.Value);

        var userNames = allUserIds.Count > 0
            ? await db.Users.AsNoTracking()
                .Where(u => allUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? u.Id.ToString(), ct)
            : new Dictionary<Guid, string>();

        var nodes = rca.Nodes.Select(n => new FiveWhyNodeDto(
            n.Id, n.RcaCaseId, n.ParentId, n.DisplayOrder,
            n.WhyQuestion, n.BecauseAnswer, n.IsRootCause)).ToList();

        var causes = rca.Causes.Select(c => new IshikawaCauseDto(
            c.Id, c.RcaCaseId, c.Category, c.ParentCauseId,
            c.DisplayOrder, c.CauseText, c.IsRootCause)).ToList();

        return new RcaCaseDto(
            rca.Id, rca.Title, rca.ProblemStatement, rca.RcaType, rca.Status,
            rca.ProcessArea, rca.PartFamily, rca.RootCauseSummary,
            rca.LinkedCapaCaseId, rca.LinkedFindingId,
            rca.InitiatedById,
            rca.InitiatedById.HasValue ? userNames.GetValueOrDefault(rca.InitiatedById.Value) : null,
            rca.ApprovedById,
            rca.ApprovedById.HasValue ? userNames.GetValueOrDefault(rca.ApprovedById.Value) : null,
            rca.ApprovedAt, rca.IsArchived, rca.Tags, nodes, causes,
            rca.CreatedAt, rca.UpdatedAt);
    }

    // ── Create ───────────────────────────────────────────────────────────────

    public async Task<RcaCaseSummaryDto> CreateAsync(
        CreateRcaCaseRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new AppValidationException("Title", "Title is required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var rca = new RcaCase
        {
            Title            = request.Title.Trim(),
            ProblemStatement = request.ProblemStatement?.Trim(),
            RcaType          = request.RcaType,
            Status           = RcaCaseStatus.Drafting,
            ProcessArea      = request.ProcessArea?.Trim(),
            PartFamily       = request.PartFamily?.Trim(),
            LinkedCapaCaseId = request.LinkedCapaCaseId,
            LinkedFindingId  = request.LinkedFindingId,
            InitiatedById    = request.InitiatedById
        };

        db.RcaCases.Add(rca);
        await db.SaveChangesAsync(ct);

        string? initiatedByName = null;
        if (rca.InitiatedById.HasValue)
        {
            var u = await users.FindByIdAsync(rca.InitiatedById.Value.ToString());
            initiatedByName = u?.DisplayName ?? u?.UserName;
        }

        return new RcaCaseSummaryDto(
            rca.Id, rca.Title, rca.RcaType, rca.Status, rca.ProcessArea, rca.PartFamily,
            rca.RootCauseSummary, rca.LinkedCapaCaseId, rca.LinkedFindingId,
            rca.InitiatedById, initiatedByName, null, null, null,
            rca.IsArchived, 0, [], rca.CreatedAt, rca.UpdatedAt);
    }

    // ── Update ───────────────────────────────────────────────────────────────

    public async Task UpdateAsync(Guid id, UpdateRcaCaseRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new AppValidationException("Title", "Title is required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var rca = await db.RcaCases.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(RcaCase), id);

        rca.Title            = request.Title.Trim();
        rca.ProblemStatement = request.ProblemStatement?.Trim();
        rca.ProcessArea      = request.ProcessArea?.Trim();
        rca.PartFamily       = request.PartFamily?.Trim();
        rca.RootCauseSummary = request.RootCauseSummary?.Trim();
        rca.LinkedCapaCaseId = request.LinkedCapaCaseId;
        rca.LinkedFindingId  = request.LinkedFindingId;
        rca.UpdatedAt        = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
    }

    // ── Status transition ─────────────────────────────────────────────────────

    public async Task TransitionStatusAsync(
        Guid id, TransitionRcaStatusRequest request, Guid currentUserId,
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var rca = await db.RcaCases.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(RcaCase), id);

        // Re-open: Approved → Drafting (special reverse transition)
        if (rca.Status == RcaCaseStatus.Approved && request.NewStatus == RcaCaseStatus.Drafting)
        {
            rca.Status       = RcaCaseStatus.Drafting;
            rca.ApprovedById = null;
            rca.ApprovedAt   = null;
            rca.UpdatedAt    = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
            return;
        }

        var currentIdx = Array.IndexOf(StatusWorkflow, rca.Status);
        var targetIdx  = Array.IndexOf(StatusWorkflow, request.NewStatus);

        if (targetIdx < 0 || targetIdx != currentIdx + 1)
            throw new AppValidationException("Status",
                $"Cannot transition from {rca.Status} to {request.NewStatus}.");

        if (request.NewStatus == RcaCaseStatus.Approved
            && string.IsNullOrWhiteSpace(rca.RootCauseSummary))
            throw new AppValidationException("RootCauseSummary",
                "Root cause summary is required before approving.");

        rca.Status    = request.NewStatus;
        rca.UpdatedAt = DateTimeOffset.UtcNow;

        if (request.NewStatus == RcaCaseStatus.Approved)
        {
            rca.ApprovedById = currentUserId;
            rca.ApprovedAt   = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(ct);
    }

    // ── Archive toggle ────────────────────────────────────────────────────────

    public async Task<bool> ToggleArchiveAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var rca = await db.RcaCases.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(RcaCase), id);

        rca.IsArchived = !rca.IsArchived;
        rca.UpdatedAt  = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return rca.IsArchived;
    }

    // ── Tags ──────────────────────────────────────────────────────────────────

    public async Task SetTagsAsync(Guid id, SetRcaTagsRequest request, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        _ = await db.RcaCases.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(RcaCase), id);

        var existing = await db.RcaCaseTags.Where(t => t.RcaCaseId == id).ToListAsync(ct);
        db.RcaCaseTags.RemoveRange(existing);

        var newTags = request.Tags
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(t => new RcaCaseTag { RcaCaseId = id, Tag = t });

        db.RcaCaseTags.AddRange(newTags);
        await db.SaveChangesAsync(ct);
    }

    // ── Cross-module ──────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<RcaCaseSummaryDto>> GetByLinkedCapaCaseAsync(
        Guid capaCaseId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var cases = await db.RcaCases
            .AsNoTracking()
            .Where(r => r.LinkedCapaCaseId == capaCaseId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id, r.Title, r.RcaType, r.Status, r.ProcessArea, r.PartFamily,
                r.RootCauseSummary, r.LinkedCapaCaseId, r.LinkedFindingId,
                r.InitiatedById, r.ApprovedById, r.ApprovedAt, r.IsArchived,
                r.CreatedAt, r.UpdatedAt,
                NodeCount = r.FiveWhyNodes.Count + r.IshikawaCauses.Count,
                Tags = r.Tags.Select(t => t.Tag).ToList()
            })
            .ToListAsync(ct);

        if (cases.Count == 0) return [];

        var allUserIds = cases
            .SelectMany(c => new[] { c.InitiatedById, c.ApprovedById }
                .Where(id => id.HasValue).Select(id => id!.Value))
            .Distinct().ToList();

        var userNames = allUserIds.Count > 0
            ? await db.Users.AsNoTracking()
                .Where(u => allUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? u.Id.ToString(), ct)
            : new Dictionary<Guid, string>();

        return cases.Select(c => new RcaCaseSummaryDto(
            c.Id, c.Title, c.RcaType, c.Status, c.ProcessArea, c.PartFamily,
            c.RootCauseSummary, c.LinkedCapaCaseId, c.LinkedFindingId,
            c.InitiatedById,
            c.InitiatedById.HasValue ? userNames.GetValueOrDefault(c.InitiatedById.Value) : null,
            c.ApprovedById,
            c.ApprovedById.HasValue ? userNames.GetValueOrDefault(c.ApprovedById.Value) : null,
            c.ApprovedAt, c.IsArchived, c.NodeCount,
            c.Tags, c.CreatedAt, c.UpdatedAt)).ToList();
    }

    // ── 5 Whys ────────────────────────────────────────────────────────────────

    public async Task<FiveWhyNodeDto> AddFiveWhyNodeAsync(
        AddFiveWhyNodeRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.WhyQuestion))
            throw new AppValidationException("WhyQuestion", "Why question is required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        _ = await db.RcaCases.FindAsync([request.RcaCaseId], ct)
            ?? throw new NotFoundException(nameof(RcaCase), request.RcaCaseId);

        var node = new FiveWhyNode
        {
            RcaCaseId     = request.RcaCaseId,
            ParentId      = request.ParentId,
            DisplayOrder  = request.DisplayOrder,
            WhyQuestion   = request.WhyQuestion.Trim(),
            BecauseAnswer = request.BecauseAnswer?.Trim(),
            IsRootCause   = request.IsRootCause
        };

        db.FiveWhyNodes.Add(node);
        await db.SaveChangesAsync(ct);

        return new FiveWhyNodeDto(
            node.Id, node.RcaCaseId, node.ParentId, node.DisplayOrder,
            node.WhyQuestion, node.BecauseAnswer, node.IsRootCause);
    }

    public async Task UpdateFiveWhyNodeAsync(
        Guid nodeId, UpdateFiveWhyNodeRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.WhyQuestion))
            throw new AppValidationException("WhyQuestion", "Why question is required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var node = await db.FiveWhyNodes.FindAsync([nodeId], ct)
            ?? throw new NotFoundException(nameof(FiveWhyNode), nodeId);

        node.WhyQuestion   = request.WhyQuestion.Trim();
        node.BecauseAnswer = request.BecauseAnswer?.Trim();
        node.IsRootCause   = request.IsRootCause;

        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteFiveWhyNodeAsync(Guid nodeId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var node = await db.FiveWhyNodes.FindAsync([nodeId], ct)
            ?? throw new NotFoundException(nameof(FiveWhyNode), nodeId);

        // Re-parent any children to the deleted node's parent so the tree stays coherent.
        var children = await db.FiveWhyNodes
            .Where(n => n.ParentId == nodeId)
            .ToListAsync(ct);

        foreach (var child in children)
            child.ParentId = node.ParentId;

        db.FiveWhyNodes.Remove(node);
        await db.SaveChangesAsync(ct);
    }

    // ── Ishikawa ──────────────────────────────────────────────────────────────

    public async Task<IshikawaCauseDto> AddIshikawaCauseAsync(
        AddIshikawaCauseRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.CauseText))
            throw new AppValidationException("CauseText", "Cause text is required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        _ = await db.RcaCases.FindAsync([request.RcaCaseId], ct)
            ?? throw new NotFoundException(nameof(RcaCase), request.RcaCaseId);

        var cause = new IshikawaCause
        {
            RcaCaseId     = request.RcaCaseId,
            Category      = request.Category,
            ParentCauseId = request.ParentCauseId,
            DisplayOrder  = request.DisplayOrder,
            CauseText     = request.CauseText.Trim(),
            IsRootCause   = request.IsRootCause
        };

        db.IshikawaCauses.Add(cause);
        await db.SaveChangesAsync(ct);

        return new IshikawaCauseDto(
            cause.Id, cause.RcaCaseId, cause.Category, cause.ParentCauseId,
            cause.DisplayOrder, cause.CauseText, cause.IsRootCause);
    }

    public async Task UpdateIshikawaCauseAsync(
        Guid causeId, UpdateIshikawaCauseRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.CauseText))
            throw new AppValidationException("CauseText", "Cause text is required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var cause = await db.IshikawaCauses.FindAsync([causeId], ct)
            ?? throw new NotFoundException(nameof(IshikawaCause), causeId);

        cause.Category    = request.Category;
        cause.CauseText   = request.CauseText.Trim();
        cause.IsRootCause = request.IsRootCause;

        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteIshikawaCauseAsync(Guid causeId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var cause = await db.IshikawaCauses.FindAsync([causeId], ct)
            ?? throw new NotFoundException(nameof(IshikawaCause), causeId);

        // Re-parent sub-causes to the deleted cause's parent.
        var subCauses = await db.IshikawaCauses
            .Where(c => c.ParentCauseId == causeId)
            .ToListAsync(ct);

        foreach (var sub in subCauses)
            sub.ParentCauseId = cause.ParentCauseId;

        db.IshikawaCauses.Remove(cause);
        await db.SaveChangesAsync(ct);
    }

    // ── Library / Recurring ───────────────────────────────────────────────────

    public async Task<IReadOnlyList<RecurringRootCauseDto>> GetRecurringRootCausesAsync(
        int minCount = 2, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        // Root cause text from 5-Whys nodes (use answer when present, else question)
        var fiveWhyRoots = await db.FiveWhyNodes
            .AsNoTracking()
            .Where(n => n.IsRootCause
                && n.RcaCase.Status == RcaCaseStatus.Approved
                && !n.RcaCase.IsArchived)
            .Select(n => new { CaseId = n.RcaCaseId, Text = n.BecauseAnswer ?? n.WhyQuestion })
            .ToListAsync(ct);

        var ishikawaRoots = await db.IshikawaCauses
            .AsNoTracking()
            .Where(c => c.IsRootCause
                && c.RcaCase.Status == RcaCaseStatus.Approved
                && !c.RcaCase.IsArchived)
            .Select(c => new { CaseId = c.RcaCaseId, Text = c.CauseText })
            .ToListAsync(ct);

        // Group by normalised text; count distinct cases per group
        var grouped = fiveWhyRoots
            .Concat(ishikawaRoots)
            .GroupBy(r => r.Text.Trim().ToLowerInvariant())
            .Where(g => g.Select(r => r.CaseId).Distinct().Count() >= minCount)
            .OrderByDescending(g => g.Select(r => r.CaseId).Distinct().Count())
            .ToList();

        if (grouped.Count == 0) return [];

        var allCaseIds = grouped
            .SelectMany(g => g.Select(r => r.CaseId))
            .Distinct()
            .ToList();

        var cases = await db.RcaCases
            .AsNoTracking()
            .Where(r => allCaseIds.Contains(r.Id))
            .Select(r => new
            {
                r.Id, r.Title, r.RcaType, r.Status, r.ProcessArea, r.PartFamily,
                r.RootCauseSummary, r.LinkedCapaCaseId, r.LinkedFindingId,
                r.InitiatedById, r.ApprovedById, r.ApprovedAt, r.IsArchived,
                r.CreatedAt, r.UpdatedAt,
                NodeCount = r.FiveWhyNodes.Count + r.IshikawaCauses.Count,
                Tags = r.Tags.Select(t => t.Tag).ToList()
            })
            .ToListAsync(ct);

        var allUserIds = cases
            .SelectMany(c => new[] { c.InitiatedById, c.ApprovedById }
                .Where(id => id.HasValue).Select(id => id!.Value))
            .Distinct().ToList();

        var userNames = allUserIds.Count > 0
            ? await db.Users.AsNoTracking()
                .Where(u => allUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? u.Id.ToString(), ct)
            : new Dictionary<Guid, string>();

        var caseById = cases.ToDictionary(c => c.Id, c => new RcaCaseSummaryDto(
            c.Id, c.Title, c.RcaType, c.Status, c.ProcessArea, c.PartFamily,
            c.RootCauseSummary, c.LinkedCapaCaseId, c.LinkedFindingId,
            c.InitiatedById,
            c.InitiatedById.HasValue ? userNames.GetValueOrDefault(c.InitiatedById.Value) : null,
            c.ApprovedById,
            c.ApprovedById.HasValue ? userNames.GetValueOrDefault(c.ApprovedById.Value) : null,
            c.ApprovedAt, c.IsArchived, c.NodeCount,
            c.Tags, c.CreatedAt, c.UpdatedAt));

        return grouped.Select(g =>
        {
            var uniqueCaseIds = g.Select(r => r.CaseId).Distinct().ToList();
            var groupCases = uniqueCaseIds
                .Where(id => caseById.ContainsKey(id))
                .Select(id => caseById[id])
                .ToList();
            return new RecurringRootCauseDto(
                g.First().Text.Trim(),
                groupCases.Count,
                groupCases);
        }).ToList();
    }

    public async Task<IReadOnlyList<string>> GetAllTagsAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.RcaCaseTags
            .AsNoTracking()
            .Select(t => t.Tag)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync(ct);
    }
}
