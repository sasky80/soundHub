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
}
