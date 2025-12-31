using System.Diagnostics;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoundHub.Domain.Entities;
using SoundHub.Domain.Interfaces;

namespace SoundHub.Infrastructure.Adapters;

/// <summary>
/// Adapter for Bose SoundTouch devices.
/// Communicates with devices via HTTP on port 8090 using the SoundTouch WebServices API.
/// </summary>
public class SoundTouchAdapter : IDeviceAdapter
{
    private readonly ILogger<SoundTouchAdapter> _logger;
    private readonly HttpClient _httpClient;
    private readonly IDeviceRepository _deviceRepository;
    private readonly SoundTouchAdapterOptions _options;

    /// <summary>
    /// Base capabilities that all SoundTouch devices support.
    /// </summary>
    private static readonly HashSet<string> BaseCapabilities = new() { "power", "volume" };

    public string VendorId => "bose-soundtouch";
    public string VendorName => "Bose SoundTouch";
    public int DefaultPort => 8090;

    public SoundTouchAdapter(
        ILogger<SoundTouchAdapter> logger,
        IHttpClientFactory httpClientFactory,
        IDeviceRepository deviceRepository,
        IOptions<SoundTouchAdapterOptions> options)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("SoundTouch");
        _deviceRepository = deviceRepository;
        _options = options.Value;
    }

    #region Private Helpers

    private async Task<Device> GetDeviceOrThrowAsync(string deviceId, CancellationToken ct)
    {
        var device = await _deviceRepository.GetDeviceAsync(deviceId, ct);
        if (device == null)
        {
            throw new KeyNotFoundException($"Device with ID {deviceId} not found");
        }
        return device;
    }

    private static string GetDeviceUrl(string ipAddress, int port, string endpoint)
    {
        return $"http://{ipAddress}:{port}{endpoint}";
    }

    private async Task<string> GetAsync(string url, CancellationToken ct)
    {
        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(ct);
    }

    private async Task PostXmlAsync(string url, string xmlContent, CancellationToken ct)
    {
        var content = new StringContent(xmlContent, Encoding.UTF8, "application/xml");
        var response = await _httpClient.PostAsync(url, content, ct);
        response.EnsureSuccessStatusCode();
    }

    private async Task SendKeyPressAsync(string ipAddress, int port, string keyName, CancellationToken ct)
    {
        var url = GetDeviceUrl(ipAddress, port, "/key");

        // Send key press
        var pressXml = $"<key state=\"press\" sender=\"Gabbo\">{keyName}</key>";
        await PostXmlAsync(url, pressXml, ct);

        // Small delay between press and release
        await Task.Delay(100, ct);

        // Send key release
        var releaseXml = $"<key state=\"release\" sender=\"Gabbo\">{keyName}</key>";
        await PostXmlAsync(url, releaseXml, ct);
    }

    #endregion

    #region IDeviceAdapter Implementation

    /// <inheritdoc />
    public async Task<IReadOnlySet<string>> GetCapabilitiesAsync(string ipAddress, CancellationToken ct = default)
    {
        var capabilities = new HashSet<string>(BaseCapabilities);

        try
        {
            var url = GetDeviceUrl(ipAddress, DefaultPort, "/supportedURLs");
            var response = await GetAsync(url, ct);
            var doc = XDocument.Parse(response);

            var supportedUrls = doc.Root?.Elements("supportedURL")
                .Select(e => e.Value)
                .ToHashSet() ?? new HashSet<string>();

            // Map supported URLs to capabilities
            if (supportedUrls.Contains("/presets"))
            {
                capabilities.Add("presets");
            }
            if (supportedUrls.Contains("/enterBluetoothPairing"))
            {
                capabilities.Add("bluetoothPairing");
            }
            if (supportedUrls.Contains("/playNotification"))
            {
                capabilities.Add("ping");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to query supported URLs for device at {IpAddress}, using base capabilities", ipAddress);
        }

        return capabilities;
    }

    public async Task<DeviceInfo> GetDeviceInfoAsync(string deviceId, CancellationToken ct = default)
    {
        var device = await GetDeviceOrThrowAsync(deviceId, ct);
        var url = GetDeviceUrl(device.IpAddress, DefaultPort, "/info");

        _logger.LogDebug("Getting device info from {Url}", url);

        try
        {
            var response = await GetAsync(url, ct);
            var doc = XDocument.Parse(response);

            var deviceIdAttr = doc.Root?.Attribute("deviceID")?.Value ?? deviceId;
            var name = doc.Root?.Element("name")?.Value ?? "Unknown";
            var type = doc.Root?.Element("type")?.Value ?? "Unknown";

            // Get network info
            var networkInfo = doc.Root?.Elements("networkInfo").FirstOrDefault();
            var macAddress = networkInfo?.Attribute("macAddress")?.Value;
            var ipAddress = networkInfo?.Element("ipAddress")?.Value ?? device.IpAddress;

            // Get software version from components
            var scmComponent = doc.Root?.Element("components")?
                .Elements("component")
                .FirstOrDefault(c => c.Element("componentCategory")?.Value == "SCM");
            var softwareVersion = scmComponent?.Element("softwareVersion")?.Value;

            return new DeviceInfo
            {
                DeviceId = deviceIdAttr,
                Name = name,
                Type = type,
                MacAddress = macAddress,
                IpAddress = ipAddress,
                SoftwareVersion = softwareVersion
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to get device info for {DeviceId}", deviceId);
            throw new InvalidOperationException($"Device {deviceId} is not reachable", ex);
        }
    }

    public async Task<NowPlayingInfo> GetNowPlayingAsync(string deviceId, CancellationToken ct = default)
    {
        var device = await GetDeviceOrThrowAsync(deviceId, ct);
        var url = GetDeviceUrl(device.IpAddress, DefaultPort, "/nowPlaying");

        _logger.LogDebug("Getting now playing from {Url}", url);

        try
        {
            var response = await GetAsync(url, ct);
            var doc = XDocument.Parse(response);

            var source = doc.Root?.Attribute("source")?.Value ?? "STANDBY";
            var track = doc.Root?.Element("track")?.Value;
            var artist = doc.Root?.Element("artist")?.Value;
            var album = doc.Root?.Element("album")?.Value;
            var stationName = doc.Root?.Element("stationName")?.Value;
            var playStatus = doc.Root?.Element("playStatus")?.Value;
            var artUrl = doc.Root?.Element("art")?.Value;

            return new NowPlayingInfo
            {
                Source = source,
                Track = track,
                Artist = artist,
                Album = album,
                StationName = stationName,
                PlayStatus = playStatus,
                ArtUrl = artUrl
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to get now playing for {DeviceId}", deviceId);
            throw new InvalidOperationException($"Device {deviceId} is not reachable", ex);
        }
    }

    public async Task<VolumeInfo> GetVolumeAsync(string deviceId, CancellationToken ct = default)
    {
        var device = await GetDeviceOrThrowAsync(deviceId, ct);
        var url = GetDeviceUrl(device.IpAddress, DefaultPort, "/volume");

        _logger.LogDebug("Getting volume from {Url}", url);

        try
        {
            var response = await GetAsync(url, ct);
            var doc = XDocument.Parse(response);

            var targetVolume = int.TryParse(doc.Root?.Element("targetvolume")?.Value, out var tv) ? tv : 0;
            var actualVolume = int.TryParse(doc.Root?.Element("actualvolume")?.Value, out var av) ? av : 0;
            var isMuted = doc.Root?.Element("muteenabled")?.Value?.ToLowerInvariant() == "true";

            return new VolumeInfo
            {
                TargetVolume = targetVolume,
                ActualVolume = actualVolume,
                IsMuted = isMuted
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to get volume for {DeviceId}", deviceId);
            throw new InvalidOperationException($"Device {deviceId} is not reachable", ex);
        }
    }

    public async Task<DeviceStatus> GetStatusAsync(string deviceId, CancellationToken ct = default)
    {
        var device = await GetDeviceOrThrowAsync(deviceId, ct);

        _logger.LogDebug("Getting status for device {DeviceId}", deviceId);

        try
        {
            // Get volume and now playing info in parallel
            var volumeTask = GetVolumeAsync(deviceId, ct);
            var nowPlayingTask = GetNowPlayingAsync(deviceId, ct);

            await Task.WhenAll(volumeTask, nowPlayingTask);

            var volume = await volumeTask;
            var nowPlaying = await nowPlayingTask;

            var isStandby = nowPlaying.Source == "STANDBY";

            return new DeviceStatus
            {
                DeviceId = deviceId,
                PowerState = !isStandby,
                Volume = volume.ActualVolume,
                CurrentSource = nowPlaying.Source,
                CurrentPreset = null, // Would need to correlate with presets
                IsOnline = true,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get status for device {DeviceId}, returning offline status", deviceId);
            return new DeviceStatus
            {
                DeviceId = deviceId,
                PowerState = false,
                Volume = 0,
                CurrentSource = null,
                CurrentPreset = null,
                IsOnline = false,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public async Task SetPowerAsync(string deviceId, bool on, CancellationToken ct = default)
    {
        var device = await GetDeviceOrThrowAsync(deviceId, ct);

        _logger.LogInformation("Setting power for device {DeviceId} to {On}", deviceId, on);

        try
        {
            if (on)
            {
                // Send POWER key to toggle on
                await SendKeyPressAsync(device.IpAddress, DefaultPort, "POWER", ct);
            }
            else
            {
                // Use standby endpoint to turn off
                var url = GetDeviceUrl(device.IpAddress, DefaultPort, "/standby");
                await GetAsync(url, ct);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to set power for {DeviceId}", deviceId);
            throw new InvalidOperationException($"Device {deviceId} is not reachable", ex);
        }
    }

    public async Task SetVolumeAsync(string deviceId, int level, CancellationToken ct = default)
    {
        if (level < 0 || level > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(level), "Volume must be between 0 and 100");
        }

        var device = await GetDeviceOrThrowAsync(deviceId, ct);
        var url = GetDeviceUrl(device.IpAddress, DefaultPort, "/volume");

        _logger.LogInformation("Setting volume for device {DeviceId} to {Level}", deviceId, level);

        try
        {
            var xml = $"<volume>{level}</volume>";
            await PostXmlAsync(url, xml, ct);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to set volume for {DeviceId}", deviceId);
            throw new InvalidOperationException($"Device {deviceId} is not reachable", ex);
        }
    }

    /// <inheritdoc />
    public async Task MuteAsync(string deviceId, CancellationToken ct = default)
    {
        var device = await GetDeviceOrThrowAsync(deviceId, ct);

        _logger.LogInformation("Toggling mute for device {DeviceId}", deviceId);

        try
        {
            await SendKeyPressAsync(device.IpAddress, DefaultPort, "MUTE", ct);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to toggle mute for {DeviceId}", deviceId);
            throw new InvalidOperationException($"Device {deviceId} is not reachable", ex);
        }
    }

    public async Task EnterPairingModeAsync(string deviceId, CancellationToken ct = default)
    {
        var device = await GetDeviceOrThrowAsync(deviceId, ct);
        var url = GetDeviceUrl(device.IpAddress, DefaultPort, "/enterBluetoothPairing");

        _logger.LogInformation("Entering Bluetooth pairing mode for device {DeviceId}", deviceId);

        try
        {
            await GetAsync(url, ct);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to enter pairing mode for {DeviceId}", deviceId);
            throw new InvalidOperationException($"Device {deviceId} is not reachable", ex);
        }
    }

    public async Task<IReadOnlyList<Preset>> ListPresetsAsync(string deviceId, CancellationToken ct = default)
    {
        var device = await GetDeviceOrThrowAsync(deviceId, ct);
        var url = GetDeviceUrl(device.IpAddress, DefaultPort, "/presets");

        _logger.LogDebug("Getting presets from {Url}", url);

        try
        {
            var response = await GetAsync(url, ct);
            var doc = XDocument.Parse(response);

            var presets = new List<Preset>();
            var presetElements = doc.Root?.Elements("preset");

            if (presetElements != null)
            {
                foreach (var presetElement in presetElements)
                {
                    var idStr = presetElement.Attribute("id")?.Value;
                    if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out var id)) continue;

                    var contentItem = presetElement.Element("ContentItem");
                    var itemName = contentItem?.Element("itemName")?.Value ?? "Unknown";
                    var source = contentItem?.Attribute("source")?.Value ?? "LOCAL_INTERNET_RADIO";
                    var location = contentItem?.Attribute("location")?.Value ?? "";
                    var type = contentItem?.Attribute("type")?.Value ?? "stationurl";
                    var containerArt = contentItem?.Element("containerArt")?.Value;
                    var isPresetable = contentItem?.Attribute("isPresetable")?.Value?.ToLowerInvariant() == "true";

                    presets.Add(new Preset
                    {
                        Id = id,
                        DeviceId = deviceId,
                        Name = itemName,
                        Location = location,
                        IconUrl = containerArt,
                        Type = type,
                        Source = source,
                        IsPresetable = isPresetable
                    });
                }
            }

            return presets;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to get presets for {DeviceId}", deviceId);
            throw new InvalidOperationException($"Device {deviceId} is not reachable", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Preset> StorePresetAsync(string deviceId, Preset preset, CancellationToken ct = default)
    {
        if (preset.Id < 1 || preset.Id > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(preset), "Preset ID must be between 1 and 6");
        }

        var device = await GetDeviceOrThrowAsync(deviceId, ct);
        var url = GetDeviceUrl(device.IpAddress, DefaultPort, "/storePreset");

        _logger.LogInformation("Storing preset {PresetId} on device {DeviceId}", preset.Id, deviceId);

        try
        {
            // Build the XML for /storePreset based on SoundTouch WebServices API
            var containerArtElement = !string.IsNullOrEmpty(preset.IconUrl)
                ? $"<containerArt>{System.Security.SecurityElement.Escape(preset.IconUrl)}</containerArt>"
                : "";

            var xml = $@"<preset id=""{preset.Id}"">
  <ContentItem source=""{System.Security.SecurityElement.Escape(preset.Source)}"" type=""{System.Security.SecurityElement.Escape(preset.Type)}"" location=""{System.Security.SecurityElement.Escape(preset.Location)}"" isPresetable=""true"">
    <itemName>{System.Security.SecurityElement.Escape(preset.Name)}</itemName>
    {containerArtElement}
  </ContentItem>
</preset>";

            await PostXmlAsync(url, xml, ct);

            // Return the preset with DeviceId set
            return new Preset
            {
                Id = preset.Id,
                DeviceId = deviceId,
                Name = preset.Name,
                Location = preset.Location,
                IconUrl = preset.IconUrl,
                Type = preset.Type,
                Source = preset.Source,
                IsPresetable = true
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to store preset for {DeviceId}", deviceId);
            throw new InvalidOperationException($"Device {deviceId} is not reachable", ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> RemovePresetAsync(string deviceId, int presetId, CancellationToken ct = default)
    {
        if (presetId < 1 || presetId > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(presetId), "Preset ID must be between 1 and 6");
        }

        var device = await GetDeviceOrThrowAsync(deviceId, ct);
        var url = GetDeviceUrl(device.IpAddress, DefaultPort, "/removePreset");

        _logger.LogInformation("Removing preset {PresetId} from device {DeviceId}", presetId, deviceId);

        try
        {
            // First check if the preset exists
            var presets = await ListPresetsAsync(deviceId, ct);
            var existingPreset = presets.FirstOrDefault(p => p.Id == presetId);
            if (existingPreset == null)
            {
                return false; // Preset doesn't exist
            }

            // Build the XML for /removePreset based on SoundTouch WebServices API
            var xml = $@"<preset id=""{presetId}""></preset>";

            await PostXmlAsync(url, xml, ct);
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to remove preset for {DeviceId}", deviceId);
            throw new InvalidOperationException($"Device {deviceId} is not reachable", ex);
        }
    }

    [Obsolete("Use StorePresetAsync instead")]
    public Task<Preset> ConfigurePresetAsync(string deviceId, string name, string url, string type, int? position = null, CancellationToken ct = default)
    {
        // SoundTouch doesn't support configuring presets via API in the same way
        // Presets are typically set through the device or app
        _logger.LogWarning("ConfigurePresetAsync is deprecated, use StorePresetAsync instead");

        var preset = new Preset
        {
            Id = position ?? 1,
            DeviceId = deviceId,
            Name = name,
            Location = url,
            Type = type
        };
        return Task.FromResult(preset);
    }

    public async Task PlayPresetAsync(string deviceId, string presetId, CancellationToken ct = default)
    {
        if (!int.TryParse(presetId, out var presetNumber) || presetNumber < 1 || presetNumber > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(presetId), "Preset ID must be a number between 1 and 6");
        }

        var device = await GetDeviceOrThrowAsync(deviceId, ct);

        _logger.LogInformation("Playing preset {PresetId} on device {DeviceId}", presetId, deviceId);

        try
        {
            var keyName = $"PRESET_{presetNumber}";
            await SendKeyPressAsync(device.IpAddress, DefaultPort, keyName, ct);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to play preset for {DeviceId}", deviceId);
            throw new InvalidOperationException($"Device {deviceId} is not reachable", ex);
        }
    }

    /// <inheritdoc />
    public async Task<PingResult> PingAsync(string deviceId, CancellationToken ct = default)
    {
        var device = await GetDeviceOrThrowAsync(deviceId, ct);
        var url = GetDeviceUrl(device.IpAddress, DefaultPort, "/playNotification");

        _logger.LogInformation("Pinging device {DeviceId} via playNotification", deviceId);

        var sw = Stopwatch.StartNew();
        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.PingTimeoutSeconds));

            await GetAsync(url, timeoutCts.Token);
            sw.Stop();

            _logger.LogInformation("Device {DeviceId} responded in {LatencyMs}ms", deviceId, sw.ElapsedMilliseconds);
            return new PingResult(true, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogWarning(ex, "Ping failed for device {DeviceId}", deviceId);
            return new PingResult(false, sw.ElapsedMilliseconds);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Device>> DiscoverDevicesAsync(string? networkMask = null, CancellationToken ct = default)
    {
        _logger.LogInformation("DiscoverDevicesAsync called for vendor {Vendor} with networkMask: {NetworkMask}", VendorId, networkMask ?? "auto-detect");

        var devices = new List<Device>();
        IEnumerable<string> ipRange;

        if (!string.IsNullOrEmpty(networkMask))
        {
            ipRange = ParseNetworkMask(networkMask);
        }
        else
        {
            // Get local IP to determine subnet
            var localIp = GetLocalIpAddress();
            if (localIp == null)
            {
                _logger.LogWarning("Could not determine local IP address for discovery");
                return devices;
            }

            var subnet = GetSubnet(localIp);
            _logger.LogInformation("Scanning subnet {Subnet} for SoundTouch devices", subnet);
            ipRange = Enumerable.Range(1, 254).Select(i => $"{subnet}.{i}");
        }

        // Scan all IPs in range
        var tasks = ipRange.Select(ip => TryDiscoverDeviceAtIpAsync(ip, ct)).ToList();
        var results = await Task.WhenAll(tasks);
        devices.AddRange(results.Where(d => d != null)!);

        _logger.LogInformation("Discovery complete: found {Count} SoundTouch devices", devices.Count);
        return devices;
    }

    #endregion

    #region Discovery Helpers

    private static IEnumerable<string> ParseNetworkMask(string networkMask)
    {
        // Parse CIDR notation (e.g., "192.168.1.0/24")
        var parts = networkMask.Split('/');
        if (parts.Length != 2 || !int.TryParse(parts[1], out var cidr) || cidr < 0 || cidr > 32)
        {
            throw new ArgumentException($"Invalid network mask format: {networkMask}", nameof(networkMask));
        }

        var ipParts = parts[0].Split('.');
        if (ipParts.Length != 4)
        {
            throw new ArgumentException($"Invalid IP address in network mask: {networkMask}", nameof(networkMask));
        }

        var baseIp = ipParts.Select(p => byte.Parse(p)).ToArray();
        var hostBits = 32 - cidr;
        var hostCount = (int)Math.Pow(2, hostBits);

        // Convert base IP to uint32
        var baseIpNum = ((uint)baseIp[0] << 24) | ((uint)baseIp[1] << 16) | ((uint)baseIp[2] << 8) | baseIp[3];

        // Generate all IPs in range (skip network and broadcast addresses for /24 and smaller)
        var start = cidr >= 24 ? 1 : 0;
        var end = cidr >= 24 ? hostCount - 1 : hostCount;

        for (int i = start; i < end; i++)
        {
            var ipNum = baseIpNum + (uint)i;
            yield return $"{(ipNum >> 24) & 0xFF}.{(ipNum >> 16) & 0xFF}.{(ipNum >> 8) & 0xFF}.{ipNum & 0xFF}";
        }
    }

    private async Task<Device?> TryDiscoverDeviceAtIpAsync(string ip, CancellationToken ct)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(2)); // Short timeout for discovery

            var url = $"http://{ip}:{DefaultPort}/info";
            var response = await _httpClient.GetAsync(url, cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cts.Token);
                var xml = XDocument.Parse(content);
                var name = xml.Root?.Element("name")?.Value ?? "Unknown Device";
                var deviceIdAttr = xml.Root?.Attribute("deviceID")?.Value ?? Guid.NewGuid().ToString();

                _logger.LogInformation("Discovered SoundTouch device at {Ip}: {Name}", ip, name);

                // Query capabilities for this device
                var capabilities = await GetCapabilitiesAsync(ip, cts.Token);

                return new Device
                {
                    Id = deviceIdAttr,
                    Vendor = VendorId,
                    Name = name,
                    IpAddress = ip,
                    Capabilities = new HashSet<string>(capabilities),
                    DateTimeAdded = DateTime.UtcNow
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

    #endregion
}
