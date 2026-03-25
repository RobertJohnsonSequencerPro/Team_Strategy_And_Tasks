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

public class ControlPlanService(
    IDbContextFactory<AppDbContext> dbFactory,
    UserManager<ApplicationUser> users) : IControlPlanService
{
    // ── List ─────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<ControlPlanSummaryDto>> GetAllAsync(
        bool showArchived = false, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var plans = await db.ControlPlans
            .AsNoTracking()
            .Where(p => showArchived || !p.IsArchived)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                p.Id, p.Title, p.ProcessItem, p.PartNumber, p.Revision,
                p.Status, p.OwnerId, p.LinkedPfmeaId, p.IsArchived,
                p.CreatedAt, p.UpdatedAt,
                CharacteristicCount = p.Characteristics.Count
            })
            .ToListAsync(ct);

        if (plans.Count == 0) return [];

        var ownerIds = plans
            .Where(p => p.OwnerId.HasValue)
            .Select(p => p.OwnerId!.Value)
            .Distinct()
            .ToList();

        var ownerNames = ownerIds.Count > 0
            ? await db.Users.AsNoTracking()
                .Where(u => ownerIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? u.Id.ToString(), ct)
            : new Dictionary<Guid, string>();

        return plans.Select(p => new ControlPlanSummaryDto(
            p.Id, p.Title, p.ProcessItem, p.PartNumber, p.Revision,
            p.Status,
            p.OwnerId, p.OwnerId.HasValue ? ownerNames.GetValueOrDefault(p.OwnerId.Value) : null,
            p.LinkedPfmeaId, p.IsArchived, p.CharacteristicCount,
            p.CreatedAt, p.UpdatedAt)).ToList();
    }

    // ── Detail ───────────────────────────────────────────────────────────────

    public async Task<ControlPlanDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var plan = await db.ControlPlans
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id, p.Title, p.ProcessItem, p.PartNumber, p.PartDescription,
                p.Revision, p.Status, p.OwnerId, p.LinkedPfmeaId, p.IsArchived,
                p.CreatedAt, p.UpdatedAt,
                Characteristics = p.Characteristics
                    .OrderBy(c => c.SortOrder)
                    .Select(c => new
                    {
                        c.Id, c.ControlPlanId, c.SortOrder,
                        c.ProcessStep, c.ProcessOperation, c.CharacteristicNo,
                        c.CharacteristicType, c.CharacteristicDescription,
                        c.SpecificationTolerance, c.ControlMethod,
                        c.SamplingSize, c.SamplingFrequency, c.MeasurementTechnique,
                        c.ReactionPlan, c.ResponsiblePersonId, c.Notes
                    }).ToList(),
                Revisions = p.Revisions
                    .OrderByDescending(r => r.ChangedAt)
                    .Select(r => new
                    {
                        r.Id, r.ControlPlanId, r.RevisionLabel,
                        r.ToStatus, r.Comments, r.ChangedById, r.ChangedAt
                    }).ToList()
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(ControlPlan), id);

        var allUserIds = new HashSet<Guid>();
        if (plan.OwnerId.HasValue) allUserIds.Add(plan.OwnerId.Value);
        foreach (var c in plan.Characteristics)
            if (c.ResponsiblePersonId.HasValue) allUserIds.Add(c.ResponsiblePersonId.Value);
        foreach (var r in plan.Revisions)
            allUserIds.Add(r.ChangedById);

        var userNames = allUserIds.Count > 0
            ? await db.Users.AsNoTracking()
                .Where(u => allUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? u.Id.ToString(), ct)
            : new Dictionary<Guid, string>();

        var charDtos = plan.Characteristics.Select(c => new ControlPlanCharacteristicDto(
            c.Id, c.ControlPlanId, c.SortOrder,
            c.ProcessStep, c.ProcessOperation, c.CharacteristicNo,
            c.CharacteristicType, c.CharacteristicDescription,
            c.SpecificationTolerance, c.ControlMethod,
            c.SamplingSize, c.SamplingFrequency, c.MeasurementTechnique,
            c.ReactionPlan,
            c.ResponsiblePersonId,
            c.ResponsiblePersonId.HasValue ? userNames.GetValueOrDefault(c.ResponsiblePersonId.Value) : null,
            c.Notes)).ToList();

        var revDtos = plan.Revisions.Select(r => new ControlPlanRevisionDto(
            r.Id, r.ControlPlanId, r.RevisionLabel, r.ToStatus, r.Comments,
            r.ChangedById, userNames.GetValueOrDefault(r.ChangedById), r.ChangedAt)).ToList();

        return new ControlPlanDto(
            plan.Id, plan.Title, plan.ProcessItem, plan.PartNumber, plan.PartDescription,
            plan.Revision, plan.Status,
            plan.OwnerId, plan.OwnerId.HasValue ? userNames.GetValueOrDefault(plan.OwnerId.Value) : null,
            plan.LinkedPfmeaId, plan.IsArchived, plan.CreatedAt, plan.UpdatedAt,
            charDtos, revDtos);
    }

    // ── Create ───────────────────────────────────────────────────────────────

    public async Task<ControlPlanSummaryDto> CreateAsync(
        CreateControlPlanRequest request, Guid userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new AppValidationException("Title", "Title is required.");
        if (string.IsNullOrWhiteSpace(request.ProcessItem))
            throw new AppValidationException("ProcessItem", "Process/Item is required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var plan = new ControlPlan
        {
            Title          = request.Title.Trim(),
            ProcessItem    = request.ProcessItem.Trim(),
            PartNumber     = request.PartNumber?.Trim(),
            PartDescription = request.PartDescription?.Trim(),
            Revision       = string.IsNullOrWhiteSpace(request.Revision) ? "Rev A" : request.Revision.Trim(),
            OwnerId        = request.OwnerId,
            LinkedPfmeaId  = request.LinkedPfmeaId,
            Status         = ControlPlanStatus.Draft
        };

        db.ControlPlans.Add(plan);
        await db.SaveChangesAsync(ct);

        string? ownerName = null;
        if (plan.OwnerId.HasValue)
        {
            var owner = await users.FindByIdAsync(plan.OwnerId.Value.ToString());
            ownerName = owner?.DisplayName ?? owner?.UserName;
        }

        return new ControlPlanSummaryDto(
            plan.Id, plan.Title, plan.ProcessItem, plan.PartNumber, plan.Revision,
            plan.Status, plan.OwnerId, ownerName,
            plan.LinkedPfmeaId, plan.IsArchived, 0,
            plan.CreatedAt, plan.UpdatedAt);
    }

    // ── Update header ────────────────────────────────────────────────────────

    public async Task UpdateAsync(Guid id, UpdateControlPlanRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new AppValidationException("Title", "Title is required.");
        if (string.IsNullOrWhiteSpace(request.ProcessItem))
            throw new AppValidationException("ProcessItem", "Process/Item is required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var plan = await db.ControlPlans.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(ControlPlan), id);

        plan.Title          = request.Title.Trim();
        plan.ProcessItem    = request.ProcessItem.Trim();
        plan.PartNumber     = request.PartNumber?.Trim();
        plan.PartDescription = request.PartDescription?.Trim();
        plan.Revision       = string.IsNullOrWhiteSpace(request.Revision) ? plan.Revision : request.Revision.Trim();
        plan.OwnerId        = request.OwnerId;
        plan.LinkedPfmeaId  = request.LinkedPfmeaId;

        await db.SaveChangesAsync(ct);
    }

    // ── Status transition ────────────────────────────────────────────────────

    public async Task<ControlPlanRevisionDto> TransitionStatusAsync(
        Guid id, TransitionStatusRequest request, Guid userId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var plan = await db.ControlPlans.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(ControlPlan), id);

        var allowed = plan.Status switch
        {
            ControlPlanStatus.Draft    => ControlPlanStatus.InReview,
            ControlPlanStatus.InReview => ControlPlanStatus.Approved,
            _ => (ControlPlanStatus?)null
        };

        if (allowed == null || request.NewStatus != allowed)
            throw new AppValidationException("Status",
                $"Cannot transition from {plan.Status} to {request.NewStatus}.");

        plan.Status = request.NewStatus;

        var revision = new ControlPlanRevision
        {
            ControlPlanId = id,
            RevisionLabel = plan.Revision,
            ToStatus      = request.NewStatus,
            Comments      = request.Comments?.Trim(),
            ChangedById   = userId,
            ChangedAt     = DateTimeOffset.UtcNow
        };

        db.ControlPlanRevisions.Add(revision);
        await db.SaveChangesAsync(ct);

        var changer = await users.FindByIdAsync(userId.ToString());
        var changerName = changer?.DisplayName ?? changer?.UserName ?? userId.ToString();

        return new ControlPlanRevisionDto(
            revision.Id, revision.ControlPlanId, revision.RevisionLabel,
            revision.ToStatus, revision.Comments,
            revision.ChangedById, changerName, revision.ChangedAt);
    }

    // ── New revision (supersede → draft copy) ────────────────────────────────

    public async Task<ControlPlanSummaryDto> NewRevisionAsync(
        Guid id, string newRevisionLabel, string? comments, Guid userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(newRevisionLabel))
            throw new AppValidationException("RevisionLabel", "New revision label is required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var plan = await db.ControlPlans
            .Include(p => p.Characteristics)
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException(nameof(ControlPlan), id);

        if (plan.Status != ControlPlanStatus.Approved)
            throw new AppValidationException("Status", "Only Approved control plans can have a new revision created.");

        // Supersede the current plan
        plan.Status = ControlPlanStatus.Superseded;

        var supersededRevision = new ControlPlanRevision
        {
            ControlPlanId = id,
            RevisionLabel = plan.Revision,
            ToStatus      = ControlPlanStatus.Superseded,
            Comments      = comments?.Trim(),
            ChangedById   = userId,
            ChangedAt     = DateTimeOffset.UtcNow
        };
        db.ControlPlanRevisions.Add(supersededRevision);

        // Create new draft plan copying all characteristics
        var newPlan = new ControlPlan
        {
            Title           = plan.Title,
            ProcessItem     = plan.ProcessItem,
            PartNumber      = plan.PartNumber,
            PartDescription = plan.PartDescription,
            Revision        = newRevisionLabel.Trim(),
            OwnerId         = plan.OwnerId,
            LinkedPfmeaId   = plan.LinkedPfmeaId,
            Status          = ControlPlanStatus.Draft
        };

        db.ControlPlans.Add(newPlan);
        await db.SaveChangesAsync(ct); // persist to get newPlan.Id

        var copiedCharacteristics = plan.Characteristics
            .OrderBy(c => c.SortOrder)
            .Select(c => new ControlPlanCharacteristic
            {
                ControlPlanId          = newPlan.Id,
                SortOrder              = c.SortOrder,
                ProcessStep            = c.ProcessStep,
                ProcessOperation       = c.ProcessOperation,
                CharacteristicNo       = c.CharacteristicNo,
                CharacteristicType     = c.CharacteristicType,
                CharacteristicDescription = c.CharacteristicDescription,
                SpecificationTolerance = c.SpecificationTolerance,
                ControlMethod          = c.ControlMethod,
                SamplingSize           = c.SamplingSize,
                SamplingFrequency      = c.SamplingFrequency,
                MeasurementTechnique   = c.MeasurementTechnique,
                ReactionPlan           = c.ReactionPlan,
                ResponsiblePersonId    = c.ResponsiblePersonId,
                Notes                  = c.Notes
            }).ToList();

        db.ControlPlanCharacteristics.AddRange(copiedCharacteristics);
        await db.SaveChangesAsync(ct);

        string? ownerName = null;
        if (newPlan.OwnerId.HasValue)
        {
            var owner = await users.FindByIdAsync(newPlan.OwnerId.Value.ToString());
            ownerName = owner?.DisplayName ?? owner?.UserName;
        }

        return new ControlPlanSummaryDto(
            newPlan.Id, newPlan.Title, newPlan.ProcessItem, newPlan.PartNumber, newPlan.Revision,
            newPlan.Status, newPlan.OwnerId, ownerName,
            newPlan.LinkedPfmeaId, newPlan.IsArchived, copiedCharacteristics.Count,
            newPlan.CreatedAt, newPlan.UpdatedAt);
    }

    // ── Archive ──────────────────────────────────────────────────────────────

    public async Task<bool> ToggleArchiveAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var plan = await db.ControlPlans.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(ControlPlan), id);

        plan.IsArchived = !plan.IsArchived;
        await db.SaveChangesAsync(ct);
        return plan.IsArchived;
    }

    // ── Characteristics ──────────────────────────────────────────────────────

    public async Task<ControlPlanCharacteristicDto> AddCharacteristicAsync(
        AddCharacteristicRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.ProcessStep))
            throw new AppValidationException("ProcessStep", "Process step is required.");
        if (string.IsNullOrWhiteSpace(request.CharacteristicDescription))
            throw new AppValidationException("CharacteristicDescription", "Characteristic description is required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        // Verify plan exists
        var planExists = await db.ControlPlans.AnyAsync(p => p.Id == request.ControlPlanId, ct);
        if (!planExists) throw new NotFoundException(nameof(ControlPlan), request.ControlPlanId);

        // Auto-assign sort order if 0
        var sortOrder = request.SortOrder > 0
            ? request.SortOrder
            : await db.ControlPlanCharacteristics
                .Where(c => c.ControlPlanId == request.ControlPlanId)
                .CountAsync(ct) + 1;

        var characteristic = new ControlPlanCharacteristic
        {
            ControlPlanId            = request.ControlPlanId,
            SortOrder                = sortOrder,
            ProcessStep              = request.ProcessStep.Trim(),
            ProcessOperation         = request.ProcessOperation?.Trim(),
            CharacteristicNo         = request.CharacteristicNo?.Trim(),
            CharacteristicType       = request.CharacteristicType,
            CharacteristicDescription = request.CharacteristicDescription.Trim(),
            SpecificationTolerance   = request.SpecificationTolerance?.Trim(),
            ControlMethod            = request.ControlMethod?.Trim(),
            SamplingSize             = request.SamplingSize?.Trim(),
            SamplingFrequency        = request.SamplingFrequency?.Trim(),
            MeasurementTechnique     = request.MeasurementTechnique?.Trim(),
            ReactionPlan             = request.ReactionPlan?.Trim(),
            ResponsiblePersonId      = request.ResponsiblePersonId,
            Notes                    = request.Notes?.Trim()
        };

        db.ControlPlanCharacteristics.Add(characteristic);
        await db.SaveChangesAsync(ct);

        string? responsibleName = null;
        if (characteristic.ResponsiblePersonId.HasValue)
        {
            var person = await users.FindByIdAsync(characteristic.ResponsiblePersonId.Value.ToString());
            responsibleName = person?.DisplayName ?? person?.UserName;
        }

        return new ControlPlanCharacteristicDto(
            characteristic.Id, characteristic.ControlPlanId, characteristic.SortOrder,
            characteristic.ProcessStep, characteristic.ProcessOperation, characteristic.CharacteristicNo,
            characteristic.CharacteristicType, characteristic.CharacteristicDescription,
            characteristic.SpecificationTolerance, characteristic.ControlMethod,
            characteristic.SamplingSize, characteristic.SamplingFrequency, characteristic.MeasurementTechnique,
            characteristic.ReactionPlan, characteristic.ResponsiblePersonId, responsibleName,
            characteristic.Notes);
    }

    public async Task UpdateCharacteristicAsync(
        Guid characteristicId, UpdateCharacteristicRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.ProcessStep))
            throw new AppValidationException("ProcessStep", "Process step is required.");
        if (string.IsNullOrWhiteSpace(request.CharacteristicDescription))
            throw new AppValidationException("CharacteristicDescription", "Characteristic description is required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var c = await db.ControlPlanCharacteristics.FindAsync([characteristicId], ct)
            ?? throw new NotFoundException(nameof(ControlPlanCharacteristic), characteristicId);

        c.SortOrder                = request.SortOrder > 0 ? request.SortOrder : c.SortOrder;
        c.ProcessStep              = request.ProcessStep.Trim();
        c.ProcessOperation         = request.ProcessOperation?.Trim();
        c.CharacteristicNo         = request.CharacteristicNo?.Trim();
        c.CharacteristicType       = request.CharacteristicType;
        c.CharacteristicDescription = request.CharacteristicDescription.Trim();
        c.SpecificationTolerance   = request.SpecificationTolerance?.Trim();
        c.ControlMethod            = request.ControlMethod?.Trim();
        c.SamplingSize             = request.SamplingSize?.Trim();
        c.SamplingFrequency        = request.SamplingFrequency?.Trim();
        c.MeasurementTechnique     = request.MeasurementTechnique?.Trim();
        c.ReactionPlan             = request.ReactionPlan?.Trim();
        c.ResponsiblePersonId      = request.ResponsiblePersonId;
        c.Notes                    = request.Notes?.Trim();

        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteCharacteristicAsync(Guid characteristicId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var c = await db.ControlPlanCharacteristics.FindAsync([characteristicId], ct)
            ?? throw new NotFoundException(nameof(ControlPlanCharacteristic), characteristicId);

        db.ControlPlanCharacteristics.Remove(c);
        await db.SaveChangesAsync(ct);
    }
}
