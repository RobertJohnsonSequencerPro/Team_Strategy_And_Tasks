namespace TeamStrategyAndTasks.Core.Entities;

/// <summary>
/// One node in a branching 5-Whys tree. Any node may have multiple children,
/// enabling branching when a single "why?" reveals more than one contributing cause.
/// </summary>
public class FiveWhyNode : BaseEntity
{
    public Guid RcaCaseId { get; set; }
    public Guid? ParentId { get; set; }
    public int DisplayOrder { get; set; }
    public string WhyQuestion { get; set; } = string.Empty;
    public string? BecauseAnswer { get; set; }
    public bool IsRootCause { get; set; }

    public RcaCase RcaCase { get; set; } = null!;
    public FiveWhyNode? Parent { get; set; }
    public ICollection<FiveWhyNode> Children { get; set; } = [];
}
