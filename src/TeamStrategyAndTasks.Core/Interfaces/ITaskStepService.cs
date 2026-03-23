using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Core.Interfaces;

public interface ITaskStepService
{
    Task<IReadOnlyList<TaskStep>> GetByTaskAsync(Guid taskId, CancellationToken ct = default);
    Task<TaskStep> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TaskStep> CreateAsync(Guid taskId, CreateTaskStepRequest request, CancellationToken ct = default);
    Task<TaskStep> UpdateAsync(Guid id, UpdateTaskStepRequest request, CancellationToken ct = default);
    Task CompleteAsync(Guid id, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task ReorderAsync(Guid taskId, IReadOnlyList<Guid> orderedIds, CancellationToken ct = default);
}
