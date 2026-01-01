using SoundHub.Domain.Entities;

namespace SoundHub.Domain.Interfaces;

/// <summary>
/// Adapter interface for vendor-specific device control logic.
/// Each vendor (e.g., Bose SoundTouch, Sonos) implements this interface.
/// </summary>
/// <remarks>
/// The device adapter pattern decouples device control logic from the rest of the system,
/// enabling easy addition of new vendors without modifying existing code.
/// </remarks>
public interface IDeviceAdapter
{
    /// <summary>
    /// Gets the vendor identifier (e.g., "bose-soundtouch").
    /// </summary>
    string VendorId { get; }

    /// <summary>
    /// Gets the human-readable vendor name (e.g., "Bose SoundTouch").
    /// </summary>
    string VendorName { get; }

    /// <summary>
    /// Gets the default port for this vendor's devices.
    /// </summary>
    int DefaultPort { get; }

    /// <summary>
    /// Queries the capabilities supported by a specific device by probing the device.
    /// Base capabilities (power, volume) are always included.
    /// Additional capabilities are determined dynamically from the device.
    /// </summary>
    /// <param name="ipAddress">The device IP address.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Set of capability names (e.g., "power", "volume", "presets", "ping").</returns>
    Task<IReadOnlySet<string>> GetCapabilitiesAsync(string ipAddress, CancellationToken ct = default);

    /// <summary>
    /// Gets the current status of a device.
    /// </summary>
    Task<DeviceStatus> GetStatusAsync(string deviceId, CancellationToken ct = default);

    /// <summary>
    /// Gets detailed device information.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Device information including name, type, firmware version, and network info.</returns>
    Task<DeviceInfo> GetDeviceInfoAsync(string deviceId, CancellationToken ct = default);

    /// <summary>
    /// Gets the current playback state of a device.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Now playing information including source, track, artist, and play status.</returns>
    Task<NowPlayingInfo> GetNowPlayingAsync(string deviceId, CancellationToken ct = default);

    /// <summary>
    /// Gets the current volume level and mute state of a device.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Volume information including target volume, actual volume, and mute state.</returns>
    Task<VolumeInfo> GetVolumeAsync(string deviceId, CancellationToken ct = default);

    /// <summary>
    /// Sets the power state of a device.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="on">True to power on, false to power off.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SetPowerAsync(string deviceId, bool on, CancellationToken ct = default);

    /// <summary>
    /// Sets the volume level of a device.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="level">Volume level (0-100).</param>
    /// <param name="ct">Cancellation token.</param>
    Task SetVolumeAsync(string deviceId, int level, CancellationToken ct = default);

    /// <summary>
    /// Toggles the mute state of a device.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task MuteAsync(string deviceId, CancellationToken ct = default);

    /// <summary>
    /// Sends a key press to the device (press + release).
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="keyName">Name of the key (e.g., PLAY_PAUSE, VOLUME_UP).</param>
    /// <param name="ct">Cancellation token.</param>
    Task PressKeyAsync(string deviceId, string keyName, CancellationToken ct = default);

    /// <summary>
    /// Puts a device into Bluetooth pairing mode.
    /// </summary>
    Task EnterPairingModeAsync(string deviceId, CancellationToken ct = default);

    /// <summary>
    /// Lists all presets configured on a device.
    /// </summary>
    Task<IReadOnlyList<Preset>> ListPresetsAsync(string deviceId, CancellationToken ct = default);

    /// <summary>
    /// Stores or updates a preset on a device.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="preset">The preset to store.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The stored preset with any server-assigned values.</returns>
    Task<Preset> StorePresetAsync(string deviceId, Preset preset, CancellationToken ct = default);

    /// <summary>
    /// Removes a preset from a device.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="presetId">The preset slot ID (1-6 for SoundTouch).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the preset was removed, false if it didn't exist.</returns>
    Task<bool> RemovePresetAsync(string deviceId, int presetId, CancellationToken ct = default);

    /// <summary>
    /// Configures a new preset on a device.
    /// </summary>
    [Obsolete("Use StorePresetAsync instead")]
    Task<Preset> ConfigurePresetAsync(string deviceId, string name, string url, string type, int? position = null, CancellationToken ct = default);

    /// <summary>
    /// Plays a preset on a device.
    /// </summary>
    Task PlayPresetAsync(string deviceId, string presetId, CancellationToken ct = default);

    /// <summary>
    /// Pings a device to verify connectivity with audible feedback.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Ping result with reachability and latency.</returns>
    Task<PingResult> PingAsync(string deviceId, CancellationToken ct = default);

    /// <summary>
    /// Discovers devices of this vendor on the local network within the specified IP range.
    /// </summary>
    /// <param name="networkMask">Network mask in CIDR notation (e.g., "192.168.1.0/24"). If null, uses local subnet.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of discovered devices.</returns>
    Task<IReadOnlyList<Device>> DiscoverDevicesAsync(string? networkMask = null, CancellationToken ct = default);
}
