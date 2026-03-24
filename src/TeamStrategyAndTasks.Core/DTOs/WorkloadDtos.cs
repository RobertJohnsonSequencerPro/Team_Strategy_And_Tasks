namespace TeamStrategyAndTasks.Core.DTOs;

public record UserWorkloadRow(
    Guid   UserId,
    string UserName,
    int    TotalActiveNodes,
    int    OverdueCount,
    int    DueThisWeekCount,
    double TaskCompletionPct);

public record TeamWorkloadRow(
    Guid   TeamId,
    string TeamName,
    int    MemberCount,
    int    TotalActiveNodes,
    int    OverdueCount,
    int    DueThisWeekCount,
    double TaskCompletionPct);
