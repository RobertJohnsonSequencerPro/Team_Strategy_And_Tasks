namespace TeamStrategyAndTasks.Core.Entities;

// Explicit M:M join entity between BusinessProcess and Initiative
public class ProcessInitiative
{
    public Guid ProcessId { get; set; }
    public Guid InitiativeId { get; set; }
    public int DisplayOrder { get; set; }
    public DateTimeOffset LinkedAt { get; init; } = DateTimeOffset.UtcNow;

    public BusinessProcess Process { get; set; } = null!;
    public Initiative Initiative { get; set; } = null!;
}
