using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Entities;

public class Milestone : BaseEntity
{
    public Guid InitiativeId { get; set; }
    public Initiative Initiative { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public DateTimeOffset DueDate { get; set; }

    public MilestoneStatus Status { get; set; } = MilestoneStatus.Pending;

    /// <summary>Set when Status transitions to Reached.</summary>
    public DateTimeOffset? CompletedAt { get; set; }

    public string? Notes { get; set; }

    /// <summary>User responsible for this milestone.</summary>
    public Guid OwnerId { get; set; }
}
