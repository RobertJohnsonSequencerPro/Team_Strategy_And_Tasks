namespace TeamStrategyAndTasks.Core.Interfaces;

public interface IRcaService
{
    // ── Cases ─────────────────────────────────────────────────────────────────
    Task<IReadOnlyList<RcaCaseSummaryDto>> GetAllAsync(
        bool showArchived = false,
        string? search = null,
        RcaType? typeFilter = null,
        string? processArea = null,
        CancellationToken ct = default);

    Task<RcaCaseDto> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<RcaCaseSummaryDto> CreateAsync(
        CreateRcaCaseRequest request, CancellationToken ct = default);

    Task UpdateAsync(Guid id, UpdateRcaCaseRequest request, CancellationToken ct = default);

    Task TransitionStatusAsync(
        Guid id, TransitionRcaStatusRequest request, Guid currentUserId,
        CancellationToken ct = default);

    Task<bool> ToggleArchiveAsync(Guid id, CancellationToken ct = default);

    Task SetTagsAsync(Guid id, SetRcaTagsRequest request, CancellationToken ct = default);

    // ── Cross-module ──────────────────────────────────────────────────────────
    Task<IReadOnlyList<RcaCaseSummaryDto>> GetByLinkedCapaCaseAsync(
        Guid capaCaseId, CancellationToken ct = default);

    // ── 5 Whys ────────────────────────────────────────────────────────────────
    Task<FiveWhyNodeDto> AddFiveWhyNodeAsync(
        AddFiveWhyNodeRequest request, CancellationToken ct = default);

    Task UpdateFiveWhyNodeAsync(
        Guid nodeId, UpdateFiveWhyNodeRequest request, CancellationToken ct = default);

    Task DeleteFiveWhyNodeAsync(Guid nodeId, CancellationToken ct = default);

    // ── Ishikawa ──────────────────────────────────────────────────────────────
    Task<IshikawaCauseDto> AddIshikawaCauseAsync(
        AddIshikawaCauseRequest request, CancellationToken ct = default);

    Task UpdateIshikawaCauseAsync(
        Guid causeId, UpdateIshikawaCauseRequest request, CancellationToken ct = default);

    Task DeleteIshikawaCauseAsync(Guid causeId, CancellationToken ct = default);

    // ── Library ───────────────────────────────────────────────────────────────
    Task<IReadOnlyList<RecurringRootCauseDto>> GetRecurringRootCausesAsync(
        int minCount = 2, CancellationToken ct = default);

    Task<IReadOnlyList<string>> GetAllTagsAsync(CancellationToken ct = default);
}
