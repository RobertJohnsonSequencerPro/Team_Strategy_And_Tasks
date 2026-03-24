using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.DTOs;

public record AddRiskRequest(
    NodeType  NodeType,
    Guid      NodeId,
    string    Title,
    string?   Description,
    RiskLevel Probability,
    RiskLevel Impact,
    string?   MitigationPlan,
    Guid      OwnerId);

public record UpdateRiskRequest(
    string    Title,
    string?   Description,
    RiskLevel Probability,
    RiskLevel Impact,
    string?   MitigationPlan,
    Guid      OwnerId,
    RiskStatus Status);

public record NodeRiskDto(
    Guid       Id,
    NodeType   NodeType,
    Guid       NodeId,
    string     NodeTitle,
    string     Title,
    string?    Description,
    RiskLevel  Probability,
    RiskLevel  Impact,
    int        Severity,
    string?    MitigationPlan,
    Guid       OwnerId,
    string     OwnerName,
    RiskStatus Status,
    DateTimeOffset  RaisedAt,
    DateTimeOffset? ResolvedAt);
