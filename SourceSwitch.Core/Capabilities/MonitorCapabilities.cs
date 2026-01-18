namespace SourceSwitch.Core.Capabilities;

/// <summary>
/// Represents the parsed capabilities of a monitor from its DDC/CI capabilities string.
/// </summary>
public sealed class MonitorCapabilities
{
    /// <summary>
    /// Gets the protocol type (e.g., "monitor").
    /// </summary>
    public string Protocol { get; }

    /// <summary>
    /// Gets the display type (e.g., "lcd", "crt").
    /// </summary>
    public string DisplayType { get; }

    /// <summary>
    /// Gets the MCCS (Monitor Control Command Set) version (e.g., "2.1").
    /// </summary>
    public string MccsVersion { get; }

    /// <summary>
    /// Gets the Microsoft WHQL flag value.
    /// </summary>
    public string? MsWhql { get; }

    /// <summary>
    /// Gets the list of MStar commands supported by the monitor.
    /// </summary>
    public IReadOnlyList<string> MStarCommands { get; }

    /// <summary>
    /// Gets the dictionary of VCP features supported by the monitor.
    /// Key is the VCP code (e.g., "60"), value is the feature details.
    /// </summary>
    public IReadOnlyDictionary<string, MonitorVcpFeature> VcpFeatures { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MonitorCapabilities"/> class.
    /// </summary>
    /// <param name="protocol">The protocol type.</param>
    /// <param name="displayType">The display type.</param>
    /// <param name="mccsVersion">The MCCS version.</param>
    /// <param name="msWhql">The Microsoft WHQL flag value.</param>
    /// <param name="mstarCommands">The list of MStar commands.</param>
    /// <param name="vcpFeatures">The list of VCP features.</param>
    public MonitorCapabilities(
        string protocol,
        string displayType,
        string mccsVersion,
        string? msWhql,
        IEnumerable<string> mstarCommands,
        IEnumerable<MonitorVcpFeature> vcpFeatures
    )
    {
        Protocol = protocol;
        DisplayType = displayType;
        MccsVersion = mccsVersion;
        MsWhql = msWhql;
        MStarCommands = new List<string>(mstarCommands).AsReadOnly();

        var features = new Dictionary<string, MonitorVcpFeature>();
        foreach (var feature in vcpFeatures)
        {
            features[feature.Code] = feature;
        }
        VcpFeatures = features;
    }
}
