namespace SoundHub.Domain.Entities;

/// <summary>
/// Represents a preset (e.g., internet radio station, playlist) on a device.
/// </summary>
public class Preset
{
    /// <summary>
    /// Preset slot ID (1-6 for SoundTouch devices).
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// The device ID this preset belongs to.
    /// </summary>
    public required string DeviceId { get; init; }

    /// <summary>
    /// User-friendly name of the preset.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// URL or location of the content (e.g., stream URL, station path).
    /// </summary>
    public required string Location { get; set; }

    /// <summary>
    /// URL to the preset icon/artwork (containerArt from SoundTouch).
    /// </summary>
    public string? IconUrl { get; set; }

    /// <summary>
    /// Content type (e.g., "stationurl" for SoundTouch).
    /// </summary>
    public string Type { get; set; } = "stationurl";

    /// <summary>
    /// Source type (e.g., "LOCAL_INTERNET_RADIO", "TUNEIN" for SoundTouch).
    /// </summary>
    public string Source { get; set; } = "LOCAL_INTERNET_RADIO";

    /// <summary>
    /// Whether this preset can be stored on the device.
    /// </summary>
    public bool IsPresetable { get; set; } = true;
}

/// <summary>
/// Request model for creating or updating a preset.
/// </summary>
public class StorePresetRequest
{
    /// <summary>
    /// Preset slot ID (1-6 for SoundTouch devices).
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// User-friendly name of the preset.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// URL or location of the content (e.g., stream URL, station path).
    /// Optional when StreamUrl is provided (backend derives it automatically).
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// URL to the preset icon/artwork (optional).
    /// </summary>
    public string? IconUrl { get; init; }

    /// <summary>
    /// Content type (default: "stationurl" for SoundTouch).
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Source type (default: "LOCAL_INTERNET_RADIO" for SoundTouch).
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// HTTP stream URL for LOCAL_INTERNET_RADIO presets.
    /// When provided, the backend creates a local station JSON file
    /// and derives the Location automatically.
    /// </summary>
    public string? StreamUrl { get; init; }

    /// <summary>
    /// Whether this is an update to an existing preset (edit mode).
    /// Used to determine create vs update semantics for station files.
    /// </summary>
    public bool IsUpdate { get; init; } = false;
}
