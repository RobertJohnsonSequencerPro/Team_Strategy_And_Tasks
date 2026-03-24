namespace TeamStrategyAndTasks.Core.Entities;

public class Team : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mandatory statement of this team's scope of responsibility and what
    /// it is authorised to act on and decide within the strategy hierarchy.
    /// This is the canonical, system-of-record answer to "what is this team here to do?"
    /// </summary>
    public string Mandate { get; set; } = string.Empty;

    public bool IsArchived { get; set; }

    public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
}
