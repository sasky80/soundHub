namespace SoundHub.Domain.Entities;

/// <summary>
/// Represents a smart audio device (e.g., Bose SoundTouch speaker).
/// Port is defined per vendor adapter, not stored in configuration.
/// Dynamic state (online, power, volume) is read on-demand from the device.
/// </summary>
public class Device
{
    public required string Id { get; init; }
    public required string Vendor { get; init; }
    public required string Name { get; set; }
    public required string IpAddress { get; set; }
    public HashSet<string> Capabilities { get; set; } = new();

    /// <summary>
    /// Timestamp when the device was added. Used for UI highlighting of recently added devices.
    /// </summary>
    public DateTime DateTimeAdded { get; init; } = DateTime.UtcNow;
}
