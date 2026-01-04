# Tasks: Update Preset Management with Edit Mode

## 1. Preset List Edit Mode

- [x] 1.1 Add `editMode` signal to `PresetListComponent`
- [x] 1.2 Add settings icon button in the top-right corner of preset list header
- [x] 1.3 Implement `toggleEditMode()` method to toggle edit mode state
- [x] 1.4 Hide the "Add preset" (+) button when not in edit mode
- [x] 1.5 Show "Add preset" (+) button only when in edit mode
- [x] 1.6 Add "Back" button at bottom-right of preset section (visible only in edit mode)
- [x] 1.7 Implement `exitEditMode()` method tied to the Back button
- [x] 1.8 Update preset item click behavior:
  - In normal mode: only play button works (no navigation on name click)
  - In edit mode: clicking preset name navigates to preset details page
- [x] 1.9 Style the settings icon and Back button appropriately
- [x] 1.10 Add visual indicator that edit mode is active (e.g., different background or icon state)

## 2. Preset Form Slot Dropdown Enhancement

- [x] 2.1 Pass existing presets data to `PresetFormComponent` via route resolver or service
- [x] 2.2 Update slot dropdown to show "Slot {X} - {PresetName}" for occupied slots
- [x] 2.3 Show "Slot {X} - Empty" for unoccupied slots
- [x] 2.4 Ensure current preset's slot shows its own name when editing

## 3. Delete Preset Flow

- [x] 3.1 Ensure delete confirmation dialog is shown before deletion (already exists, verify)
- [x] 3.2 After successful deletion, navigate back to device details page (already exists, verify)

## 4. Translations

- [x] 4.1 Add translation key for "Settings" icon aria-label
- [x] 4.2 Add translation key for "Back" button in edit mode
- [x] 4.3 Add translation key for "Slot {X} - Empty" pattern
- [x] 4.4 Add translations for both English and Polish

## 5. Testing

- [x] 5.1 Update E2E tests for edit mode flow
- [x] 5.2 Add unit tests for edit mode toggle
- [x] 5.3 Test slot dropdown shows correct preset names
- [x] 5.4 Test delete confirmation and navigation
