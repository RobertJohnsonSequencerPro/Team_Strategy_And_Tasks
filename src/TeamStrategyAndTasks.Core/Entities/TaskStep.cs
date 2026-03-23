using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Entities;

public class TaskStep : BaseEntity
{
    public Guid WorkTaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? AssigneeId { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public int DisplayOrder { get; set; }
    public NodeStatus Status { get; set; } = NodeStatus.NotStarted;
    public DateTimeOffset? CompletionDate { get; set; }

    public WorkTask WorkTask { get; set; } = null!;
}
