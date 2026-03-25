using TeamStrategyAndTasks.Core.DTOs;

namespace TeamStrategyAndTasks.Core.Interfaces;

public interface IControlPlanService
{
    // ── Control Plans ────────────────────────────────────────────────────────

    Task<IReadOnlyList<ControlPlanSummaryDto>> GetAllAsync(
        bool showArchived = false, CancellationToken ct = default);

    Task<ControlPlanDto> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<ControlPlanSummaryDto> CreateAsync(
        CreateControlPlanRequest request, Guid userId, CancellationToken ct = default);

    Task UpdateAsync(Guid id, UpdateControlPlanRequest request, CancellationToken ct = default);

    /// <summary>
    /// Transitions the plan to a new status and writes an immutable revision history record.
    /// Valid transitions: Draft → InReview → Approved. Approved → Superseded is handled
    /// automatically when a new revision is created via <see cref="NewRevisionAsync"/>.
    /// </summary>
    Task<ControlPlanRevisionDto> TransitionStatusAsync(
        Guid id, TransitionStatusRequest request, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Creates a new revision of an approved plan: marks the current plan as Superseded,
    /// creates a new ControlPlan entity with the next revision label (and Status = Draft),
    /// copying all characteristics. Returns the new plan's summary.
    /// </summary>
    Task<ControlPlanSummaryDto> NewRevisionAsync(
        Guid id, string newRevisionLabel, string? comments, Guid userId, CancellationToken ct = default);

    /// <summary>Toggles IsArchived. Returns the new archived state.</summary>
    Task<bool> ToggleArchiveAsync(Guid id, CancellationToken ct = default);

    // ── Characteristics ──────────────────────────────────────────────────────

    Task<ControlPlanCharacteristicDto> AddCharacteristicAsync(
        AddCharacteristicRequest request, CancellationToken ct = default);

    Task UpdateCharacteristicAsync(
        Guid characteristicId, UpdateCharacteristicRequest request, CancellationToken ct = default);

    Task DeleteCharacteristicAsync(Guid characteristicId, CancellationToken ct = default);
}
