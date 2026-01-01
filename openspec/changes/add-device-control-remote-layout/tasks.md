# Implementation Tasks

## 1. Backend - API Endpoints

### 1.1 Key Press Endpoint
- [ ] 1.1.1 Add `POST /api/devices/{id}/key` endpoint to `DevicesController`
- [ ] 1.1.2 Implement `PressKeyAsync` method accepting key name and device ID
- [ ] 1.1.3 Map to `ISoundTouchAdapter.PressKey(deviceId, keyName)` method
- [ ] 1.1.4 Support key values: `AUX_INPUT`, `PREV_TRACK`, `NEXT_TRACK`, `PLAY_PAUSE`, `VOLUME_UP`, `VOLUME_DOWN`
- [ ] 1.1.5 Implement XML formatting for SoundTouch `/key` endpoint: `<key state="press" sender="Gabbo">KEY_NAME</key>` followed by release

### 1.2 Bluetooth Pairing Endpoint
- [ ] 1.2.1 Add `POST /api/devices/{id}/bluetooth/enter-pairing` endpoint to `DevicesController`
- [ ] 1.2.2 Implement `EnterBluetoothPairingAsync` method
- [ ] 1.2.3 Map to `ISoundTouchAdapter.EnterBluetoothPairing(deviceId)` method
- [ ] 1.2.4 Call SoundTouch `/enterBluetoothPairing` endpoint (GET request)
- [ ] 1.2.5 Return success/failure status

### 1.3 Capabilities Enhancement
- [ ] 1.3.1 Update `CapabilitiesDto` to include `bluetoothPairing` boolean property
- [ ] 1.3.2 Parse `<capability name="bluetoothPairing" ...>` from SoundTouch `/capabilities` response
- [ ] 1.3.3 Return capability info in device status or info endpoint

## 2. Frontend - Control Panel Redesign

### 2.1 Header Update
- [ ] 2.1.1 Change "Device Details" to "Control panel" in `device-details.component.html`
- [ ] 2.1.2 Update translation keys: `deviceDetails.title` → `controlPanel.title`

### 2.2 Volume Section Redesign
- [ ] 2.2.1 Replace "Volume" text label with volume icon (e.g., 🔊 or SVG icon)
- [ ] 2.2.2 Position icon on the left side of the slider
- [ ] 2.2.3 Update CSS to accommodate icon layout
- [ ] 2.2.4 Keep slider and mute button functionality unchanged

### 2.3 Remote Controller Button Grid
- [ ] 2.3.1 Create CSS grid layout for remote controller buttons
- [ ] 2.3.2 Design icon-based button components (no text labels on buttons)
- [ ] 2.3.3 Add aria-labels for accessibility
- [ ] 2.3.4 Implement responsive layout (adjust grid for mobile/tablet/desktop)

### 2.4 Playback Control Buttons
- [ ] 2.4.1 Add Previous Track button with icon (⏮ or SVG)
- [ ] 2.4.2 Add Next Track button with icon (⏭ or SVG)
- [ ] 2.4.3 Add Play/Pause toggle button with dynamic icon (▶ when paused, ⏸ when playing)
- [ ] 2.4.4 Wire up buttons to call `deviceService.pressKey(deviceId, keyName)` method
- [ ] 2.4.5 Play/Pause button always sends 'PLAY_PAUSE' key
- [ ] 2.4.6 Show loading state during key press operation

### 2.5 Volume Control Buttons
- [ ] 2.5.1 Add Volume Up button with icon (🔊+ or SVG)
- [ ] 2.5.2 Add Volume Down button with icon (🔉- or SVG)
- [ ] 2.5.3 Wire up to `deviceService.pressKey(deviceId, 'VOLUME_UP')` and `VOLUME_DOWN`
- [ ] 2.5.4 Optional: Add haptic feedback or visual confirmation

### 2.6 Source Control Buttons
- [ ] 2.6.1 Add AUX button with icon (AUX text or cable icon)
- [ ] 2.6.2 Wire up to `deviceService.pressKey(deviceId, 'AUX_INPUT')`
- [ ] 2.6.3 Add visual feedback when AUX source is active

### 2.7 Bluetooth Pairing Button
- [ ] 2.7.1 Check device capabilities for `bluetoothPairing` support
- [ ] 2.7.2 Conditionally render Bluetooth button if capability exists
- [ ] 2.7.3 Add Bluetooth icon button
- [ ] 2.7.4 Wire up to `deviceService.enterBluetoothPairing(deviceId)` method
- [ ] 2.7.5 Show pairing status/confirmation message
- [ ] 2.7.6 Add tooltip explaining Bluetooth pairing action

### 2.8 Power and Preset Sections
- [ ] 2.8.1 Convert power toggle to icon-based button (if not already)
- [ ] 2.8.2 Keep preset list section unchanged
- [ ] 2.8.3 Ensure remote layout integrates visually with preset section

## 3. Frontend - DeviceService Updates

### 3.1 Key Press Method
- [ ] 3.1.1 Add `pressKey(deviceId: string, keyName: string): Observable<void>` method
- [ ] 3.1.2 Call `POST /api/devices/{id}/key` with `{ key: keyName }` body
- [ ] 3.1.3 Handle errors gracefully (show toast notifications)

### 3.2 Bluetooth Pairing Method
- [ ] 3.2.1 Add `enterBluetoothPairing(deviceId: string): Observable<void>` method
- [ ] 3.2.2 Call `POST /api/devices/{id}/bluetooth/enter-pairing`
- [ ] 3.2.3 Return observable with success/failure

## 4. Testing

### 4.1 Backend Tests
- [ ] 4.1.1 Unit test `PressKeyAsync` endpoint with valid key names
- [ ] 4.1.2 Unit test error handling for invalid keys
- [ ] 4.1.3 Unit test `EnterBluetoothPairingAsync` endpoint
- [ ] 4.1.4 Mock SoundTouchAdapter responses

### 4.2 Frontend Tests
- [ ] 4.2.1 Component test: remote buttons rendered correctly
- [ ] 4.2.2 Component test: Bluetooth button hidden if capability not present
- [ ] 4.2.3 Component test: buttons disabled when device is off
- [ ] 4.2.4 Component test: key press triggers correct service method
- [ ] 4.2.5 Component test: Play/Pause button toggles icon based on play state
- [ ] 4.2.6 Component test: loading states displayed correctly

### 4.3 E2E Tests
- [ ] 4.3.1 E2E test: click playback buttons and verify API call
- [ ] 4.3.2 E2E test: click Bluetooth button (if visible)
- [ ] 4.3.3 E2E test: click AUX button and verify source switch
- [ ] 4.3.4 E2E test: volume buttons increment/decrement volume

## 5. Documentation

- [ ] 5.1 Update `device-configuration-guide.md` with remote controller usage instructions
- [ ] 5.2 Document key press API endpoint in `api-reference.md`
- [ ] 5.3 Document Bluetooth pairing endpoint in `api-reference.md`
- [ ] 5.4 Add screenshots of new remote controller layout

## 6. Design Assets

- [ ] 6.1 Create or source icon assets for all buttons
- [ ] 6.2 Ensure icons are accessible (SVG format, proper contrast)
- [ ] 6.3 Design loading/active states for buttons
- [ ] 6.4 Create responsive grid breakpoints for different screen sizes
