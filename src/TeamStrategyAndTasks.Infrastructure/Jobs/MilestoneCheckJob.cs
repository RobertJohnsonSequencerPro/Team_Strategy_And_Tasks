using Microsoft.Extensions.Logging;
using TeamStrategyAndTasks.Core.Interfaces;

namespace TeamStrategyAndTasks.Infrastructure.Jobs;

/// <summary>
/// Hangfire recurring job — runs nightly to flag overdue milestones as Missed
/// and notify Initiative owners.
/// </summary>
public class MilestoneCheckJob(
    IProgressWriteBackService writeBack,
    ILogger<MilestoneCheckJob> logger)
{
    public async Task CheckAsync()
    {
        logger.LogInformation("MilestoneCheckJob: checking for missed milestones at {Time}", DateTimeOffset.UtcNow);
        await writeBack.CheckMissedMilestonesAsync();
    }
}
