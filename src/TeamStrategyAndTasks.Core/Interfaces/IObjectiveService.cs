using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Core.Interfaces;

public interface IObjectiveService
{
    Task<IReadOnlyList<Objective>> GetAllAsync(CancellationToken ct = default);
    Task<Objective> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Objective> CreateAsync(CreateObjectiveRequest request, Guid ownerId, CancellationToken ct = default);
    Task<Objective> UpdateAsync(Guid id, UpdateObjectiveRequest request, Guid performedByUserId, CancellationToken ct = default);
    Task ArchiveAsync(Guid id, CancellationToken ct = default);
    Task LinkProcessAsync(Guid objectiveId, Guid processId, CancellationToken ct = default);
    Task UnlinkProcessAsync(Guid objectiveId, Guid processId, CancellationToken ct = default);
    Task<IReadOnlyList<Objective>> GetFullHierarchyAsync(CancellationToken ct = default);
}
