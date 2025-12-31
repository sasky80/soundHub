# Change: Implement SoundTouch Extended Device Control Features

## Why
The SoundTouchAdapter currently contains mock implementations for device control. To make SoundHub functional with real Bose SoundTouch devices, the adapter must communicate with devices over HTTP using the SoundTouch WebServices API (port 8090). This change implements core device control features: volume management, device info/status retrieval, Bluetooth pairing mode, preset listing, and preset playback.

## What Changes
- Implement real HTTP calls to SoundTouch devices replacing mock methods in `SoundTouchAdapter`
- Add volume control: GET/POST `/volume` endpoint calls
- Add device info retrieval: GET `/info` and `/nowPlaying` endpoints
- Add Bluetooth pairing mode: GET `/enterBluetoothPairing` endpoint
- Add preset list retrieval: GET `/presets` endpoint
- Add preset playback: POST `/key` with `PRESET_1`–`PRESET_6` key presses
- Add power control: POST `/key` with `POWER` key and GET `/standby`
- Add new DTOs for parsing SoundTouch XML responses (`DeviceInfo`, `NowPlayingResponse`, etc.)
- Add new domain entity `DeviceInfo` to hold detailed device information

## Impact
- **Affected specs**: `api-device-control` (extended with new requirements)
- **Affected code**:
  - `SoundHub.Infrastructure/Adapters/SoundTouchAdapter.cs` – primary implementation
  - `SoundHub.Domain/Entities/` – new DTOs for device info
  - `SoundHub.Application/Services/DeviceService.cs` – may need updates for new operations
- **Dependencies**: Requires HttpClient configured for SoundTouch devices
- **No breaking changes**: Existing interface methods remain; implementations are replaced

## References
- [SoundTouch WebServices API](https://github.com/thlucas1/homeassistantcomponent_soundtouchplus/wiki/SoundTouch-WebServices-API)
- [Sample SoundTouchClient.cs](https://github.com/sasky80/SoundTouchMCP/blob/main/Services/SoundTouchClient.cs)
