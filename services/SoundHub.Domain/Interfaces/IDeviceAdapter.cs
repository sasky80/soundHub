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
    /// Queries the capabilities supported by a specific device.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Set of capability names (e.g., "power", "volume", "presets").</returns>
    Task<IReadOnlySet<string>> GetCapabilitiesAsync(string deviceId, CancellationToken ct = default);

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
    /// Puts a device into Bluetooth pairing mode.
    /// </summary>
    Task EnterPairingModeAsync(string deviceId, CancellationToken ct = default);

    /// <summary>
    /// Lists all presets configured on a device.
    /// </summary>
    Task<IReadOnlyList<Preset>> ListPresetsAsync(string deviceId, CancellationToken ct = default);

    /// <summary>
    /// Configures a new preset on a device.
    /// </summary>
    Task<Preset> ConfigurePresetAsync(string deviceId, string name, string url, string type, int? position = null, CancellationToken ct = default);

    /// <summary>
    /// Plays a preset on a device.
    /// </summary>
    Task PlayPresetAsync(string deviceId, string presetId, CancellationToken ct = default);

    /// <summary>
    /// Discovers devices of this vendor on the local network.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of discovered devices.</returns>
    Task<IReadOnlyList<Device>> DiscoverDevicesAsync(CancellationToken ct = default);
}
