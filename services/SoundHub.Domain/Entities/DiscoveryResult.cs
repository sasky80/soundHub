namespace SoundHub.Domain.Entities;

/// <summary>
/// Result of a device discovery operation.
/// </summary>
public record DiscoveryResult(int Discovered, int New, IReadOnlyList<Device> Devices);
