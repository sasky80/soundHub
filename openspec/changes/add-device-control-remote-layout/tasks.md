# Implementation Tasks

## 1. Backend - API Endpoints

### 1.1 Key Press Endpoint
- [x] 1.1.1 Add `POST /api/devices/{id}/key` endpoint to `DevicesController`
- [x] 1.1.2 Implement `PressKeyAsync` method accepting key name and device ID
- [x] 1.1.3 Map to `ISoundTouchAdapter.PressKey(deviceId, keyName)` method
- [x] 1.1.4 Support key values: `AUX_INPUT`, `PREV_TRACK`, `NEXT_TRACK`, `PLAY_PAUSE`, `VOLUME_UP`, `VOLUME_DOWN`
- [x] 1.1.5 Implement XML formatting for SoundTouch `/key` endpoint: `<key state="press" sender="Gabbo">KEY_NAME</key>` followed by release

### 1.2 Bluetooth Pairing Endpoint
- [x] 1.2.1 Add `POST /api/devices/{id}/bluetooth/enter-pairing` endpoint to `DevicesController`
- [x] 1.2.2 Implement `EnterBluetoothPairingAsync` method
- [x] 1.2.3 Map to `ISoundTouchAdapter.EnterBluetoothPairing(deviceId)` method
- [x] 1.2.4 Call SoundTouch `/enterBluetoothPairing` endpoint (GET request)
- [x] 1.2.5 Return success/failure status

### 1.3 Capabilities Enhancement
- [x] 1.3.1 Update `CapabilitiesDto` to include `bluetoothPairing` boolean property
- [x] 1.3.2 Parse `<capability name="bluetoothPairing" ...>` from SoundTouch `/capabilities` response
- [x] 1.3.3 Return capability info in device status or info endpoint

## 2. Frontend - Control Panel Redesign

### 2.1 Header Update
- [x] 2.1.1 Change "Device Details" to "Control panel" in `device-details.component.html`
- [x] 2.1.2 Update translation keys: `deviceDetails.title` → `controlPanel.title`

### 2.2 Volume Section Redesign
- [x] 2.2.1 Replace "Volume" text label with volume icon (e.g., 🔊 or SVG icon)
- [x] 2.2.2 Position icon on the left side of the slider
- [x] 2.2.3 Update CSS to accommodate icon layout
- [x] 2.2.4 Keep slider and mute button functionality unchanged

### 2.3 Remote Controller Button Grid
- [x] 2.3.1 Create CSS grid layout for remote controller buttons
- [x] 2.3.2 Design icon-based button components (no text labels on buttons)
- [x] 2.3.3 Add aria-labels for accessibility
- [x] 2.3.4 Implement responsive layout (adjust grid for mobile/tablet/desktop)

### 2.4 Playback Control Buttons
- [x] 2.4.1 Add Previous Track button with icon (⏮ or SVG)
- [x] 2.4.2 Add Next Track button with icon (⏭ or SVG)
- [x] 2.4.3 Add Play/Pause toggle button with dynamic icon (▶ when paused, ⏸ when playing)
- [x] 2.4.4 Wire up buttons to call `deviceService.pressKey(deviceId, keyName)` method
- [x] 2.4.5 Play/Pause button always sends 'PLAY_PAUSE' key
- [x] 2.4.6 Show loading state during key press operation

### 2.5 Volume Control Buttons
- [x] 2.5.1 Add Volume Up button with icon (🔊+ or SVG)
- [x] 2.5.2 Add Volume Down button with icon (🔉- or SVG)
- [x] 2.5.3 Wire up to `deviceService.pressKey(deviceId, 'VOLUME_UP')` and `VOLUME_DOWN`
- [x] 2.5.4 Optional: Add haptic feedback or visual confirmation

### 2.6 Source Control Buttons
- [x] 2.6.1 Add AUX button with icon (AUX text or cable icon)
- [x] 2.6.2 Wire up to `deviceService.pressKey(deviceId, 'AUX_INPUT')`
- [x] 2.6.3 Add visual feedback when AUX source is active

### 2.7 Bluetooth Pairing Button
- [x] 2.7.1 Check device capabilities for `bluetoothPairing` support
- [x] 2.7.2 Conditionally render Bluetooth button if capability exists
- [x] 2.7.3 Add Bluetooth icon button
- [x] 2.7.4 Wire up to `deviceService.enterBluetoothPairing(deviceId)` method
- [x] 2.7.5 Show pairing status/confirmation message
- [x] 2.7.6 Add tooltip explaining Bluetooth pairing action

### 2.8 Power and Preset Sections
- [x] 2.8.1 Convert power toggle to icon-based button (if not already)
- [x] 2.8.2 Keep preset list section unchanged
- [x] 2.8.3 Ensure remote layout integrates visually with preset section

## 3. Frontend - DeviceService Updates

### 3.1 Key Press Method
- [x] 3.1.1 Add `pressKey(deviceId: string, keyName: string): Observable<void>` method
- [x] 3.1.2 Call `POST /api/devices/{id}/key` with `{ key: keyName }` body
- [x] 3.1.3 Handle errors gracefully (show toast notifications)

### 3.2 Bluetooth Pairing Method
- [x] 3.2.1 Add `enterBluetoothPairing(deviceId: string): Observable<void>` method
- [x] 3.2.2 Call `POST /api/devices/{id}/bluetooth/enter-pairing`
- [x] 3.2.3 Return observable with success/failure

## 4. Testing

### 4.1 Backend Tests
- [x] 4.1.1 Unit test `PressKeyAsync` endpoint with valid key names
- [x] 4.1.2 Unit test error handling for invalid keys
- [x] 4.1.3 Unit test `EnterBluetoothPairingAsync` endpoint
- [x] 4.1.4 Mock SoundTouchAdapter responses

### 4.2 Frontend Tests
- [x] 4.2.1 Component test: remote buttons rendered correctly
- [x] 4.2.2 Component test: Bluetooth button hidden if capability not present
- [x] 4.2.3 Component test: buttons disabled when device is off
- [x] 4.2.4 Component test: key press triggers correct service method
- [x] 4.2.5 Component test: Play/Pause button toggles icon based on play state
- [x] 4.2.6 Component test: loading states displayed correctly

### 4.3 E2E Tests
- [x] 4.3.1 E2E test: click playback buttons and verify API call
- [x] 4.3.2 E2E test: click Bluetooth button (if visible)
- [x] 4.3.3 E2E test: click AUX button and verify source switch
- [x] 4.3.4 E2E test: volume buttons increment/decrement volume

## 5. Documentation

- [x] 5.1 Update `device-configuration-guide.md` with remote controller usage instructions
- [x] 5.2 Document key press API endpoint in `api-reference.md`
- [x] 5.3 Document Bluetooth pairing endpoint in `api-reference.md`
- [x] 5.4 Add screenshots of new remote controller layout

## 6. Design Assets

- [x] 6.1 Create or source icon assets for all buttons
- [x] 6.2 Ensure icons are accessible (SVG format, proper contrast)
- [x] 6.3 Design loading/active states for buttons
- [x] 6.4 Create responsive grid breakpoints for different screen sizes
