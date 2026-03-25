using TeamStrategyAndTasks.Core.DTOs;

namespace TeamStrategyAndTasks.Core.Interfaces;

public interface IClauseConformanceService
{
    /// <summary>Returns all active clauses ordered by SortOrder with their current assessment state.</summary>
    Task<IReadOnlyList<QualityClauseDto>> GetChecklistAsync(CancellationToken ct = default);

    Task<QualityClauseDto> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Updates the conformance status, assigned owner, due date, and notes on a clause.
    /// Writes a <see cref="ClauseReviewEventDto"/> to history when status changes.
    /// </summary>
    Task UpdateAssessmentAsync(Guid clauseId, UpdateClauseAssessmentRequest request,
        Guid performedByUserId, CancellationToken ct = default);

    Task<IReadOnlyList<ClauseEvidenceItemDto>> GetEvidenceAsync(Guid clauseId, CancellationToken ct = default);

    Task<ClauseEvidenceItemDto> AddEvidenceAsync(AddEvidenceItemRequest request,
        Guid performedByUserId, CancellationToken ct = default);

    Task DeleteEvidenceAsync(Guid evidenceId, Guid performedByUserId, CancellationToken ct = default);

    Task<IReadOnlyList<ClauseReviewEventDto>> GetReviewHistoryAsync(Guid clauseId, CancellationToken ct = default);
}
