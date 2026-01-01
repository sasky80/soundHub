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

    public async Task<Device> AddDeviceAsync(string name, string ipAddress, string vendor, CancellationToken ct = default)
    {
        // Resolve FQDN to IP address if needed
        var resolvedIpAddress = await ResolveHostnameAsync(ipAddress, ct);

        var device = new Device
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            IpAddress = resolvedIpAddress,
            Vendor = vendor,
            DateTimeAdded = DateTime.UtcNow
        };

        // Query capabilities from adapter if available
        var adapter = _adapterRegistry.GetAdapter(vendor);
        if (adapter != null)
        {
            try
            {
                var capabilities = await adapter.GetCapabilitiesAsync(resolvedIpAddress, ct);
                device.Capabilities = new HashSet<string>(capabilities);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to query capabilities for device at {IpAddress}", resolvedIpAddress);
                device.Capabilities = new HashSet<string> { "power", "volume" };
            }
        }

        return await _repository.AddDeviceAsync(device, ct);
    }

    public async Task<Device> UpdateDeviceAsync(string id, string name, string ipAddress, IEnumerable<string>? capabilities = null, CancellationToken ct = default)
    {
        var device = await _repository.GetDeviceAsync(id, ct)
            ?? throw new KeyNotFoundException($"Device with ID {id} not found");

        // Resolve FQDN to IP address if needed
        var resolvedIpAddress = await ResolveHostnameAsync(ipAddress, ct);

        device.Name = name;
        device.IpAddress = resolvedIpAddress;
        if (capabilities != null)
        {
            device.Capabilities = new HashSet<string>(capabilities);
        }

        return await _repository.UpdateDeviceAsync(device, ct);
    }

    private static async Task<string> ResolveHostnameAsync(string hostOrIp, CancellationToken ct)
    {
        // Check if it's already an IP address
        if (System.Net.IPAddress.TryParse(hostOrIp, out _))
        {
            return hostOrIp;
        }

        // Resolve hostname to IP
        try
        {
            var addresses = await System.Net.Dns.GetHostAddressesAsync(hostOrIp, ct);
            var ipv4 = addresses.FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            return ipv4?.ToString() ?? throw new ArgumentException($"Could not resolve hostname: {hostOrIp}");
        }
        catch (System.Net.Sockets.SocketException ex)
        {
            throw new ArgumentException($"Could not resolve hostname: {hostOrIp}", ex);
        }
    }

    public async Task<bool> RemoveDeviceAsync(string id, CancellationToken ct = default)
    {
        return await _repository.RemoveDeviceAsync(id, ct);
    }

    public async Task<PingResult> PingDeviceAsync(string id, CancellationToken ct = default)
    {
        var device = await _repository.GetDeviceAsync(id, ct)
            ?? throw new KeyNotFoundException($"Device with ID {id} not found");

        var adapter = _adapterRegistry.GetAdapter(device.Vendor)
            ?? throw new NotSupportedException($"No adapter found for vendor {device.Vendor}");

        return await adapter.PingAsync(id, ct);
    }

    public async Task<DiscoveryResult> DiscoverAndSaveDevicesAsync(CancellationToken ct = default)
    {
        var networkMask = await _repository.GetNetworkMaskAsync(ct);
        var existingDevices = await _repository.GetAllDevicesAsync(ct);
        var existingIps = existingDevices.Select(d => d.IpAddress).ToHashSet();

        var allDiscovered = new List<Device>();
        var newDevices = new List<Device>();

        foreach (var vendorId in _adapterRegistry.GetRegisteredVendors())
        {
            var adapter = _adapterRegistry.GetAdapter(vendorId);
            if (adapter == null) continue;

            try
            {
                var devices = await adapter.DiscoverDevicesAsync(networkMask, ct);
                foreach (var device in devices)
                {
                    allDiscovered.Add(device);

                    // Skip if device already exists (by IP)
                    if (existingIps.Contains(device.IpAddress))
                    {
                        continue;
                    }

                    // Save new device
                    await _repository.AddDeviceAsync(device, ct);
                    newDevices.Add(device);
                    existingIps.Add(device.IpAddress);
                    _logger.LogInformation("Auto-saved discovered device: {Name} at {IpAddress}", device.Name, device.IpAddress);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to discover devices for vendor {Vendor}", vendorId);
            }
        }

        return new DiscoveryResult(allDiscovered.Count, newDevices.Count, newDevices);
    }

    public async Task<string?> GetNetworkMaskAsync(CancellationToken ct = default)
    {
        return await _repository.GetNetworkMaskAsync(ct);
    }

    public async Task SetNetworkMaskAsync(string networkMask, CancellationToken ct = default)
    {
        // Validate network mask format
        if (!IsValidNetworkMask(networkMask))
        {
            throw new ArgumentException($"Invalid network mask format: {networkMask}. Expected CIDR notation (e.g., 192.168.1.0/24)");
        }

        await _repository.SetNetworkMaskAsync(networkMask, ct);
    }

    public IReadOnlyList<VendorInfo> GetVendors()
    {
        return _adapterRegistry.GetRegisteredVendors()
            .Select(vendorId =>
            {
                var adapter = _adapterRegistry.GetAdapter(vendorId);
                return new VendorInfo(vendorId, adapter?.VendorName ?? vendorId);
            })
            .ToList();
    }

    private static bool IsValidNetworkMask(string networkMask)
    {
        var parts = networkMask.Split('/');
        if (parts.Length != 2) return false;
        if (!int.TryParse(parts[1], out var cidr) || cidr < 0 || cidr > 32) return false;

        var ipParts = parts[0].Split('.');
        if (ipParts.Length != 4) return false;

        foreach (var part in ipParts)
        {
            if (!byte.TryParse(part, out _)) return false;
        }

        return true;
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

    public async Task SetPowerAsync(string id, bool on, CancellationToken ct = default)
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

        await adapter.SetPowerAsync(id, on, ct);
    }

    public async Task<DeviceInfo> GetDeviceInfoAsync(string id, CancellationToken ct = default)
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

        return await adapter.GetDeviceInfoAsync(id, ct);
    }

    public async Task<NowPlayingInfo> GetNowPlayingAsync(string id, CancellationToken ct = default)
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

        return await adapter.GetNowPlayingAsync(id, ct);
    }

    public async Task<VolumeInfo> GetVolumeAsync(string id, CancellationToken ct = default)
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

        return await adapter.GetVolumeAsync(id, ct);
    }

    public async Task SetVolumeAsync(string id, int level, CancellationToken ct = default)
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

        await adapter.SetVolumeAsync(id, level, ct);
    }

    public async Task PressKeyAsync(string id, string keyName, CancellationToken ct = default)
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

        await adapter.PressKeyAsync(id, keyName, ct);
    }

    /// <summary>
    /// Toggles the mute state of a device.
    /// </summary>
    /// <param name="id">The device ID.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task MuteAsync(string id, CancellationToken ct = default)
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

        await adapter.MuteAsync(id, ct);
    }

    public async Task EnterPairingModeAsync(string id, CancellationToken ct = default)
    {
        var device = await _repository.GetDeviceAsync(id, ct);
        if (device == null)
        {
            throw new KeyNotFoundException($"Device with ID {id} not found");
        }

        if (!device.Capabilities.Contains("bluetoothPairing"))
        {
            throw new InvalidOperationException("Device does not support Bluetooth pairing");
        }

        var adapter = _adapterRegistry.GetAdapter(device.Vendor);
        if (adapter == null)
        {
            throw new NotSupportedException($"No adapter found for vendor {device.Vendor}");
        }

        await adapter.EnterPairingModeAsync(id, ct);
    }

    public async Task<IReadOnlyList<Preset>> ListPresetsAsync(string id, CancellationToken ct = default)
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

        return await adapter.ListPresetsAsync(id, ct);
    }

    public async Task<Preset> StorePresetAsync(string id, Preset preset, CancellationToken ct = default)
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

        return await adapter.StorePresetAsync(id, preset, ct);
    }

    public async Task<bool> RemovePresetAsync(string id, int presetId, CancellationToken ct = default)
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

        return await adapter.RemovePresetAsync(id, presetId, ct);
    }

    public async Task PlayPresetAsync(string id, string presetId, CancellationToken ct = default)
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

        await adapter.PlayPresetAsync(id, presetId, ct);
    }
}
