## MODIFIED Requirements

### Requirement: Device details page displays preset list
The system SHALL display a list of configured presets on the device details page below the volume controls section, with a settings icon to toggle Edit mode.

#### Scenario: Display presets with icons
- **GIVEN** the user is on the device details page
- **AND** the device has presets configured
- **WHEN** the page loads
- **THEN** the UI displays a preset list section below volume controls
- **AND** each preset shows a 64x64px play button
- **AND** if the preset has an iconUrl, the icon is used as the button background
- **AND** the preset name is displayed alongside the play button
- **AND** a settings icon is displayed in the top-right corner of the preset section header

#### Scenario: Display presets without icons
- **GIVEN** the user is on the device details page
- **AND** a preset does not have an iconUrl
- **WHEN** the preset is displayed
- **THEN** the play button shows a default play icon
- **AND** the preset name is displayed

#### Scenario: No presets configured
- **GIVEN** the user is on the device details page
- **AND** the device has no presets configured
- **WHEN** the page loads
- **THEN** the UI shows an empty state for presets
- **AND** displays a message indicating no presets are configured
- **AND** the settings icon is still displayed in the header

#### Scenario: Display add preset button only in edit mode
- **GIVEN** the user is on the device details page
- **AND** the preset section is NOT in edit mode
- **WHEN** the preset section is displayed
- **THEN** the "+" button to add a new preset is NOT visible

#### Scenario: Display add preset button in edit mode
- **GIVEN** the user is on the device details page
- **AND** the preset section is in edit mode
- **WHEN** the preset section is displayed
- **THEN** the "+" button is visible to add a new preset

---

### Requirement: Toggle preset list edit mode
The system SHALL allow users to toggle the preset list into Edit mode by clicking the settings icon.

#### Scenario: Enter edit mode via settings icon
- **GIVEN** the user is on the device details page
- **AND** the preset section is in normal mode
- **WHEN** the user clicks the settings icon in the preset section header
- **THEN** the preset section switches to edit mode
- **AND** a visual indicator shows that edit mode is active
- **AND** the "Back" button appears at the bottom-right corner of the preset section

#### Scenario: Exit edit mode via Back button
- **GIVEN** the user is on the device details page
- **AND** the preset section is in edit mode
- **WHEN** the user clicks the "Back" button at the bottom-right corner
- **THEN** the preset section switches back to normal mode
- **AND** the "Back" button is no longer visible
- **AND** the "+" add preset button is no longer visible

#### Scenario: Preset name click in normal mode
- **GIVEN** the user is on the device details page
- **AND** the preset section is in normal mode
- **WHEN** the user clicks on a preset name
- **THEN** the system plays the preset (existing behavior)

#### Scenario: Preset name click in edit mode
- **GIVEN** the user is on the device details page
- **AND** the preset section is in edit mode
- **WHEN** the user clicks on a preset name
- **THEN** the system navigates to the preset details page for that preset

#### Scenario: Pen icon displayed in edit mode
- **GIVEN** the user is on the device details page
- **AND** the preset section is in edit mode
- **WHEN** presets are displayed
- **THEN** each preset name shows a pen icon before the text
- **AND** the pen icon indicates that clicking will edit the preset

---

### Requirement: Navigate to add new preset page
The system SHALL allow users to navigate to a new preset definition page only when in edit mode.

#### Scenario: Navigate to new preset page in edit mode
- **GIVEN** the user is on the device details page
- **AND** the preset section is in edit mode
- **WHEN** the user clicks the "+" button in the presets section
- **THEN** the system navigates to the new preset definition page

---

### Requirement: Preset definition page for new presets
The system SHALL provide a form page for creating new presets with required and optional fields, displaying slot usage information in the slot dropdown.

#### Scenario: Display new preset form
- **GIVEN** the user navigates to the new preset definition page
- **WHEN** the page loads
- **THEN** the form displays fields for:
  - Preset slot (1-6, required)
  - Name (required)
  - Image URL (optional)
  - Location (required, the stream/content URL)
