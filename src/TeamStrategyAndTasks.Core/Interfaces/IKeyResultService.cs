using TeamStrategyAndTasks.Core.DTOs;

namespace TeamStrategyAndTasks.Core.Interfaces;

public interface IKeyResultService
{
    Task<IReadOnlyList<KeyResultDto>> GetForObjectiveAsync(Guid objectiveId, CancellationToken ct = default);

    Task<KeyResultDto> AddAsync(AddKeyResultRequest request, Guid performedByUserId, CancellationToken ct = default);

    Task<KeyResultDto> UpdateAsync(Guid id, UpdateKeyResultRequest request, Guid performedByUserId, CancellationToken ct = default);

    Task DeleteAsync(Guid id, Guid performedByUserId, CancellationToken ct = default);
}
