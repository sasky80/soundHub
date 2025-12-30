using System.Net;
using System.Net.Http.Json;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SoundHub.Domain.Entities;
using SoundHub.Domain.Interfaces;

namespace SoundHub.Infrastructure.Adapters;

/// <summary>
/// Adapter for Bose SoundTouch devices.
/// This is a stub implementation with mock methods for initial scaffolding.
/// Real implementation will follow in a separate change.
/// </summary>
public class SoundTouchAdapter : IDeviceAdapter
{
    private readonly ILogger<SoundTouchAdapter> _logger;
    private readonly HttpClient _httpClient;

    public string VendorId => "bose-soundtouch";

    public SoundTouchAdapter(ILogger<SoundTouchAdapter> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("SoundTouch");
    }

    public Task<IReadOnlySet<string>> GetCapabilitiesAsync(string deviceId, CancellationToken ct = default)
    {
        // Mock capabilities - real implementation will query device
        var capabilities = new HashSet<string>
        {
            "power", "volume", "presets", "pairing", "status"
        };
        return Task.FromResult<IReadOnlySet<string>>(capabilities);
    }

    public Task<DeviceStatus> GetStatusAsync(string deviceId, CancellationToken ct = default)
    {
        _logger.LogInformation("GetStatusAsync called for device {DeviceId} (mock)", deviceId);
        
        // Mock status - real implementation will query device via HTTP
        var status = new DeviceStatus
        {
            DeviceId = deviceId,
            PowerState = true,
            Volume = 50,
            CurrentSource = "INTERNET_RADIO",
            IsOnline = true,
            Timestamp = DateTime.UtcNow
        };
        return Task.FromResult(status);
    }

    public Task SetPowerAsync(string deviceId, bool on, CancellationToken ct = default)
    {
        _logger.LogInformation("SetPowerAsync called for device {DeviceId}: {On} (mock)", deviceId, on);
        // Real implementation: POST to http://<ip>:8090/key with XML body <key state="press" sender="Gabbo">POWER</key>
        return Task.CompletedTask;
    }

    public Task SetVolumeAsync(string deviceId, int level, CancellationToken ct = default)
    {
        if (level < 0 || level > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(level), "Volume must be between 0 and 100");
        }

        _logger.LogInformation("SetVolumeAsync called for device {DeviceId}: {Level} (mock)", deviceId, level);
        // Real implementation: POST to http://<ip>:8090/volume with XML body <volume>{level}</volume>
        return Task.CompletedTask;
    }

    public Task EnterPairingModeAsync(string deviceId, CancellationToken ct = default)
    {
        _logger.LogInformation("EnterPairingModeAsync called for device {DeviceId} (mock)", deviceId);
        // Real implementation: POST to http://<ip>:8090/key with XML body <key state="press" sender="Gabbo">BLUETOOTH</key>
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Preset>> ListPresetsAsync(string deviceId, CancellationToken ct = default)
    {
        _logger.LogInformation("ListPresetsAsync called for device {DeviceId} (mock)", deviceId);
        
        // Mock presets - real implementation will query device
        var presets = new List<Preset>
        {
            new Preset
            {
                Id = "1",
                DeviceId = deviceId,
                Name = "BBC Radio 1",
                Url = "http://bbc.co.uk/radio1",
                Type = "InternetRadio",
                Position = 1
            }
        };
        return Task.FromResult<IReadOnlyList<Preset>>(presets);
    }

    public Task<Preset> ConfigurePresetAsync(string deviceId, string name, string url, string type, int? position = null, CancellationToken ct = default)
    {
        _logger.LogInformation("ConfigurePresetAsync called for device {DeviceId}: {Name} (mock)", deviceId, name);
        
        // Mock preset creation
        var preset = new Preset
        {
            Id = Guid.NewGuid().ToString(),
            DeviceId = deviceId,
            Name = name,
            Url = url,
            Type = type,
            Position = position
        };
        return Task.FromResult(preset);
    }

    public Task PlayPresetAsync(string deviceId, string presetId, CancellationToken ct = default)
    {
        _logger.LogInformation("PlayPresetAsync called for device {DeviceId}: preset {PresetId} (mock)", deviceId, presetId);
        // Real implementation: POST to http://<ip>:8090/select with XML preset content
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Device>> DiscoverDevicesAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("DiscoverDevicesAsync called for vendor {Vendor} (scanning network)", VendorId);
        
        var devices = new List<Device>();
        
        // Scan local network by probing common IP ranges
        // For SoundTouch, we check http://<ip>:8090/info or http://<ip>:8090/name
        var localIp = GetLocalIpAddress();
        if (localIp == null)
        {
            _logger.LogWarning("Could not determine local IP address for discovery");
            return devices;
        }

        var subnet = GetSubnet(localIp);
        _logger.LogInformation("Scanning subnet {Subnet} for SoundTouch devices", subnet);

        // Scan a reasonable range (e.g., .1 to .254)
        var tasks = new List<Task<Device?>>();
        for (int i = 1; i <= 254; i++)
        {
            var ip = $"{subnet}.{i}";
            tasks.Add(TryDiscoverDeviceAtIpAsync(ip, ct));
        }

        var results = await Task.WhenAll(tasks);
        devices.AddRange(results.Where(d => d != null)!);
        
        _logger.LogInformation("Discovery complete: found {Count} SoundTouch devices", devices.Count);
        return devices;
    }

    private async Task<Device?> TryDiscoverDeviceAtIpAsync(string ip, CancellationToken ct)
    {
        try
        {
            var response = await _httpClient.GetAsync($"http://{ip}:8090/info", ct);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                var xml = XDocument.Parse(content);
                var name = xml.Root?.Element("name")?.Value ?? "Unknown Device";
                
                _logger.LogInformation("Discovered SoundTouch device at {Ip}: {Name}", ip, name);
                
                return new Device
                {
                    Id = Guid.NewGuid().ToString(),
                    Vendor = VendorId,
                    Name = name,
                    IpAddress = ip,
                    Port = 8090,
                    IsOnline = true,
                    PowerState = false,
                    Volume = 0,
                    LastSeen = DateTime.UtcNow
                };
            }
        }
        catch
        {
            // Ignore errors - host is not a SoundTouch device or is unreachable
        }

        return null;
    }

    private static string? GetLocalIpAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
        }
        catch
        {
            // Ignore
        }

        return null;
    }

    private static string GetSubnet(string ip)
    {
        var parts = ip.Split('.');
        return $"{parts[0]}.{parts[1]}.{parts[2]}";
    }
}
