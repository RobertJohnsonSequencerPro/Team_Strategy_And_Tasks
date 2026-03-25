using TeamStrategyAndTasks.Core.DTOs;

namespace TeamStrategyAndTasks.Core.Interfaces;

public interface IPfmeaService
{
    // ── Records ──────────────────────────────────────────────────────────────

    Task<IReadOnlyList<PfmeaRecordSummaryDto>> GetAllAsync(
        bool showArchived = false, CancellationToken ct = default);

    Task<PfmeaRecordDto> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<PfmeaRecordSummaryDto> CreateAsync(
        CreatePfmeaRecordRequest request, Guid userId, CancellationToken ct = default);

    Task UpdateAsync(Guid id, UpdatePfmeaRecordRequest request, CancellationToken ct = default);

    /// <summary>Toggles IsArchived. Returns the new archived state.</summary>
    Task<bool> ToggleArchiveAsync(Guid id, CancellationToken ct = default);

    // ── Failure Modes ────────────────────────────────────────────────────────

    Task<PfmeaFailureModeDto> AddFailureModeAsync(
        AddFailureModeRequest request, CancellationToken ct = default);

    Task UpdateFailureModeAsync(
        Guid failureModeId, UpdateFailureModeRequest request, CancellationToken ct = default);

    Task DeleteFailureModeAsync(Guid failureModeId, CancellationToken ct = default);

    // ── Actions ──────────────────────────────────────────────────────────────

    Task<PfmeaActionDto> AddActionAsync(
        AddPfmeaActionRequest request, Guid userId, CancellationToken ct = default);

    Task UpdateActionAsync(
        Guid actionId, UpdatePfmeaActionRequest request, CancellationToken ct = default);

    Task DeleteActionAsync(Guid actionId, CancellationToken ct = default);
}
