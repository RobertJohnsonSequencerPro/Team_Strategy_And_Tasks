using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Core.Interfaces;

public interface IProcessService
{
    Task<IReadOnlyList<BusinessProcess>> GetAllAsync(CancellationToken ct = default);
    Task<BusinessProcess> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<BusinessProcess> CreateAsync(CreateProcessRequest request, Guid ownerId, CancellationToken ct = default);
    Task<BusinessProcess> UpdateAsync(Guid id, UpdateProcessRequest request, CancellationToken ct = default);
    Task ArchiveAsync(Guid id, CancellationToken ct = default);
    Task LinkInitiativeAsync(Guid processId, Guid initiativeId, CancellationToken ct = default);
    Task UnlinkInitiativeAsync(Guid processId, Guid initiativeId, CancellationToken ct = default);
}
