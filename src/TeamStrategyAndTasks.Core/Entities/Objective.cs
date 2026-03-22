using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Entities;

public class Objective : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid OwnerId { get; set; }
    public NodeStatus Status { get; set; } = NodeStatus.NotStarted;
    public string? SuccessMetric { get; set; }
    public string? TargetValue { get; set; }
    public DateTimeOffset? TargetDate { get; set; }
    public bool IsArchived { get; set; }

    public ICollection<ObjectiveProcess> ObjectiveProcesses { get; set; } = new List<ObjectiveProcess>();
}
