namespace TeamStrategyAndTasks.Core.Entities;

public class CapaAction : BaseEntity
{
    public Guid CapaCaseId { get; set; }
    public CapaCase CapaCase { get; set; } = null!;

    public string Description { get; set; } = string.Empty;
    public Guid? AssignedToId { get; set; }
    public DateTimeOffset? TargetDate { get; set; }
    public DateTimeOffset? CompletionDate { get; set; }
    public CapaActionStatus Status { get; set; } = CapaActionStatus.Open;
    public string? Notes { get; set; }
}
