# Change: Update Preset Management with Edit Mode

## Why
Currently, the preset list always shows the "Add preset" button and clicking on a preset name plays the preset. There is no direct way to edit an existing preset from the preset list. We need a dedicated "Edit mode" that allows users to access preset configuration (add/edit/delete) while keeping the normal mode focused on playback functionality.

## What Changes
- Add a small "settings" icon in the top-right corner of the preset list section
- Clicking the settings icon toggles the preset list into "Edit mode"
- In Edit mode:
  - Clicking on a preset opens the preset definition page for viewing/editing
  - The "Add preset" (+) button is only visible in Edit mode (hidden in normal mode)
  - A "Back" button appears at the bottom-right corner to exit Edit mode
- On preset details page:
  - User can view/edit all preset fields
  - Delete button is available; clicking it shows a confirmation dialog
  - After deletion, user is redirected back to device details page
- Slot dropdown on add/edit preset page shows actual slot usage: "Slot {X} - {PresetName}" format

## Impact
- Affected specs: `web-ui`
- Affected code:
  - `frontend/libs/frontend/feature/src/lib/device-details/preset-list.component.ts`
  - `frontend/libs/frontend/feature/src/lib/device-details/preset-list.component.html`
  - `frontend/libs/frontend/feature/src/lib/device-details/preset-list.component.scss`
  - `frontend/libs/frontend/feature/src/lib/preset-form/preset-form.component.ts`
  - `frontend/libs/frontend/feature/src/lib/preset-form/preset-form.component.html`
  - Translation files (i18n)
