namespace TeamStrategyAndTasks.Core.Enums;

/// <summary>
/// AIAG-aligned classification of a control plan characteristic.
/// </summary>
public enum CharacteristicType
{
    /// <summary>Controls a product dimension or attribute.</summary>
    Product,
    /// <summary>Controls a process parameter (e.g. temperature, pressure, speed).</summary>
    Process,
    /// <summary>Safety-critical characteristic — receives elevated control attention.</summary>
    Safety,
    /// <summary>Dimensional characteristic tied to drawing callouts.</summary>
    Dimensional
}
