namespace TeamStrategyAndTasks.Core.Entities;

// Explicit M:M join entity between Initiative and WorkTask
public class InitiativeWorkTask
{
    public Guid InitiativeId { get; set; }
    public Guid WorkTaskId { get; set; }
    public int DisplayOrder { get; set; }
    public DateTimeOffset LinkedAt { get; init; } = DateTimeOffset.UtcNow;

    public Initiative Initiative { get; set; } = null!;
    public WorkTask WorkTask { get; set; } = null!;
}
