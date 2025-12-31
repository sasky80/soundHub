# Tasks: Add Volume Control UI

## Phase 1: Backend - Mute Endpoint

- [ ] 1.1 Add `MuteAsync` method to `IDeviceAdapter` interface
  - Signature: `Task MuteAsync(string deviceId, CancellationToken ct = default)`
  - Verification: Interface compiles

- [ ] 1.2 Implement `MuteAsync` in `SoundTouchAdapter`
  - Send `POST /key` with `<key state="press" sender="Gabbo">MUTE</key>` and release
  - Verification: Unit test passes

- [ ] 1.3 Add `POST /api/devices/{id}/mute` endpoint to `DevicesController`
  - Returns 200 on success, 404 if device not found, 501 if not supported
  - Verification: Endpoint accessible via Swagger

## Phase 2: Frontend - Data Access Layer

- [ ] 2.1 Add `VolumeInfo` interface to `DeviceService`
  - Properties: `targetVolume: number`, `actualVolume: number`, `isMuted: boolean`
  - Verification: Interface compiles

- [ ] 2.2 Add `getVolume(id: string)` method to `DeviceService`
  - Returns `Observable<VolumeInfo>`
  - Calls `GET /api/devices/{id}/volume`
  - Verification: Service method exists

- [ ] 2.3 Add `setVolume(id: string, level: number)` method to `DeviceService`
  - Returns `Observable<void>`
  - Calls `POST /api/devices/{id}/volume` with `{ level }`
  - Verification: Service method exists

- [ ] 2.4 Add `toggleMute(id: string)` method to `DeviceService`
  - Returns `Observable<void>`
  - Calls `POST /api/devices/{id}/mute`
  - Verification: Service method exists

## Phase 3: Frontend - Device Details UI

- [ ] 3.1 Add volume-related signals to `DeviceDetailsComponent`
  - `volumeInfo = signal<VolumeInfo | null>(null)`
  - `volumeLoading = signal(false)`
  - `volumeValue = signal(0)` (for slider binding)
  - Verification: Component compiles

- [ ] 3.2 Fetch volume info on device load
  - Call `getVolume()` after `loadStatus()` succeeds
  - Update `volumeInfo` and `volumeValue` signals
  - Verification: Volume info loads on page open

- [ ] 3.3 Add volume slider to device details template
  - Range input (0-100) bound to `volumeValue`
  - Display current volume value
  - Disabled when `status.powerState === false`
  - Verification: Slider renders and is disabled when device off

- [ ] 3.4 Implement volume change handler
  - Debounce slider input (e.g., 300ms)
  - Call `setVolume()` on debounced value change
  - Update `volumeInfo` on success
  - Verification: Moving slider updates volume on device

- [ ] 3.5 Add mute button to device details template
  - Button with mute/unmute icon based on `volumeInfo.isMuted`
  - Disabled when `status.powerState === false`
  - Verification: Mute button renders with correct state

- [ ] 3.6 Implement mute toggle handler
  - Call `toggleMute()` on click
  - Refetch volume info on success to update mute state
  - Verification: Clicking mute toggles device mute state

- [ ] 3.7 Add volume control styling
  - Style slider to match design system
  - Add hover/focus states
  - Add disabled state styling
  - Verification: Controls visually match app design

## Phase 4: Internationalization

- [ ] 4.1 Add English translation keys
  - `deviceDetails.volume`: "Volume"
  - `deviceDetails.mute`: "Mute"
  - `deviceDetails.unmute`: "Unmute"
  - `deviceDetails.volumeDisabled`: "Volume control unavailable when device is off"
  - Verification: English translations display

- [ ] 4.2 Add Polish translation keys
  - Corresponding Polish translations for all new keys
  - Verification: Polish translations display

## Phase 5: Testing

- [ ] 5.1 Add unit tests for `DeviceDetailsComponent` volume controls
  - Test slider disabled when device off
  - Test mute button state reflects `isMuted`
  - Test volume change calls service
  - Verification: Unit tests pass

- [ ] 5.2 Add unit tests for new `DeviceService` methods
  - Test `getVolume` makes correct HTTP call
  - Test `setVolume` sends correct payload
  - Test `toggleMute` calls correct endpoint
  - Verification: Unit tests pass

- [ ] 5.3 Add E2E test for volume control flow
  - Navigate to device details
  - Verify slider present and interactive
  - Verification: E2E test passes

## Phase 6: Documentation

- [ ] 6.1 Update API reference documentation
  - Add `POST /api/devices/{id}/mute` endpoint documentation
  - Document request/response format and error codes
  - Verification: `docs/api-reference.md` includes mute endpoint

- [ ] 6.2 Update architecture documentation
  - Add volume slider to frontend library architecture diagram
  - Update Web UI Routes table if needed
  - Verification: `docs/architecture.md` reflects volume control feature

- [ ] 6.3 Update README features list
  - Add volume control to Features section
  - Verification: `README.md` mentions volume control capability

- [ ] 6.4 Update device configuration guide
  - Document volume control usage in device details page
  - Note disabled state when device is off
  - Verification: `docs/device-configuration-guide.md` covers volume control
