namespace SourceSwitch.Core.Capabilities;

/// <summary>
/// Represents a VCP (VESA Control Panel) feature supported by the monitor.
/// </summary>
public sealed class MonitorVcpFeature
{
    /// <summary>
    /// Gets the VCP code (e.g., "60" for an input source, "10" for brightness).
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the list of supported values for this VCP feature.
    /// Empty if the feature is continuous (doesn't have discrete values).
    /// </summary>
    public IReadOnlyList<string> SupportedValues { get; }

    /// <summary>
    /// Gets a value indicating whether this feature supports specific discrete values.
    /// </summary>
    public bool HasDiscreteValues => SupportedValues.Count > 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="MonitorVcpFeature"/> class.
    /// </summary>
    /// <param name="code">The VCP code.</param>
    /// <param name="supportedValues">The list of supported values for this feature.</param>
    /// <exception cref="ArgumentNullException">Thrown when code is null.</exception>
    public MonitorVcpFeature(string code, IEnumerable<string> supportedValues)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        SupportedValues = new List<string>(supportedValues).AsReadOnly();
    }

    /// <summary>
    /// Returns a string representation of this VCP feature.
    /// </summary>
    /// <returns>A string describing the VCP code and its supported values.</returns>
    public override string ToString()
    {
        if (HasDiscreteValues)
        {
            return $"VCP {Code}: [{string.Join(", ", SupportedValues)}]";
        }
        return $"VCP {Code}: continuous";
    }
}
