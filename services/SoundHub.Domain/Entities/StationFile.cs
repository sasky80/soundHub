using System.Text.Json.Serialization;

namespace SoundHub.Domain.Entities;

/// <summary>
/// Represents a local internet radio station JSON file definition.
/// This is the structure written to disk and served to SoundTouch devices.
/// </summary>
public class StationFile
{
    [JsonPropertyName("audio")]
    public StationAudio Audio { get; set; } = new();

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("streamType")]
    public string StreamType { get; set; } = "liveRadio";
}

public class StationAudio
{
    [JsonPropertyName("hasPlaylist")]
    public bool HasPlaylist { get; set; } = false;

    [JsonPropertyName("isRealtime")]
    public bool IsRealtime { get; set; } = true;

    [JsonPropertyName("streamUrl")]
    public string StreamUrl { get; set; } = string.Empty;
}
