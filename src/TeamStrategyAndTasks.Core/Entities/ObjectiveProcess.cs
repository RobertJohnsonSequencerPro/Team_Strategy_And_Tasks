namespace TeamStrategyAndTasks.Core.Entities;

// Explicit M:M join entity between Objective and BusinessProcess
public class ObjectiveProcess
{
    public Guid ObjectiveId { get; set; }
    public Guid ProcessId { get; set; }
    public int DisplayOrder { get; set; }
    public DateTimeOffset LinkedAt { get; init; } = DateTimeOffset.UtcNow;

    public Objective Objective { get; set; } = null!;
    public BusinessProcess Process { get; set; } = null!;
}
