namespace SourceSwitch.Core.Capabilities;

/// <summary>
/// Parses DDC/CI monitor capabilities strings into structured data.
/// </summary>
public static class CapabilityParser
{
    /// <summary>
    /// Parses a capabilities string into a MonitorCapabilities object.
    /// </summary>
    /// <param name="capabilitiesString">The capabilities string to parse.</param>
    /// <returns>A MonitorCapabilities object containing the parsed data.</returns>
    /// <exception cref="ArgumentException">Thrown when the input string is null or empty.</exception>
    /// <exception cref="FormatException">Thrown when the input string is malformed.</exception>
    public static MonitorCapabilities Parse(string capabilitiesString)
    {
        if (string.IsNullOrWhiteSpace(capabilitiesString))
            throw new ArgumentException(
                "Capabilities string cannot be null or empty",
                nameof(capabilitiesString)
            );

        // Remove outer parentheses and trim
        var content = capabilitiesString.Trim();
        if (content.StartsWith('(') && content.EndsWith(')'))
        {
            content = content[1..^1];
        }

        var properties = ParseTopLevel(content);

        var protocol = properties.GetValueOrDefault("prot", string.Empty);
        var displayType = properties.GetValueOrDefault("type", string.Empty);
        var mccsVersion = properties.GetValueOrDefault("mccs_ver", string.Empty);
        var msWhql = properties.GetValueOrDefault("mswhql", string.Empty);

        var mstarCommands = ParseSpaceSeparatedValues(
            properties.GetValueOrDefault("MStarcmds", string.Empty)
        );
        var vcpFeatures = ParseVcpFeatures(properties.GetValueOrDefault("vcp", string.Empty));

        return new MonitorCapabilities(
            protocol,
            displayType,
            mccsVersion,
            msWhql,
            mstarCommands,
            vcpFeatures
        );
    }

    /// <summary>
    /// Parses the top-level key-value pairs from the capabilities string.
    /// </summary>
    private static Dictionary<string, string> ParseTopLevel(string content)
    {
        var properties = new Dictionary<string, string>();
        var index = 0;

        while (index < content.Length)
        {
            // Skip whitespace
            while (index < content.Length && char.IsWhiteSpace(content[index]))
                index++;

            if (index >= content.Length)
                break;

            // Find the key
            var keyStart = index;
            while (index < content.Length && content[index] != '(')
                index++;

            if (index >= content.Length)
                break;

            var key = content[keyStart..index].Trim();
            index++; // Skip '('

            // Find the matching closing parenthesis
            var valueStart = index;
            var depth = 1;

            while (index < content.Length && depth > 0)
            {
                if (content[index] == '(')
                    depth++;
                else if (content[index] == ')')
                    depth--;

                index++;
            }

            var value = content[valueStart..(index - 1)];
            properties[key] = value;
        }

        return properties;
    }

    /// <summary>
    /// Parses space-separated hexadecimal values.
    /// </summary>
    private static List<string> ParseSpaceSeparatedValues(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return [];

        return input
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
    }

    /// <summary>
    /// Parses the VCP features section.
    /// </summary>
    private static List<MonitorVcpFeature> ParseVcpFeatures(string vcpContent)
    {
        if (string.IsNullOrWhiteSpace(vcpContent))
            return [];

        var features = new List<MonitorVcpFeature>();
        var index = 0;

        while (index < vcpContent.Length)
        {
            // Skip whitespace
            while (index < vcpContent.Length && char.IsWhiteSpace(vcpContent[index]))
                index++;

            if (index >= vcpContent.Length)
                break;

            // Read VCP code (hex digits)
            var codeStart = index;
            while (index < vcpContent.Length && IsHexDigit(vcpContent[index]))
                index++;

            if (codeStart == index)
            {
                // Not a valid hex code, skip this character
                index++;
                continue;
            }

            var code = vcpContent[codeStart..index].ToUpperInvariant();

            // Check if this VCP code has discrete values in parentheses
            while (index < vcpContent.Length && char.IsWhiteSpace(vcpContent[index]))
                index++;

            List<string> values = [];

            if (index < vcpContent.Length && vcpContent[index] == '(')
            {
                index++; // Skip '('

                // Find matching ')'
                var valuesStart = index;
                var depth = 1;

                while (index < vcpContent.Length && depth > 0)
                {
                    if (vcpContent[index] == '(')
                        depth++;
                    else if (vcpContent[index] == ')')
                        depth--;

                    if (depth > 0)
                        index++;
                }

                var valuesContent = vcpContent[valuesStart..index];
                values = ParseSpaceSeparatedValues(valuesContent);

                if (index < vcpContent.Length && vcpContent[index] == ')')
                    index++; // Skip ')'
            }

            features.Add(new MonitorVcpFeature(code, values));
        }

        return features;
    }

    /// <summary>
    /// Checks if a character is a hexadecimal digit.
    /// </summary>
    private static bool IsHexDigit(char c)
    {
        return c is >= '0' and <= '9' || c is >= 'A' and <= 'F' || c is >= 'a' and <= 'f';
    }
}
