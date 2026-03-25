namespace TeamStrategyAndTasks.Core.Entities;

/// <summary>
/// One cause entry on an Ishikawa (fishbone) diagram.
/// Primary bones (ParentCauseId == null) attach to the spine at the 6M category level.
/// Sub-bones attach to existing primary causes.
/// </summary>
public class IshikawaCause : BaseEntity
{
    public Guid RcaCaseId { get; set; }
    public IshikawaCauseCategory Category { get; set; }
    public Guid? ParentCauseId { get; set; }
    public int DisplayOrder { get; set; }
    public string CauseText { get; set; } = string.Empty;
    public bool IsRootCause { get; set; }

    public RcaCase RcaCase { get; set; } = null!;
    public IshikawaCause? ParentCause { get; set; }
    public ICollection<IshikawaCause> SubCauses { get; set; } = [];
}
