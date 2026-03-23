namespace TeamStrategyAndTasks.Web.Services;

/// <summary>
/// Scoped service that carries presentation-mode state across the component tree.
/// MainLayout subscribes to <see cref="StateChanged"/> to hide chrome;
/// communication view pages use it to show the Enter/Exit button.
/// </summary>
public sealed class PresentationModeService
{
    public bool IsPresenting { get; private set; }

    public event Action? StateChanged;

    public void Enter()
    {
        if (IsPresenting) return;
        IsPresenting = true;
        StateChanged?.Invoke();
    }

    public void Exit()
    {
        if (!IsPresenting) return;
        IsPresenting = false;
        StateChanged?.Invoke();
    }

    public void Toggle() { if (IsPresenting) Exit(); else Enter(); }
}
