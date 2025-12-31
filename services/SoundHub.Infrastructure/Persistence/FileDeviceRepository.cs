using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoundHub.Domain.Entities;
using SoundHub.Domain.Interfaces;

namespace SoundHub.Infrastructure.Persistence;

/// <summary>
/// File-based device repository using devices.json with vendor grouping.
/// </summary>
public class FileDeviceRepository : IDeviceRepository
{
    private readonly string _filePath;
    private readonly ILogger<FileDeviceRepository> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public FileDeviceRepository(IOptions<FileDeviceRepositoryOptions> options, ILogger<FileDeviceRepository> logger)
    {
        _filePath = options.Value.FilePath;
        _logger = logger;
        EnsureFileExists();
    }

    private void EnsureFileExists()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (!File.Exists(_filePath))
        {
            File.WriteAllText(_filePath, "{}");
        }
    }

    private async Task<DevicesFileRoot> LoadFromFileAsync(CancellationToken ct)
    {
        try
        {
            var json = await File.ReadAllTextAsync(_filePath, ct).ConfigureAwait(false);
            var data = JsonSerializer.Deserialize<DevicesFileRoot>(json, JsonOptions);
            return data ?? new DevicesFileRoot();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load devices from {FilePath}", _filePath);
            return new DevicesFileRoot();
        }
    }

    private async Task SaveToFileAsync(DevicesFileRoot data, CancellationToken ct)
    {
        // Build the output dictionary with NetworkMask at root and vendor groups
        var output = new Dictionary<string, object?>();
        if (data.NetworkMask != null)
        {
            output["NetworkMask"] = data.NetworkMask;
        }
        foreach (var (key, group) in data.VendorGroups)
        {
            output[key] = group;
        }

        var json = JsonSerializer.Serialize(output, JsonOptions);
        await File.WriteAllTextAsync(_filePath, json, ct).ConfigureAwait(false);
    }

    public async Task<Device?> GetDeviceAsync(string id, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var data = await LoadFromFileAsync(ct).ConfigureAwait(false);
            foreach (var vendorGroup in data.VendorGroups.Values)
            {
                var dto = vendorGroup.Devices.FirstOrDefault(d => d.Id == id);
                if (dto != null)
                {
                    return DtoToEntity(dto);
                }
            }
            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IReadOnlyList<Device>> GetAllDevicesAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var data = await LoadFromFileAsync(ct).ConfigureAwait(false);
            return data.VendorGroups.Values
                .SelectMany(g => g.Devices)
                .Select(DtoToEntity)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Device> AddDeviceAsync(Device device, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var data = await LoadFromFileAsync(ct).ConfigureAwait(false);
            var vendorKey = GetVendorKey(device.Vendor);
            if (!data.VendorGroups.ContainsKey(vendorKey))
            {
                data.VendorGroups[vendorKey] = new VendorGroup();
            }

            data.VendorGroups[vendorKey].Devices.Add(EntityToDto(device));
            await SaveToFileAsync(data, ct).ConfigureAwait(false);
            return device;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Device> UpdateDeviceAsync(Device device, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var data = await LoadFromFileAsync(ct).ConfigureAwait(false);
            foreach (var (vendorKey, vendorGroup) in data.VendorGroups)
            {
                var index = vendorGroup.Devices.FindIndex(d => d.Id == device.Id);
                if (index >= 0)
                {
                    vendorGroup.Devices[index] = EntityToDto(device);
                    await SaveToFileAsync(data, ct).ConfigureAwait(false);
                    return device;
                }
            }
            throw new KeyNotFoundException($"Device with ID {device.Id} not found");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> RemoveDeviceAsync(string id, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var data = await LoadFromFileAsync(ct).ConfigureAwait(false);
            foreach (var (vendorKey, vendorGroup) in data.VendorGroups)
            {
                var removed = vendorGroup.Devices.RemoveAll(d => d.Id == id) > 0;
                if (removed)
                {
                    await SaveToFileAsync(data, ct).ConfigureAwait(false);
                    return true;
                }
            }
            return false;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IReadOnlyList<Device>> GetDevicesByVendorAsync(string vendor, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var data = await LoadFromFileAsync(ct).ConfigureAwait(false);
            var vendorKey = GetVendorKey(vendor);
            if (data.VendorGroups.TryGetValue(vendorKey, out var vendorGroup))
            {
                return vendorGroup.Devices.Select(DtoToEntity).ToList();
            }
            return Array.Empty<Device>();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<string?> GetNetworkMaskAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var data = await LoadFromFileAsync(ct).ConfigureAwait(false);
            return data.NetworkMask;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SetNetworkMaskAsync(string networkMask, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var data = await LoadFromFileAsync(ct).ConfigureAwait(false);
            data.NetworkMask = networkMask;
            await SaveToFileAsync(data, ct).ConfigureAwait(false);
        }
        finally
        {
            _lock.Release();
        }
    }

    private static string GetVendorKey(string vendor)
    {
        // Convert vendor id to display key (e.g., "bose-soundtouch" -> "SoundTouch")
        return vendor switch
        {
            "bose-soundtouch" => "SoundTouch",
            _ => vendor
        };
    }

    private static Device DtoToEntity(DeviceDto dto)
    {
        return new Device
        {
            Id = dto.Id,
            Vendor = dto.Vendor,
            Name = dto.Name,
            IpAddress = dto.IpAddress,
            Capabilities = new HashSet<string>(dto.Capabilities ?? Enumerable.Empty<string>()),
            DateTimeAdded = dto.DateTimeAdded ?? DateTime.UtcNow
        };
    }

    private static DeviceDto EntityToDto(Device entity)
    {
        return new DeviceDto
        {
            Id = entity.Id,
            Vendor = entity.Vendor,
            Name = entity.Name,
            IpAddress = entity.IpAddress,
            Capabilities = entity.Capabilities.Any() ? entity.Capabilities.ToList() : null,
            DateTimeAdded = entity.DateTimeAdded
        };
    }

    /// <summary>
    /// Root structure of devices.json file.
    /// </summary>
    private class DevicesFileRoot
    {
        public string? NetworkMask { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }

        private Dictionary<string, VendorGroup>? _vendorGroups;

        [JsonIgnore]
        public Dictionary<string, VendorGroup> VendorGroups
        {
            get
            {
                if (_vendorGroups != null)
                {
                    return _vendorGroups;
                }

                _vendorGroups = new Dictionary<string, VendorGroup>();
                if (ExtensionData == null)
                {
                    return _vendorGroups;
                }

                foreach (var (key, element) in ExtensionData)
                {
                    if (key == "NetworkMask")
                    {
                        continue;
                    }

                    try
                    {
                        var group = element.Deserialize<VendorGroup>(JsonOptions);
                        if (group != null)
                        {
                            _vendorGroups[key] = group;
                        }
                    }
                    catch
                    {
                        // Skip malformed vendor groups
                    }
                }

                return _vendorGroups;
            }
        }

        public JsonElement ToJsonElement()
        {
            var dict = new Dictionary<string, object?> { ["NetworkMask"] = NetworkMask };
            foreach (var (key, group) in VendorGroups)
            {
                dict[key] = group;
            }
            return JsonSerializer.SerializeToElement(dict, JsonOptions);
        }
    }

    private class VendorGroup
    {
        public List<DeviceDto> Devices { get; set; } = new();
    }

    private class DeviceDto
    {
        public required string Id { get; init; }
        public required string Vendor { get; init; }
        public required string Name { get; set; }
        public required string IpAddress { get; set; }
        public List<string>? Capabilities { get; set; }
        public DateTime? DateTimeAdded { get; set; }
    }
}

public class FileDeviceRepositoryOptions
{
    /// <summary>
    /// Path to the devices.json file.
    /// </summary>
    public string FilePath { get; set; } = "/data/devices.json";

    /// <summary>
    /// When enabled, the system watches for external changes to devices.json
    /// and notifies subscribers via <see cref="DeviceFileWatcher"/>.
    /// </summary>
    public bool EnableHotReload { get; set; } = true;
}
