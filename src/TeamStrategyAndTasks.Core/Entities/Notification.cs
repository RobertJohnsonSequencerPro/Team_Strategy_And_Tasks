using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Entities;

public class Notification
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public NodeType? RelatedNodeType { get; set; }
    public Guid? RelatedNodeId { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
