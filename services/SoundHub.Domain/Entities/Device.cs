namespace SoundHub.Domain.Entities;

/// <summary>
/// Represents a smart audio device (e.g., Bose SoundTouch speaker).
/// </summary>
public class Device
{
    public required string Id { get; init; }
    public required string Vendor { get; init; }
    public required string Name { get; set; }
    public required string IpAddress { get; set; }
    public int Port { get; set; } = 8090;
    public bool IsOnline { get; set; }
    public bool PowerState { get; set; }
    public int Volume { get; set; }
    public DateTime LastSeen { get; set; }
    public HashSet<string> Capabilities { get; set; } = new();
}
