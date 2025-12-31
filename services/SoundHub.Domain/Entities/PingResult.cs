namespace SoundHub.Domain.Entities;

/// <summary>
/// Result of a device ping operation.
/// </summary>
public record PingResult(bool Reachable, long LatencyMs);
