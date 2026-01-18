using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using SourceSwitch.Core.Display;
using Windows.Win32;
using Windows.Win32.Devices.Display;
using Windows.Win32.Graphics.Gdi;

namespace SourceSwitch.Core.Physical;

/// <summary>
/// Represents a collection of physical monitors associated with a display monitor.
/// Provides methods to access and control physical monitors via DDC/CI.
/// This class must be disposed of after use to release monitor handles.
/// </summary>
[SupportedOSPlatform("windows6.0.6000")]
public sealed class PhysicalMonitors : IDisposable
{
    private readonly PhysicalMonitorInfo[] _monitors;
    private bool _disposed;

    /// <summary>
    /// Creates a physical monitor collection.
    /// </summary>
    /// <param name="hMonitor">The monitor handle.</param>
    /// <exception cref="ArgumentException">Thrown when the handle is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when unable to retrieve physical monitor information.</exception>
    public PhysicalMonitors(IntPtr hMonitor)
    {
        if (hMonitor == IntPtr.Zero)
            throw new ArgumentException("Monitor handle cannot be zero.", nameof(hMonitor));

        if (
            !PInvoke.GetNumberOfPhysicalMonitorsFromHMONITOR(
                (HMONITOR)hMonitor,
                out var physicalMonitorNum
            )
        )
        {
            var errorCode = Marshal.GetLastWin32Error();
            throw new InvalidOperationException(
                $"Failed to get the number of physical monitors for handle 0x{hMonitor:X}. Win32 error code: {errorCode}",
                new Win32Exception(errorCode)
            );
        }

        if (physicalMonitorNum == 0)
            throw new InvalidOperationException(
                $"Monitor handle 0x{hMonitor:X} has no associated physical monitors. "
                    + "This may indicate the monitor does not support DDC/CI."
            );

        var tempMonitors = new PHYSICAL_MONITOR[physicalMonitorNum];

        if (!PInvoke.GetPhysicalMonitorsFromHMONITOR((HMONITOR)hMonitor, tempMonitors))
        {
            var errorCode = Marshal.GetLastWin32Error();
            throw new InvalidOperationException(
                $"Failed to retrieve physical monitor information from handle 0x{hMonitor:X}. Win32 error code: {errorCode}",
                new Win32Exception(errorCode)
            );
        }

        // Wrap HANDLE and description into PhysicalMonitorInfo
        _monitors = new PhysicalMonitorInfo[physicalMonitorNum];
        for (int i = 0; i < physicalMonitorNum; i++)
        {
            var handle = new PhysicalMonitorSafeHandle(tempMonitors[i].hPhysicalMonitor);
            var description = tempMonitors[i].szPhysicalMonitorDescription.ToString();
            _monitors[i] = new PhysicalMonitorInfo(handle, description);
        }
    }

    /// <summary>
    /// Creates a physical monitor collection from a display monitor.
    /// </summary>
    /// <param name="displayMonitor">The display monitor.</param>
    /// <exception cref="InvalidOperationException">Thrown when unable to retrieve physical monitor information.</exception>
    public PhysicalMonitors(DisplayMonitor displayMonitor)
        : this(displayMonitor.Handle) { }

    /// <summary>
    /// Gets the number of physical monitors in this collection.
    /// </summary>
    public int Count => _monitors.Length;

    /// <summary>
    /// Gets the description of a physical monitor at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the physical monitor.</param>
    /// <returns>The description of the physical monitor.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the object has been disposed of.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
    public string GetDescription(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _monitors.Length);

        return _monitors[index].Description;
    }

    /// <summary>
    /// Gets all physical monitor descriptions.
    /// </summary>
    /// <returns>An array of physical monitor descriptions.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the object has been disposed of.</exception>
    public string[] GetAllDescriptions()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var descriptions = new string[_monitors.Length];
        for (int i = 0; i < _monitors.Length; i++)
        {
            descriptions[i] = _monitors[i].Description;
        }
        return descriptions;
    }

    /// <summary>
    /// Releases all resources used by this instance.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Executes an action with a specified physical monitor.
    /// </summary>
    /// <typeparam name="T">The return type of the action.</typeparam>
    /// <param name="index">The zero-based index of the physical monitor to access.</param>
    /// <param name="action">The action to execute with the physical monitor.</param>
    /// <returns>The result returned by the action.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the object has been disposed of.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the action is null.</exception>
    public T WithMonitor<T>(int index, Func<PhysicalMonitor, T> action)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(action);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _monitors.Length);

        var monitorControl = new PhysicalMonitor(_monitors[index].Handle);

        return action(monitorControl);
    }

    /// <summary>
    /// Executes an action on all physical monitors.
    /// </summary>
    /// <typeparam name="T">The return type of the action.</typeparam>
    /// <param name="action">The action to execute with each physical monitor. Receives the monitor and its index.</param>
    /// <returns>An array of results from each physical monitor.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the object has been disposed of.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the action is null.</exception>
    public T[] WithAllMonitors<T>(Func<PhysicalMonitor, int, T> action)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(action);

        var results = new T[_monitors.Length];
        for (int i = 0; i < _monitors.Length; i++)
        {
            var monitorControl = new PhysicalMonitor(_monitors[i].Handle);
            results[i] = action(monitorControl, i);
        }
        return results;
    }

    /// <summary>
    /// Executes an action on all physical monitors without returning a value.
    /// </summary>
    /// <param name="action">The action to execute with each physical monitor. Receives the monitor and its index.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the object has been disposed of.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the action is null.</exception>
    public void ForEachMonitor(Action<PhysicalMonitor, int> action)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(action);

        for (int i = 0; i < _monitors.Length; i++)
        {
            var monitorControl = new PhysicalMonitor(_monitors[i].Handle);
            action(monitorControl, i);
        }
    }

    /// <summary>
    /// Finalizer to ensure resources are released.
    /// </summary>
    ~PhysicalMonitors()
    {
        Dispose(false);
    }

    /// <summary>
    /// Releases physical monitor resources.
    /// </summary>
    /// <param name="disposing">Indicates whether managed resources are being released.</param>
    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // SafeHandle automatically handles release via DestroyPhysicalMonitor
            foreach (var monitor in _monitors)
            {
                monitor.Handle.Dispose();
            }
        }

        _disposed = true;
    }
}
