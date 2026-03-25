namespace TeamStrategyAndTasks.Web.Services;

public enum WorkspaceType
{
    StrategicPlanning,
    QualityEngineering
}

/// <summary>
/// Tracks the active workspace for the current user session.
/// Scoped per Blazor circuit (one instance per connected user).
/// </summary>
public class WorkspaceService
{
    public WorkspaceType Current { get; private set; } = WorkspaceType.StrategicPlanning;

    public event Action? StateChanged;

    public void SwitchTo(WorkspaceType workspace)
    {
        if (Current == workspace) return;
        Current = workspace;
        StateChanged?.Invoke();
    }

    /// <summary>
    /// Infers the correct workspace from the current route path and switches
    /// if needed. Called on every navigation so deep links work without
    /// requiring an explicit switcher click.
    /// </summary>
    public void InferFromPath(string absolutePath)
    {
        var target = absolutePath.StartsWith("/quality", StringComparison.OrdinalIgnoreCase)
            ? WorkspaceType.QualityEngineering
            : WorkspaceType.StrategicPlanning;
        SwitchTo(target);
    }
}
