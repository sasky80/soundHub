namespace SoundHub.Domain.Entities;

/// <summary>
/// Detailed device information retrieved from the device.
/// </summary>
public class DeviceInfo
{
    /// <summary>
    /// The device identifier (from deviceID attribute).
    /// </summary>
    public required string DeviceId { get; init; }

    /// <summary>
    /// The device display name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The device type (e.g., "SoundTouch 10", "SoundTouch 300").
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// The device MAC address.
    /// </summary>
    public string? MacAddress { get; init; }

    /// <summary>
    /// The device IP address.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// The device software/firmware version.
    /// </summary>
    public string? SoftwareVersion { get; init; }
}
