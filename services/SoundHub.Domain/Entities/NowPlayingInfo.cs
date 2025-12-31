namespace SoundHub.Domain.Entities;

/// <summary>
/// Represents the current playback state of a device.
/// </summary>
public class NowPlayingInfo
{
    /// <summary>
    /// The current media source (e.g., TUNEIN, SPOTIFY, BLUETOOTH, STANDBY).
    /// </summary>
    public required string Source { get; init; }

    /// <summary>
    /// The current track name, if playing.
    /// </summary>
    public string? Track { get; init; }

    /// <summary>
    /// The current artist name, if available.
    /// </summary>
    public string? Artist { get; init; }

    /// <summary>
    /// The current album name, if available.
    /// </summary>
    public string? Album { get; init; }

    /// <summary>
    /// The current station name (for radio sources).
    /// </summary>
    public string? StationName { get; init; }

    /// <summary>
    /// The playback status (e.g., PLAY_STATE, PAUSE_STATE, STOP_STATE, BUFFERING_STATE).
    /// </summary>
    public string? PlayStatus { get; init; }

    /// <summary>
    /// URL to album/station artwork, if available.
    /// </summary>
    public string? ArtUrl { get; init; }
}
