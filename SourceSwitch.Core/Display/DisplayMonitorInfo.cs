using System.Runtime.Versioning;
using Windows.Win32.Foundation;

namespace SourceSwitch.Core.Display;

/// <summary>
/// Contains detailed information about a display monitor.
/// </summary>
[SupportedOSPlatform("windows5.0")]
public readonly struct DisplayMonitorInfo
{
    /// <summary>
    /// Gets the device name of the monitor (e.g., "\\\\.\\DISPLAY1").
    /// </summary>
    public string DeviceName { get; init; }

    /// <summary>
    /// Gets the monitor flags from the system.
    /// </summary>
    public uint Flags { get; init; }

    /// <summary>
    /// Gets the full area of the monitor in screen coordinates.
    /// </summary>
    public MonitorRect MonitorArea { get; init; }

    /// <summary>
    /// Gets the working area of the monitor (excluding taskbar and docked windows).
    /// </summary>
    public MonitorRect WorkArea { get; init; }

    /// <summary>
    /// Gets a value indicating whether this is the primary monitor.
    /// </summary>
    public bool IsPrimary { get; init; }

    internal DisplayMonitorInfo(string deviceName, uint flags, RECT monitorRect, RECT workRect)
    {
        DeviceName = deviceName;
        Flags = flags;
        MonitorArea = new MonitorRect(monitorRect);
        WorkArea = new MonitorRect(workRect);
        IsPrimary = (flags & 1) != 0;
    }
}
