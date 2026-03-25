namespace TeamStrategyAndTasks.Core.Entities;

public class EffectivenessCheck : BaseEntity
{
    public Guid CapaCaseId { get; set; }
    public CapaCase CapaCase { get; set; } = null!;

    public DateTimeOffset CheckDate { get; set; } = DateTimeOffset.UtcNow;
    public Guid CheckedById { get; set; }
    public EffectivenessVerdict Verdict { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset? NextCheckDate { get; set; }
}
