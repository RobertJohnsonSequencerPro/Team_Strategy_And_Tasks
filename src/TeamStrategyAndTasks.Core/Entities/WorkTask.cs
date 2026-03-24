using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Entities;

// Named WorkTask to avoid collision with System.Threading.Tasks.Task
public class WorkTask : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid OwnerId { get; set; }
    public Guid? AssigneeId { get; set; }
    public NodeStatus Status { get; set; } = NodeStatus.NotStarted;
    public decimal? EstimatedEffort { get; set; }
    public decimal? ActualEffort { get; set; }
    public DateTimeOffset? TargetDate { get; set; }
    public DateTimeOffset? CompletionDate { get; set; }
    public bool IsArchived { get; set; }

    /// <summary>Optional: the team primarily responsible for this task.</summary>
    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }

    public ICollection<InitiativeWorkTask> InitiativeWorkTasks { get; set; } = new List<InitiativeWorkTask>();
}
