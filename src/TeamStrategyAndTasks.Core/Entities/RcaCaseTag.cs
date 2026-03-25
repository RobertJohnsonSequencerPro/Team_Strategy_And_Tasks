namespace TeamStrategyAndTasks.Core.Entities;

public class RcaCaseTag
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid RcaCaseId { get; set; }
    public string Tag { get; set; } = string.Empty;

    public RcaCase RcaCase { get; set; } = null!;
}
