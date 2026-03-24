using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Core.Interfaces;

public interface ISharedValueService
{
    Task<IReadOnlyList<SharedValue>> GetAllActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SharedValue>> GetAllAdminAsync(CancellationToken ct = default);
    Task<SharedValue> CreateAsync(CreateSharedValueRequest request, CancellationToken ct = default);
    Task<SharedValue> UpdateAsync(Guid id, UpdateSharedValueRequest request, CancellationToken ct = default);
    Task ArchiveAsync(Guid id, CancellationToken ct = default);
    Task MoveUpAsync(Guid id, CancellationToken ct = default);
    Task MoveDownAsync(Guid id, CancellationToken ct = default);
}
