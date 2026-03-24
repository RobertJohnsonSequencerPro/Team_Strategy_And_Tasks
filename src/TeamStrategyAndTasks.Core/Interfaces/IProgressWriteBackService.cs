namespace TeamStrategyAndTasks.Core.Interfaces;

public interface IProgressWriteBackService
{
    /// <summary>Recalculate status for all Initiatives that contain this Task, then cascade up.</summary>
    Task RecalculateFromTaskAsync(Guid taskId, CancellationToken ct = default);

    /// <summary>Recalculate status for a specific Initiative, then cascade up to Processes and Objectives.</summary>
    Task RecalculateFromInitiativeAsync(Guid initiativeId, CancellationToken ct = default);
}
