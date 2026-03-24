using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Core.Interfaces;

public interface ITeamService
{
    Task<IReadOnlyList<Team>> GetAllAsync(CancellationToken ct = default);
    Task<Team> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Team> CreateAsync(CreateTeamRequest request, CancellationToken ct = default);
    Task<Team> UpdateAsync(Guid id, UpdateTeamRequest request, CancellationToken ct = default);
    Task ArchiveAsync(Guid id, CancellationToken ct = default);
    Task AddMemberAsync(Guid teamId, Guid userId, CancellationToken ct = default);
    Task RemoveMemberAsync(Guid teamId, Guid userId, CancellationToken ct = default);
}
