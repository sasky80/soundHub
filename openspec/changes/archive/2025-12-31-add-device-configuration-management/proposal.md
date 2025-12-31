# Change: Add Device Configuration Management

## Why
Users need a comprehensive UI and API for managing device configuration. Currently, the system only displays devices but lacks full CRUD capabilities, device discovery, and ping functionality. This change provides a complete device management solution enabling users to add, edit, remove, discover, and verify connectivity of devices from the web interface.

## What Changes

### Web UI
- Add device configuration page with list of configured devices (name, vendor, ping button)
- Ping button visibility based on device "ping" capability
- Add new device form (IP/FQDN, vendor selection, name, type, capabilities)
- Device details/edit page for updating device properties
- Remove device functionality
- Network mask input for discovery range configuration
- Device discovery launcher with highlighting of newly discovered devices

### API
- Add `GET /api/devices/{id}/ping` endpoint to verify device connectivity
- Add `POST /api/devices/discover` endpoint to discover devices on network
- Add `GET /api/config/network-mask` and `PUT /api/config/network-mask` for discovery range
- Extend `POST /api/devices` to support adding devices with all required fields
- Extend `PUT /api/devices/{id}` to support updating device configuration
- Add `DELETE /api/devices/{id}` to remove devices

### Data Model
- Add `NetworkMask` attribute to devices.json root for discovery configuration
- Base capabilities for all devices: `["power", "volume"]`
- SoundTouch additional capabilities determined dynamically via `/supportedUrls`:
  - `/presets` → "presets"
  - `/enterBluetoothPairing` → "bluetoothPairing"
  - `/playNotification` → "ping"

### Infrastructure
- Implement ping operation in device adapters
- Implement IP range scanning for device discovery
- Ensure discovery process does not overwrite existing devices

## Impact
- **Affected specs**: `api-device-control`, `web-ui`
- **Affected code**:
  - `services/SoundHub.Api/Controllers/DevicesController.cs` – new endpoints
  - `services/SoundHub.Api/Controllers/ConfigController.cs` – new controller for config
  - `services/SoundHub.Domain/Interfaces/IDeviceAdapter.cs` – add ping method
  - `services/SoundHub.Infrastructure/Adapters/SoundTouchAdapter.cs` – ping and discovery implementation
  - `services/SoundHub.Infrastructure/Persistence/JsonDeviceRepository.cs` – network mask persistence
  - `frontend/libs/frontend/feature/` – device configuration feature module
  - `frontend/libs/frontend/data-access/` – device service updates
  - `data/devices.json` – schema update with NetworkMask
- **No breaking changes**: Existing API endpoints remain unchanged
