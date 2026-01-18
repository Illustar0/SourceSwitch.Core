using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace SourceSwitch.Core.Display;

/// <summary>
/// Manages and enumerates display monitors on the system.
/// Provides access to monitor information and physical monitor controls.
/// </summary>
[SupportedOSPlatform("windows5.0")]
public class DisplayMonitors
{
    private List<HMONITOR> HMonitors { get; } = [];

    /// <summary>
    /// Creates a properly initialized MONITORINFOEXW structure for querying monitor information.
    /// </summary>
    /// <returns>A new MONITORINFOEXW structure with the correct size set.</returns>
    internal static unsafe MONITORINFOEXW CreateMonitorInfoStructure()
    {
        return new MONITORINFOEXW
        {
            monitorInfo = new MONITORINFO { cbSize = (uint)sizeof(MONITORINFOEXW) },
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DisplayMonitors"/> class and enumerates all display monitors.
    /// </summary>
    public DisplayMonitors()
    {
        var gcHandle = GCHandle.Alloc(this);
        try
        {
            unsafe
            {
                PInvoke.EnumDisplayMonitors(
                    default,
                    null,
                    &EnumMonitorsCallback,
                    GCHandle.ToIntPtr(gcHandle)
                );
            }
        }
        finally
        {
            gcHandle.Free();
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvStdcall)])]
    private static unsafe BOOL EnumMonitorsCallback(
        HMONITOR hMonitor,
        HDC hdc,
        RECT* lprcMonitor,
        LPARAM lParam
    )
    {
        var handle = GCHandle.FromIntPtr(lParam);
        var instance = (DisplayMonitors)handle.Target!;
        instance.HMonitors.Add(hMonitor);
        return true;
    }

    /// <summary>
    /// Gets a display monitor by its device name.
    /// </summary>
    /// <param name="name">The device name (e.g., "\\\\.\\DISPLAY1").</param>
    /// <returns>A DisplayMonitor if found, otherwise null.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or empty.</exception>
    public DisplayMonitor? GetDisplayMonitorByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Monitor name cannot be empty", nameof(name));

        foreach (var hMonitor in HMonitors)
        {
            var monitorInfo = CreateMonitorInfoStructure();

            if (!PInvoke.GetMonitorInfo(hMonitor, ref monitorInfo.monitorInfo))
                continue;

            var deviceName = monitorInfo.szDevice.ToString();
            if (deviceName == name)
            {
                return new DisplayMonitor(hMonitor);
            }
        }

        return null;
    }

    /// <summary>
    /// Gets all display monitors.
    /// </summary>
    /// <returns>An enumerable collection of all display monitors.</returns>
    public IEnumerable<DisplayMonitor> GetAllMonitors()
    {
        foreach (var hMonitor in HMonitors)
        {
            yield return new DisplayMonitor(hMonitor);
        }
    }

    /// <summary>
    /// Gets the primary display monitor.
    /// </summary>
    /// <returns>The primary DisplayMonitor if found, otherwise null.</returns>
    public static DisplayMonitor GetPrimaryMonitor()
    {
        var hMonitor = PInvoke.MonitorFromWindow(
            HWND.Null,
            MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY
        );

        return new DisplayMonitor(hMonitor);
    }

    /// <summary>
    /// Gets the display monitor where the cursor is currently located.
    /// </summary>
    /// <returns>The DisplayMonitor containing the cursor position.</returns>
    public static DisplayMonitor GetCurrentMonitor()
    {
        PInvoke.GetCursorPos(out var cursorPos);
        var hMonitor = PInvoke.MonitorFromPoint(
            cursorPos,
            MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST
        );
        return new DisplayMonitor(hMonitor);
    }

    /// <summary>
    /// Gets the number of display monitors enumerated.
    /// </summary>
    public int Count => HMonitors.Count;
}
