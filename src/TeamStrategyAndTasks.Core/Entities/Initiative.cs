using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Entities;

public class Initiative : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid OwnerId { get; set; }
    public NodeStatus Status { get; set; } = NodeStatus.NotStarted;
    public DateTimeOffset? TargetDate { get; set; }
    public bool IsArchived { get; set; }

    public ICollection<ProcessInitiative> ProcessInitiatives { get; set; } = new List<ProcessInitiative>();
    public ICollection<InitiativeWorkTask> InitiativeWorkTasks { get; set; } = new List<InitiativeWorkTask>();
}
