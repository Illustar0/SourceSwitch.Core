using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using SourceSwitch.Core.Physical;
using Windows.Win32;
using Windows.Win32.Graphics.Gdi;

namespace SourceSwitch.Core.Display;

/// <summary>
/// Represents a display monitor with its properties and associated operations.
/// </summary>
public readonly struct DisplayMonitor
{
    internal HMONITOR Handle { get; }

    internal DisplayMonitor(HMONITOR handle)
    {
        Handle = handle;
    }

    /// <summary>
    /// Retrieves detailed information about this display monitor.
    /// </summary>
    /// <returns>A DisplayMonitorInfo object containing detailed information, or null if the information cannot be retrieved.</returns>
    [SupportedOSPlatform("windows5.0")]
    public DisplayMonitorInfo? GetMonitorInfo()
    {
        var monitorInfo = DisplayMonitors.CreateMonitorInfoStructure();

        if (!PInvoke.GetMonitorInfo(Handle, ref monitorInfo.monitorInfo))
            return null;

        return new DisplayMonitorInfo(
            monitorInfo.szDevice.ToString(),
            monitorInfo.monitorInfo.dwFlags,
            monitorInfo.monitorInfo.rcMonitor,
            monitorInfo.monitorInfo.rcWork
        );
    }

    /// <summary>
    /// Gets the collection of physical monitors associated with this display monitor.
    /// </summary>
    /// <returns>A collection of physical monitors that must be disposed of after use.</returns>
    /// <exception cref="InvalidOperationException">Thrown when unable to retrieve physical monitor information.</exception>
    [SupportedOSPlatform("windows6.0.6000")]
    public PhysicalMonitors GetPhysicalMonitors()
    {
        return new PhysicalMonitors(Handle);
    }

    /// <summary>
    /// Gets the number of physical monitors associated with this display monitor.
    /// </summary>
    /// <returns>The number of physical monitors.</returns>
    /// <exception cref="InvalidOperationException">Thrown when unable to retrieve physical monitor count.</exception>
    [SupportedOSPlatform("windows6.0.6000")]
    public int GetPhysicalMonitorCount()
    {
        if (!PInvoke.GetNumberOfPhysicalMonitorsFromHMONITOR(Handle, out var count))
        {
            var errorCode = Marshal.GetLastWin32Error();
            throw new InvalidOperationException(
                $"Failed to get the number of physical monitors. Win32 error code: {errorCode}",
                new Win32Exception(errorCode)
            );
        }

        return (int)count;
    }

    /// <summary>
    /// Executes an operation using a physical monitor.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="physicalMonitorIndex">The index of the physical monitor.</param>
    /// <param name="action">The operation to execute.</param>
    /// <returns>The result of the operation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the action is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when unable to retrieve physical monitor information.</exception>
    [SupportedOSPlatform("windows6.0.6000")]
    public T WithPhysicalMonitor<T>(int physicalMonitorIndex, Func<PhysicalMonitor, T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        using var monitors = GetPhysicalMonitors();
        return monitors.WithMonitor(physicalMonitorIndex, action);
    }

    /// <summary>
    /// Executes an operation using the first physical monitor (index 0).
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="action">The operation to execute.</param>
    /// <returns>The result of the operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the action is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when unable to retrieve physical monitor information.</exception>
    [SupportedOSPlatform("windows6.0.6000")]
    public T WithPhysicalMonitor<T>(Func<PhysicalMonitor, T> action)
    {
        return WithPhysicalMonitor(0, action);
    }

    /// <summary>
    /// Executes an operation on all physical monitors associated with this display monitor.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="action">The operation to execute with each physical monitor. Receives the monitor and its index.</param>
    /// <returns>An array of results from each physical monitor.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the action is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when unable to retrieve physical monitor information.</exception>
    [SupportedOSPlatform("windows6.0.6000")]
    public T[] WithAllPhysicalMonitors<T>(Func<PhysicalMonitor, int, T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        using var monitors = GetPhysicalMonitors();
        return monitors.WithAllMonitors(action);
    }

    /// <summary>
    /// Executes an action on all physical monitors associated with this display monitor.
    /// </summary>
    /// <param name="action">The action to execute with each physical monitor. Receives the monitor and its index.</param>
    /// <exception cref="ArgumentNullException">Thrown when the action is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when unable to retrieve physical monitor information.</exception>
    [SupportedOSPlatform("windows6.0.6000")]
    public void ForEachPhysicalMonitor(Action<PhysicalMonitor, int> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        using var monitors = GetPhysicalMonitors();
        monitors.ForEachMonitor(action);
    }
}
