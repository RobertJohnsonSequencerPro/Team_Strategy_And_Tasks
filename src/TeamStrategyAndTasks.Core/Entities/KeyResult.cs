namespace TeamStrategyAndTasks.Core.Entities;

public class KeyResult : BaseEntity
{
    public Guid ObjectiveId { get; set; }
    public Objective Objective { get; set; } = null!;

    /// <summary>Short description of what is being measured.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Most recently recorded value.</summary>
    public decimal CurrentValue { get; set; }

    /// <summary>Value that represents 100% completion.</summary>
    public decimal TargetValue { get; set; }

    /// <summary>User-defined unit label (e.g. %, $, count, NPS, days).</summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>User responsible for updating progress on this key result.</summary>
    public Guid OwnerId { get; set; }
}
