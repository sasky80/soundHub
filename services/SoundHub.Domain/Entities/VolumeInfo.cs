namespace SoundHub.Domain.Entities;

/// <summary>
/// Represents the current volume state of a device.
/// </summary>
public class VolumeInfo
{
    /// <summary>
    /// The target volume level (0-100).
    /// </summary>
    public required int TargetVolume { get; init; }

    /// <summary>
    /// The actual current volume level (0-100).
    /// </summary>
    public required int ActualVolume { get; init; }

    /// <summary>
    /// Whether the device is muted.
    /// </summary>
    public required bool IsMuted { get; init; }
}
