using System.Runtime.Versioning;
using Microsoft.Win32.SafeHandles;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace SourceSwitch.Core.Physical;

/// <summary>
/// Represents a safe handle to a physical monitor that ensures proper resource cleanup.
/// </summary>
[SupportedOSPlatform("windows6.0.6000")]
public sealed class PhysicalMonitorSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PhysicalMonitorSafeHandle"/> class.
    /// </summary>
    public PhysicalMonitorSafeHandle()
        : base(ownsHandle: true) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PhysicalMonitorSafeHandle"/> class with the specified handle.
    /// </summary>
    /// <param name="handle">The physical monitor handle to wrap.</param>
    public PhysicalMonitorSafeHandle(IntPtr handle)
        : base(ownsHandle: true)
    {
        SetHandle(handle);
    }

    /// <summary>
    /// Releases the physical monitor handle by calling DestroyPhysicalMonitor.
    /// </summary>
    /// <returns>true if the handle was released successfully; otherwise, false.</returns>
    protected override bool ReleaseHandle()
    {
        return PInvoke.DestroyPhysicalMonitor((HANDLE)handle);
    }
}
