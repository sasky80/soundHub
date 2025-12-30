using System.Text.Json;
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

    private async Task<Dictionary<string, List<DeviceDto>>> LoadFromFileAsync(CancellationToken ct)
    {
        try
        {
            var json = await File.ReadAllTextAsync(_filePath, ct).ConfigureAwait(false);
            var data = JsonSerializer.Deserialize<Dictionary<string, VendorGroup>>(json);
            
            var result = new Dictionary<string, List<DeviceDto>>();
            if (data != null)
            {
                foreach (var (vendor, group) in data)
                {
                    result[vendor] = group.Devices;
                }
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load devices from {FilePath}", _filePath);
            return new Dictionary<string, List<DeviceDto>>();
        }
    }

    private async Task SaveToFileAsync(Dictionary<string, List<DeviceDto>> data, CancellationToken ct)
    {
        var grouped = data.ToDictionary(
            kvp => kvp.Key,
            kvp => new VendorGroup { Devices = kvp.Value }
        );

        var json = JsonSerializer.Serialize(grouped, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_filePath, json, ct).ConfigureAwait(false);
    }

    public async Task<Device?> GetDeviceAsync(string id, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var data = await LoadFromFileAsync(ct).ConfigureAwait(false);
            foreach (var devices in data.Values)
            {
                var dto = devices.FirstOrDefault(d => d.Id == id);
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
            return data.Values
                .SelectMany(devices => devices)
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
            if (!data.ContainsKey(device.Vendor))
            {
                data[device.Vendor] = new List<DeviceDto>();
            }

            data[device.Vendor].Add(EntityToDto(device));
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
            foreach (var (vendor, devices) in data)
            {
                var index = devices.FindIndex(d => d.Id == device.Id);
                if (index >= 0)
                {
                    devices[index] = EntityToDto(device);
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
            foreach (var (vendor, devices) in data)
            {
                var removed = devices.RemoveAll(d => d.Id == id) > 0;
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
            if (data.TryGetValue(vendor, out var devices))
            {
                return devices.Select(DtoToEntity).ToList();
            }
            return Array.Empty<Device>();
        }
        finally
        {
            _lock.Release();
        }
    }

    private static Device DtoToEntity(DeviceDto dto)
    {
        return new Device
        {
            Id = dto.Id,
            Vendor = dto.Vendor,
            Name = dto.Name,
            IpAddress = dto.IpAddress,
            Port = dto.Port,
            IsOnline = dto.IsOnline,
            PowerState = dto.PowerState,
            Volume = dto.Volume,
            LastSeen = dto.LastSeen,
            Capabilities = new HashSet<string>(dto.Capabilities ?? Enumerable.Empty<string>())
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
            Port = entity.Port,
            IsOnline = entity.IsOnline,
            PowerState = entity.PowerState,
            Volume = entity.Volume,
            LastSeen = entity.LastSeen,
            Capabilities = entity.Capabilities.Any() ? entity.Capabilities.ToList() : null
        };
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
        public int Port { get; set; }
        public bool IsOnline { get; set; }
        public bool PowerState { get; set; }
        public int Volume { get; set; }
        public DateTime LastSeen { get; set; }
        public List<string>? Capabilities { get; set; }
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
