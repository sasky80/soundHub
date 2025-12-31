namespace SoundHub.Domain.Entities;

/// <summary>
/// Global device configuration including network settings.
/// </summary>
public class DeviceConfiguration
{
    /// <summary>
    /// Network mask in CIDR notation (e.g., "192.168.1.0/24") for device discovery.
    /// </summary>
    public string? NetworkMask { get; set; }
}
