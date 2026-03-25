namespace TeamStrategyAndTasks.Core.Interfaces;

public interface IQualityAuditService
{
    // ── Audits ────────────────────────────────────────────────────────────────
    Task<IReadOnlyList<AuditSummaryDto>> GetAllAsync(bool showArchived = false, CancellationToken ct = default);
    Task<AuditDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AuditSummaryDto> CreateAsync(CreateAuditRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateAuditRequest request, CancellationToken ct = default);
    Task TransitionStatusAsync(Guid id, TransitionAuditStatusRequest request, CancellationToken ct = default);
    Task<bool> ToggleArchiveAsync(Guid id, CancellationToken ct = default);

    // ── Findings ──────────────────────────────────────────────────────────────
    Task<AuditFindingDto> AddFindingAsync(AddFindingRequest request, CancellationToken ct = default);
    Task UpdateFindingAsync(Guid findingId, UpdateFindingRequest request, CancellationToken ct = default);
    Task TransitionFindingStatusAsync(Guid findingId, TransitionFindingStatusRequest request, CancellationToken ct = default);
    Task DeleteFindingAsync(Guid findingId, CancellationToken ct = default);
}
