using Microsoft.Extensions.Logging;
using SoundHub.Domain.Entities;
using SoundHub.Domain.Interfaces;

namespace SoundHub.Application.Services;

/// <summary>
/// Service for managing devices (add, remove, list, discover).
/// </summary>
public class DeviceService
{
    private readonly IDeviceRepository _repository;
    private readonly DeviceAdapterRegistry _adapterRegistry;
    private readonly ILogger<DeviceService> _logger;

    public DeviceService(
        IDeviceRepository repository,
        DeviceAdapterRegistry adapterRegistry,
        ILogger<DeviceService> logger)
    {
        _repository = repository;
        _adapterRegistry = adapterRegistry;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Device>> GetAllDevicesAsync(CancellationToken ct = default)
    {
        return await _repository.GetAllDevicesAsync(ct);
    }

    public async Task<Device?> GetDeviceAsync(string id, CancellationToken ct = default)
    {
        return await _repository.GetDeviceAsync(id, ct);
    }

    public async Task<Device> AddDeviceAsync(string name, string ipAddress, string vendor, int port = 8090, CancellationToken ct = default)
    {
        var device = new Device
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            IpAddress = ipAddress,
            Vendor = vendor,
            Port = port,
            IsOnline = false,
            PowerState = false,
            Volume = 0,
            LastSeen = DateTime.UtcNow
        };

        // Query capabilities from adapter if available
        var adapter = _adapterRegistry.GetAdapter(vendor);
        if (adapter != null)
        {
            try
            {
                var capabilities = await adapter.GetCapabilitiesAsync(device.Id, ct);
                device.Capabilities = new HashSet<string>(capabilities);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to query capabilities for device {DeviceId}", device.Id);
            }
        }

        return await _repository.AddDeviceAsync(device, ct);
    }

    public async Task<bool> RemoveDeviceAsync(string id, CancellationToken ct = default)
    {
        return await _repository.RemoveDeviceAsync(id, ct);
    }

    public async Task<IReadOnlyList<Device>> DiscoverDevicesAsync(string? vendor = null, CancellationToken ct = default)
    {
        var discoveredDevices = new List<Device>();

        var vendors = vendor != null
            ? new[] { vendor }
            : _adapterRegistry.GetRegisteredVendors();

        foreach (var vendorId in vendors)
        {
            var adapter = _adapterRegistry.GetAdapter(vendorId);
            if (adapter == null)
            {
                _logger.LogWarning("No adapter found for vendor {Vendor}", vendorId);
                continue;
            }

            try
            {
                var devices = await adapter.DiscoverDevicesAsync(ct);
                discoveredDevices.AddRange(devices);
                _logger.LogInformation("Discovered {Count} devices for vendor {Vendor}", devices.Count, vendorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to discover devices for vendor {Vendor}", vendorId);
            }
        }

        return discoveredDevices;
    }

    public async Task<DeviceStatus> GetDeviceStatusAsync(string id, CancellationToken ct = default)
    {
        var device = await _repository.GetDeviceAsync(id, ct);
        if (device == null)
        {
            throw new KeyNotFoundException($"Device with ID {id} not found");
        }

        var adapter = _adapterRegistry.GetAdapter(device.Vendor);
        if (adapter == null)
        {
            throw new NotSupportedException($"No adapter found for vendor {device.Vendor}");
        }

        return await adapter.GetStatusAsync(id, ct);
    }
}
