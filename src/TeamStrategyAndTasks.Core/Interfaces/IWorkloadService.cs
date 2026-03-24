namespace TeamStrategyAndTasks.Core.Interfaces;

public interface IWorkloadService
{
    /// <summary>
    /// Returns one row per active user, aggregating owned and assigned node counts,
    /// overdue / due-this-week counts, and task-completion percentage.
    /// </summary>
    Task<IReadOnlyList<UserWorkloadRow>> GetUserWorkloadsAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns one row per active, non-archived team, aggregating the same metrics
    /// across all team members.
    /// </summary>
    Task<IReadOnlyList<TeamWorkloadRow>> GetTeamWorkloadsAsync(CancellationToken ct = default);
}
