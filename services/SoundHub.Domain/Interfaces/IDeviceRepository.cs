using SoundHub.Domain.Entities;

namespace SoundHub.Domain.Interfaces;

/// <summary>
/// Repository for persisting and retrieving device metadata.
/// </summary>
public interface IDeviceRepository
{
    Task<Device?> GetDeviceAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<Device>> GetAllDevicesAsync(CancellationToken ct = default);
    Task<Device> AddDeviceAsync(Device device, CancellationToken ct = default);
    Task<Device> UpdateDeviceAsync(Device device, CancellationToken ct = default);
    Task<bool> RemoveDeviceAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<Device>> GetDevicesByVendorAsync(string vendor, CancellationToken ct = default);

    /// <summary>
    /// Gets the configured network mask for device discovery.
    /// </summary>
    Task<string?> GetNetworkMaskAsync(CancellationToken ct = default);

    /// <summary>
    /// Sets the network mask for device discovery.
    /// </summary>
    /// <param name="networkMask">Network mask in CIDR notation (e.g., "192.168.1.0/24").</param>
    Task SetNetworkMaskAsync(string networkMask, CancellationToken ct = default);
}
