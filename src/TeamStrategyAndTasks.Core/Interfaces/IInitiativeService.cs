using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Core.Interfaces;

public interface IInitiativeService
{
    Task<IReadOnlyList<Initiative>> GetAllAsync(CancellationToken ct = default);
    Task<Initiative> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Initiative> CreateAsync(CreateInitiativeRequest request, Guid ownerId, CancellationToken ct = default);
    Task<Initiative> UpdateAsync(Guid id, UpdateInitiativeRequest request, Guid performedByUserId, CancellationToken ct = default);
    Task ArchiveAsync(Guid id, CancellationToken ct = default);
    Task LinkTaskAsync(Guid initiativeId, Guid taskId, CancellationToken ct = default);
    Task UnlinkTaskAsync(Guid initiativeId, Guid taskId, CancellationToken ct = default);
    Task SetResponsibleTeamAsync(Guid id, Guid? teamId, Guid performedByUserId, CancellationToken ct = default);
}
