using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Core.Interfaces;

public interface ITaskService
{
    Task<IReadOnlyList<WorkTask>> GetAllAsync(CancellationToken ct = default);
    Task<WorkTask> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<WorkTask> CreateAsync(CreateTaskRequest request, Guid ownerId, CancellationToken ct = default);
    Task<WorkTask> UpdateAsync(Guid id, UpdateTaskRequest request, Guid performedByUserId, CancellationToken ct = default);
    Task CompleteAsync(Guid id, CancellationToken ct = default);
    Task ArchiveAsync(Guid id, CancellationToken ct = default);
}
