using SoundHub.Domain.Interfaces;

namespace SoundHub.Application.Services;

/// <summary>
/// Registry for device adapters. Maps vendor IDs to adapter implementations.
/// </summary>
public class DeviceAdapterRegistry
{
    private readonly Dictionary<string, IDeviceAdapter> _adapters = new(StringComparer.OrdinalIgnoreCase);

    public void RegisterAdapter(IDeviceAdapter adapter)
    {
        _adapters[adapter.VendorId] = adapter;
    }

    public IDeviceAdapter? GetAdapter(string vendorId)
    {
        _adapters.TryGetValue(vendorId, out var adapter);
        return adapter;
    }

    public IReadOnlyCollection<string> GetRegisteredVendors()
    {
        return _adapters.Keys;
    }
}
