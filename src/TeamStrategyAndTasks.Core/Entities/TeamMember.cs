namespace TeamStrategyAndTasks.Core.Entities;

// Explicit M:M join entity between Team and the Identity user table.
public class TeamMember
{
    public Guid TeamId { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset JoinedAt { get; init; } = DateTimeOffset.UtcNow;

    public Team Team { get; set; } = null!;
}