- **AND** for SoundTouch devices, additional fields:
  - Type (default: "stationurl")
  - Source (default: "LOCAL_INTERNET_RADIO")

#### Scenario: Slot dropdown shows usage information
- **GIVEN** the user is on the new preset form
- **AND** slots 1 and 3 have presets named "Radio Jazz" and "Morning News"
- **WHEN** the user views the slot dropdown
- **THEN** the dropdown displays:
  - "Slot 1 - Radio Jazz"
  - "Slot 2 - Empty"
  - "Slot 3 - Morning News"
  - "Slot 4 - Empty"
  - "Slot 5 - Empty"
  - "Slot 6 - Empty"

#### Scenario: Save new preset
- **GIVEN** the user is on the new preset form
- **AND** all required fields are filled
- **WHEN** the user clicks the save button
- **THEN** the system sends a create preset request to the backend
- **AND** on success, navigates back to the device details page
- **AND** the new preset appears in the preset list

#### Scenario: Cancel new preset creation
- **GIVEN** the user is on the new preset form
- **WHEN** the user clicks cancel or navigates back
- **THEN** the system navigates back to the device details page
- **AND** no preset is created

#### Scenario: Validation error on new preset
- **GIVEN** the user is on the new preset form
- **AND** required fields are missing
- **WHEN** the user attempts to save
- **THEN** the system displays validation errors
- **AND** does not submit the form

---

### Requirement: Edit existing preset
The system SHALL allow users to edit an existing preset's configuration when accessed via edit mode.

#### Scenario: Navigate to edit preset page from edit mode
- **GIVEN** the user is on the device details page
- **AND** the preset section is in edit mode
- **AND** presets are displayed
- **WHEN** the user clicks on a preset name
- **THEN** the system navigates to the preset details page with current values populated

#### Scenario: Display edit preset form
- **GIVEN** the user navigates to an existing preset's details page
- **WHEN** the page loads
- **THEN** the form displays current values for:
  - Preset slot (read-only)
  - Name
  - Image URL
  - Location
- **AND** for SoundTouch devices: Type and Source

#### Scenario: Slot dropdown shows usage in edit mode
- **GIVEN** the user is editing preset in slot 1 named "Radio Jazz"
- **AND** slot 3 has a preset named "Morning News"
- **WHEN** the user views the slot dropdown
- **THEN** the dropdown displays:
  - "Slot 1 - Radio Jazz" (current preset)
  - "Slot 2 - Empty"
  - "Slot 3 - Morning News"
  - "Slot 4 - Empty"
  - "Slot 5 - Empty"
  - "Slot 6 - Empty"

#### Scenario: Update preset
- **GIVEN** the user is on the preset details page
- **AND** modifies preset values
- **WHEN** the user clicks the save button
- **THEN** the system sends an update preset request to the backend
- **AND** on success, navigates back to the device details page
- **AND** the updated preset values are reflected

---

### Requirement: Remove preset from device
The system SHALL allow users to remove a preset from the device via the preset details page with confirmation.

#### Scenario: Delete button on preset details page
- **GIVEN** the user is on the preset details page (editing an existing preset)
- **WHEN** the page is displayed
- **THEN** a delete button is visible

#### Scenario: Remove preset with confirmation
- **GIVEN** the user is on the preset details page
- **WHEN** the user clicks the delete button
- **THEN** the system prompts for confirmation with a dialog asking if they are sure
- **AND** upon confirmation, sends a delete preset request to the backend
- **AND** on success, navigates back to the device details page
- **AND** the preset is removed from the preset list

#### Scenario: Cancel preset deletion
- **GIVEN** the user is on the preset details page
- **AND** the delete confirmation dialog is displayed
- **WHEN** the user cancels the deletion
- **THEN** the dialog closes
- **AND** the preset is not deleted
- **AND** the user remains on the preset details page
