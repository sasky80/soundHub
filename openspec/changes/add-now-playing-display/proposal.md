# Change: Add Now Playing Display with Auto-Scrolling LCD

## Why
Users need real-time feedback about what's currently playing on their audio device. The control panel should display the current station, artist, and track information in a visually appealing LCD-style display. Additionally, the Bluetooth and AUX buttons should indicate active state based on the current source rather than just showing action feedback messages.

## What Changes
- **Add Now Playing LCD display** in device info section (under device vendor):
  - LCD-style single-line display with retro appearance
  - Shows text in format: `{StationName}: {Artist}, {Track}`
  - Auto-scrolls text from right to left for long content
  - Only visible when device is powered on
  - Positioned between device info and control section

- **Implement device status polling**:
  - Poll device status every 10 seconds while device is on
  - Includes power state, current source, and now playing info
  - Stops polling when device is off or component is destroyed

- **Modify Bluetooth button behavior**:
  - Remove "Bluetooth pairing started" message display
  - Change to two-state toggle button (like AUX)
  - Active state when `currentSource === 'BLUETOOTH'`
  - Keep pairing action functionality on click

- **Update AUX button** to properly reflect source state:
  - Already has two-state implementation (`activeSource()`)
  - Ensure state is derived from polled `currentSource`
  - Active when `currentSource === 'AUX'`

## Impact
- **Affected specs:**
  - `web-ui` (add Now Playing display, update button states, add polling)
  - `api-device-control` (extend status response with now playing data)

- **Affected code:**
  - `DeviceDetailsComponent` (add LCD display, polling logic, button state updates)
  - `DeviceService` (update DeviceStatus interface, polling method)
  - Device details component styles (LCD display styling, scrolling animation)
  - Translation files (Now Playing section labels)
  - `SoundTouchAdapter` (ensure nowPlaying data is included in status)

- **New UI components:**
  - LCD display component or template section with CSS animation
