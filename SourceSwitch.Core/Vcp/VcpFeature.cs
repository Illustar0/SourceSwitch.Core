using System.Runtime.Versioning;

namespace SourceSwitch.Core.Vcp;

/// <summary>
/// VESA Monitor Control Command Set (MCCS) VCP code enumeration.
/// </summary>
/// <remarks>
/// Common monitor control codes based on the VESA MCCS standard.
/// </remarks>
[SupportedOSPlatform("windows6.0.6000")]
public enum VcpFeature : byte
{
    /// <summary>
    /// Brightness control (0x10).
    /// </summary>
    Brightness = 0x10,

    /// <summary>
    /// Contrast control (0x12).
    /// </summary>
    Contrast = 0x12,

    /// <summary>
    /// Color temperature control (0x14).
    /// </summary>
    ColorTemperature = 0x14,

    /// <summary>
    /// Red video gain (0x16).
    /// </summary>
    RedVideoGain = 0x16,

    /// <summary>
    /// Green video gain (0x18).
    /// </summary>
    GreenVideoGain = 0x18,

    /// <summary>
    /// Blue video gain (0x1A).
    /// </summary>
    BlueVideoGain = 0x1A,

    /// <summary>
    /// Input source selection (0x60) - Used to switch between HDMI, DP, etc.
    /// </summary>
    InputSource = 0x60,

    /// <summary>
    /// Audio speaker volume (0x62).
    /// </summary>
    AudioSpeakerVolume = 0x62,

    /// <summary>
    /// Power mode control (0xD6).
    /// </summary>
    PowerMode = 0xD6,
}
