# Change: Add Remote Controller Layout to Device Control Panel

## Why
The current Device Details page has a basic control interface with labeled buttons. Users need a more compact, intuitive remote control-style interface with icon-based buttons for common media operations (play/pause, track navigation, volume adjustment) and device-specific controls (Bluetooth pairing, AUX input). This will provide a familiar, space-efficient control panel that resembles physical remote controllers while leveraging the SoundTouch device capabilities.

## What Changes
- **Rename** "Device Details" header to "Control panel"
- **Compact control buttons section** by replacing text labels with icon-only buttons
- **Replace Volume label** with a volume icon positioned on the left side of the slider
- **Keep presets section unchanged** (maintain current implementation)
- **Add Bluetooth pairing button** (visible only if device has "bluetoothPairing" capability):
  - Triggers `/enterBluetoothPairing` endpoint
  - Icon-based button in remote layout
- **Add AUX input button**:
  - Switches device source to AUX via `/key` endpoint with `AUX_INPUT` key
  - Icon-based button in remote layout
- **Add playback control buttons**:
  - Previous Track (`PREV_TRACK` key)
  - Next Track (`NEXT_TRACK` key)
  - Play/Pause toggle (`PLAY_PAUSE` key)
- **Add volume control buttons**:
  - Volume Up (`VOLUME_UP` key)
  - Volume Down (`VOLUME_DOWN` key)
- **Remote controller layout**:
  - Buttons arranged in a logical grid layout resembling a physical remote control
  - Icon-based design for all buttons (no text labels on buttons)
  - Responsive layout that adapts to different screen sizes

## Impact
- **Affected specs:**
  - `web-ui` (redesign Device Details page control section with remote layout)
  - `api-device-control` (add new key press endpoints and Bluetooth pairing endpoint)
- **Affected code:**
  - `DeviceDetailsComponent` (refactor control section, add new buttons)
  - `DeviceService` (add methods for key operations and Bluetooth pairing)
  - `DevicesController` (add endpoints for key operations and Bluetooth pairing)
  - `SoundTouchAdapter` (implement key press and enterBluetoothPairing methods)
  - CSS/SCSS styling (create remote controller grid layout)
  - Translation files (update header text, add new button labels for accessibility)
  - Device capabilities detection (check for "bluetoothPairing" support)
