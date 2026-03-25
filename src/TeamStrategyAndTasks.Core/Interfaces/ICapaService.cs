namespace TeamStrategyAndTasks.Core.Interfaces;

public interface ICapaService
{
    // ── CAPA Cases ────────────────────────────────────────────────────────────
    Task<IReadOnlyList<CapaCaseSummaryDto>> GetAllAsync(bool showArchived = false, CancellationToken ct = default);
    Task<CapaCaseDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CapaCaseSummaryDto> CreateAsync(CreateCapaCaseRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateCapaCaseRequest request, CancellationToken ct = default);
    Task TransitionStatusAsync(Guid id, TransitionCapaStatusRequest request, CancellationToken ct = default);
    Task<bool> ToggleArchiveAsync(Guid id, CancellationToken ct = default);

    // ── Actions ───────────────────────────────────────────────────────────────
    Task<CapaActionDto> AddActionAsync(AddCapaActionRequest request, CancellationToken ct = default);
    Task UpdateActionAsync(Guid actionId, UpdateCapaActionRequest request, CancellationToken ct = default);
    Task DeleteActionAsync(Guid actionId, CancellationToken ct = default);

    // ── Effectiveness Checks ──────────────────────────────────────────────────
    Task<EffectivenessCheckDto> AddEffectivenessCheckAsync(
        AddEffectivenessCheckRequest request, Guid checkedById, CancellationToken ct = default);
}
