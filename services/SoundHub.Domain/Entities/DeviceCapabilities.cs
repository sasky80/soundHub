namespace SoundHub.Domain.Entities;

/// <summary>
/// Strongly typed view of device capabilities for API responses.
/// </summary>
public sealed class DeviceCapabilities
{
    public bool Power { get; init; }
    public bool Volume { get; init; }
    public bool Presets { get; init; }
    public bool Ping { get; init; }
    public bool BluetoothPairing { get; init; }

    public static DeviceCapabilities FromCapabilities(IReadOnlyCollection<string> capabilities)
    {
        ArgumentNullException.ThrowIfNull(capabilities);

        return new DeviceCapabilities
        {
            Power = capabilities.Contains("power"),
            Volume = capabilities.Contains("volume"),
            Presets = capabilities.Contains("presets"),
            Ping = capabilities.Contains("ping"),
            BluetoothPairing = capabilities.Contains("bluetoothPairing")
        };
    }
}
