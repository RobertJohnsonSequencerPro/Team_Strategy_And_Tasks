using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.DTOs;

public record DecisionDto(
    Guid           Id,
    string         Title,
    string?        Context,
    string?        Rationale,
    string?        AlternativesConsidered,
    Guid           MadeById,
    string         MadeByName,
    DateTimeOffset MadeAt,
    DecisionStatus Status,
    Guid?          SupersededById,
    string?        SupersededByTitle,
    IReadOnlyList<DecisionNodeLinkDto> NodeLinks,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record DecisionNodeLinkDto(NodeType NodeType, Guid NodeId, string NodeTitle);

public record AddDecisionRequest(
    string         Title,
    string?        Context,
    string?        Rationale,
    string?        AlternativesConsidered,
    DateTimeOffset MadeAt,
    /// <summary>Optional: immediately link this decision to a node on creation.</summary>
    NodeType?      InitialNodeType,
    Guid?          InitialNodeId);

public record UpdateDecisionRequest(
    string         Title,
    string?        Context,
    string?        Rationale,
    string?        AlternativesConsidered,
    DateTimeOffset MadeAt);
