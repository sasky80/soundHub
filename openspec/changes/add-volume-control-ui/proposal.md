# Change: Add volume control UI to device details page

## Why
The device details page currently only supports power on/off. Users need to control device volume directly from the web UI without needing physical access to the device or a separate app. The volume control API endpoint already exists (`GET/POST /api/devices/{id}/volume`), but there is no frontend UI to interact with it.

## What Changes
- Add a **volume slider** control to the device details page for adjusting volume (0-100)
- Add a **mute button** to toggle mute state
- **Disable volume controls** when the device is powered off (standby)
- Fetch initial volume state from the existing volume API endpoint
- For **SoundTouch devices**, use the `/volume` endpoint which returns XML:
  ```xml
  <volume deviceID="...">
    <targetvolume>15</targetvolume>
    <actualvolume>15</actualvolume>
    <muteenabled>false</muteenabled>
  </volume>
  ```
- Send volume changes via `POST /api/devices/{id}/volume` with `{ "level": <0-100> }`
- Toggle mute via key press endpoint using `MUTE` key (already supported in SoundTouch API)

## Impact
- **Affected specs:**
  - `web-ui` (new volume control UI on device details page)
  - `api-device-control` (add mute toggle endpoint)
- **Affected code:**
  - `DeviceDetailsComponent` (add slider and mute button)
  - `DeviceService` (add volume get/set and mute methods)
  - `DevicesController` (add mute endpoint)
  - `SoundTouchAdapter` (already has volume, may need mute implementation)
  - Translation files (new i18n keys)
