namespace SoundHub.Domain.Entities;

/// <summary>
/// Represents a preset (e.g., internet radio station, playlist) on a device.
/// </summary>
public class Preset
{
    public required string Id { get; init; }
    public required string DeviceId { get; init; }
    public required string Name { get; set; }
    public required string Url { get; set; }
    public required string Type { get; set; } // "InternetRadio", "Playlist", etc.
    public int? Position { get; set; } // Preset button number (1-6 for SoundTouch)
}
