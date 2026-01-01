namespace SoundHub.Domain.Entities;

/// <summary>
/// Represents the current status of a device.
/// </summary>
public class DeviceStatus
{
    public required string DeviceId { get; init; }
    public required bool PowerState { get; init; }
    public required int Volume { get; init; }
    public string? CurrentSource { get; init; }
    public string? CurrentPreset { get; init; }
    public bool IsOnline { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public DeviceCapabilities? Capabilities { get; init; }
}
