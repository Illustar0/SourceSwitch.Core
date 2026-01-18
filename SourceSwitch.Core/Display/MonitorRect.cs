using System.Runtime.Versioning;
using Windows.Win32.Foundation;

namespace SourceSwitch.Core.Display;

/// <summary>
/// Represents a rectangular area defined by left, top, right, and bottom coordinates.
/// </summary>
[SupportedOSPlatform("windows5.0")]
public readonly struct MonitorRect(int leftValue, int topValue, int rightValue, int bottomValue)
{
    /// <summary>
    /// Gets the left coordinate of the area.
    /// </summary>
    public int Left { get; init; } = leftValue;

    /// <summary>
    /// Gets the top coordinate of the area.
    /// </summary>
    public int Top { get; init; } = topValue;

    /// <summary>
    /// Gets the right coordinate of the area.
    /// </summary>
    public int Right { get; init; } = rightValue;

    /// <summary>
    /// Gets the bottom coordinate of the area.
    /// </summary>
    public int Bottom { get; init; } = bottomValue;

    /// <summary>
    /// Gets the width of the area (Right - Left).
    /// </summary>
    public int Width => Right - Left;

    /// <summary>
    /// Gets the height of the area (Bottom - Top).
    /// </summary>
    public int Height => Bottom - Top;

    internal MonitorRect(RECT rect)
        : this(rect.left, rect.top, rect.right, rect.bottom) { }
}
