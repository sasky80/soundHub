# Change: Add Preset Management from Device Details Page

## Why
Users need to manage presets (stations, playlists, radio streams) directly from the device details page. Currently, the system supports listing and playing presets but lacks the ability to view, add, edit, or remove them through the UI. For SoundTouch devices, presets are a core feature that allows users to configure up to 6 quick-access media sources.

## What Changes
- Add a presets section below the volume controls on the device details page
- Display configured presets as 64x64px play buttons with preset icons (if available) and preset names
- Play button activates the preset and powers on the device if currently off
- Add new preset form for manual preset creation (name, image URL, location, and SoundTouch-specific fields)
- Edit existing preset via dedicated preset details page
- Remove preset with confirmation
- **BREAKING**: None - this is additive functionality

## Impact
- Affected specs: `api-device-control`, `web-ui`
- Affected code:
  - Backend: New preset CRUD endpoints (store, remove), extend presets list response
  - Frontend: Preset list component, preset form component, preset details page, routing updates
  - Device adapters: SoundTouch adapter methods for preset store/remove
