using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Entities;

public class BusinessProcess : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid OwnerId { get; set; }
    public NodeStatus Status { get; set; } = NodeStatus.NotStarted;
    public string? SuccessMetric { get; set; }
    public string? TargetValue { get; set; }
    public DateTimeOffset? TargetDate { get; set; }
    public bool IsArchived { get; set; }

    /// <summary>Optional: the team primarily responsible for this process.</summary>
    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }

    public ICollection<ObjectiveProcess> ObjectiveProcesses { get; set; } = new List<ObjectiveProcess>();
    public ICollection<ProcessInitiative> ProcessInitiatives { get; set; } = new List<ProcessInitiative>();
}
