namespace TeamStrategyAndTasks.Core.Enums;

public enum DependencyType
{
    /// <summary>The blocked node cannot start until the blocker is Done/Complete.</summary>
    FinishToStart,
    /// <summary>The blocked node cannot start until the blocker has started.</summary>
    StartToStart
}
