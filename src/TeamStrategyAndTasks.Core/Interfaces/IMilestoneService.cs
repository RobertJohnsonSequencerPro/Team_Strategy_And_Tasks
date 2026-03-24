using TeamStrategyAndTasks.Core.DTOs;

namespace TeamStrategyAndTasks.Core.Interfaces;

public interface IMilestoneService
{
    Task<IReadOnlyList<MilestoneDto>> GetForInitiativeAsync(Guid initiativeId, CancellationToken ct = default);

    Task<MilestoneDto> AddAsync(AddMilestoneRequest request, Guid performedByUserId, CancellationToken ct = default);

    Task<MilestoneDto> UpdateAsync(Guid id, UpdateMilestoneRequest request, Guid performedByUserId, CancellationToken ct = default);

    /// <summary>Marks a Pending milestone as Reached and records CompletedAt.</summary>
    Task<MilestoneDto> MarkReachedAsync(Guid id, Guid performedByUserId, CancellationToken ct = default);

    Task DeleteAsync(Guid id, Guid performedByUserId, CancellationToken ct = default);
}
