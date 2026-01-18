using System.Runtime.Versioning;

namespace SourceSwitch.Core.Physical;

/// <summary>
/// Contains information about a physical monitor including its handle and description.
/// </summary>
[SupportedOSPlatform("windows6.0.6000")]
public readonly struct PhysicalMonitorInfo
{
    /// <summary>
    /// Gets the safe handle to the physical monitor.
    /// </summary>
    internal PhysicalMonitorSafeHandle Handle { get; }

    /// <summary>
    /// Gets the description of the physical monitor (e.g., "Generic PnP Monitor").
    /// </summary>
    public string Description { get; }

    internal PhysicalMonitorInfo(PhysicalMonitorSafeHandle handle, string description)
    {
        Handle = handle;
        Description = description;
    }
}
