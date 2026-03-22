using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Entities;

public class AuditLog
{
    // Long sequential ID for append-only audit records
    public long Id { get; init; }
    public NodeType NodeType { get; set; }
    public Guid NodeId { get; set; }
    public Guid UserId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
