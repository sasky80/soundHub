namespace SoundHub.Infrastructure.Adapters;

/// <summary>
/// Configuration options for the SoundTouch adapter.
/// </summary>
public class SoundTouchAdapterOptions
{
    /// <summary>
    /// Timeout in seconds for ping operations. Default: 10 seconds.
    /// </summary>
    public int PingTimeoutSeconds { get; set; } = 10;
}
