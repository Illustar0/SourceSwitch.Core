using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using SourceSwitch.Core.Vcp;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace SourceSwitch.Core.Physical;

/// <summary>
/// Represents a physical monitor and provides access to VCP (Virtual Control Panel) features.
/// </summary>
[SupportedOSPlatform("windows6.0.6000")]
public readonly struct PhysicalMonitor(PhysicalMonitorSafeHandle handle)
{
    /// <summary>
    /// Attempts to parse a VCP code string into a byte value.
    /// Supports both hexadecimal formats with "0x" prefix and plain hexadecimal strings.
    /// </summary>
    /// <param name="vcpCode">The VCP code string to parse (e.g., "0x60" or "60").</param>
    /// <param name="codeByte">When this method returns, contains the parsed byte value if successful, or 0 if parsing failed.</param>
    /// <returns>true if the parsing was successful; otherwise, false.</returns>
    private static bool TryParseVcpCode(string vcpCode, out byte codeByte)
    {
        if (string.IsNullOrEmpty(vcpCode))
        {
            codeByte = 0;
            return false;
        }

        if (vcpCode.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            vcpCode = vcpCode[2..];

        return byte.TryParse(
            vcpCode,
            NumberStyles.HexNumber,
            CultureInfo.InvariantCulture,
            out codeByte
        );
    }

    /// <summary>
    /// Gets a VCP feature value.
    /// </summary>
    /// <param name="vcpCode">The VCP code as a string (e.g., "0x60" or "60").</param>
    /// <returns>A tuple containing a success flag, VCP code type, current value, and maximum value.</returns>
    /// <exception cref="ArgumentException">Thrown when vcpCode is null, empty, or not a valid hex string.</exception>
    public (bool success, int pvct, uint currentValue, uint maximumValue) GetVcpFeature(
        string vcpCode
    )
    {
        if (string.IsNullOrEmpty(vcpCode))
            throw new ArgumentException("VCP code cannot be null or empty.", nameof(vcpCode));

        if (!TryParseVcpCode(vcpCode, out var codeByte))
            throw new ArgumentException(
                $"Invalid VCP code format: '{vcpCode}'. Expected hexadecimal value.",
                nameof(vcpCode)
            );

        var result = PInvoke.GetVCPFeatureAndVCPFeatureReply(
            handle,
            codeByte,
            out var pvct,
            out var currentValue,
            out var maximumValue
        );

        return (result != 0, (int)pvct, currentValue, maximumValue);
    }

    /// <summary>
    /// Sets a VCP feature value.
    /// </summary>
    /// <param name="vcpCode">The VCP code (supports hexadecimal formats like "0x60" or "60").</param>
    /// <param name="newValue">The new value to set.</param>
    /// <returns>true if the operation succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when vcpCode is null, empty, or not a valid hex string.</exception>
    public bool SetVcpFeature(string vcpCode, uint newValue)
    {
        if (string.IsNullOrEmpty(vcpCode))
            throw new ArgumentException("VCP code cannot be null or empty.", nameof(vcpCode));

        if (!TryParseVcpCode(vcpCode, out var codeByte))
            throw new ArgumentException(
                $"Invalid VCP code format: '{vcpCode}'. Expected hexadecimal value.",
                nameof(vcpCode)
            );

        return PInvoke.SetVCPFeature(handle, codeByte, newValue) != 0;
    }

    /// <summary>
    /// Sets a VCP feature value.
    /// </summary>
    /// <param name="feature">The VCP feature enumeration.</param>
    /// <param name="newValue">The new value to set.</param>
    /// <returns>true if the operation succeeded; otherwise, false.</returns>
    public bool SetVcpFeature(VcpFeature feature, uint newValue)
    {
        return PInvoke.SetVCPFeature(handle, (byte)feature, newValue) != 0;
    }

    /// <summary>
    /// Gets a VCP feature value.
    /// </summary>
    /// <param name="feature">The VCP feature enumeration.</param>
    /// <returns>A tuple containing a success flag, VCP code type, current value, and maximum value.</returns>
    public (bool success, int pvct, uint currentValue, uint maximumValue) GetVcpFeature(
        VcpFeature feature
    )
    {
        var result = PInvoke.GetVCPFeatureAndVCPFeatureReply(
            handle,
            (byte)feature,
            out var pvct,
            out var currentValue,
            out var maximumValue
        );

        return (result != 0, (int)pvct, currentValue, maximumValue);
    }

    /// <summary>
    /// Gets the DDC/CI capabilities string from the monitor.
    /// </summary>
    /// <returns>The capabilities string describing the monitor's supported features.</returns>
    /// <exception cref="InvalidOperationException">Thrown when unable to retrieve the capabilities string.</exception>
    public unsafe string GetCapabilitiesString()
    {
        if (PInvoke.GetCapabilitiesStringLength(handle, out uint bufferLength) == 0)
        {
            var errorCode = Marshal.GetLastWin32Error();
            throw new InvalidOperationException(
                $"Failed to get capabilities string length. Win32 error code: {errorCode}",
                new Win32Exception(errorCode)
            );
        }

        if (bufferLength == 0)
        {
            return string.Empty;
        }

        var buffer = new byte[bufferLength];

        string resultString;

        fixed (byte* pBuffer = buffer)
        {
            var pstr = new PSTR(pBuffer);

            var result = PInvoke.CapabilitiesRequestAndCapabilitiesReply(
                handle,
                pstr,
                bufferLength
            );

            if (result == 0)
            {
                var errorCode = Marshal.GetLastWin32Error();
                throw new InvalidOperationException(
                    $"Failed to retrieve capabilities string from monitor. Win32 error code: {errorCode}",
                    new Win32Exception(errorCode)
                );
            }

            resultString = pstr.ToString();
        }

        return resultString;
    }
}
