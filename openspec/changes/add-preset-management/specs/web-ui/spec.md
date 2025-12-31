## ADDED Requirements

### Requirement: Device details page displays preset list
The system SHALL display a list of configured presets on the device details page below the volume controls section.

#### Scenario: Display presets with icons
- **GIVEN** the user is on the device details page
- **AND** the device has presets configured
- **WHEN** the page loads
- **THEN** the UI displays a preset list section below volume controls
- **AND** each preset shows a 64x64px play button
- **AND** if the preset has an iconUrl, the icon is used as the button background
- **AND** the preset name is displayed alongside the play button

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

#### Scenario: Display add preset button
- **GIVEN** the user is on the device details page
- **WHEN** the preset section is displayed
- **THEN** a "+" button is visible to add a new preset

---

### Requirement: Play preset from device details page
The system SHALL allow users to play a preset by clicking the play button, powering on the device if necessary.

#### Scenario: Play preset when device is on
- **GIVEN** the user is on the device details page
- **AND** the device is powered on
- **AND** presets are displayed
- **WHEN** the user clicks a preset play button
- **THEN** the system sends a play preset request to the backend
- **AND** the device begins playing the preset content

#### Scenario: Play preset powers on device if off
- **GIVEN** the user is on the device details page
- **AND** the device is powered off (standby)
- **AND** presets are displayed
- **WHEN** the user clicks a preset play button
- **THEN** the system sends a play preset request to the backend
- **AND** the backend powers on the device first
- **AND** the device begins playing the preset content
- **AND** the UI reflects the new power state

#### Scenario: Show loading state while playing preset
- **GIVEN** the user clicks a preset play button
- **WHEN** the request is in progress
- **THEN** the play button shows a loading indicator
- **AND** the button is disabled until the request completes

---

### Requirement: Navigate to add new preset page
The system SHALL allow users to navigate to a new preset definition page.

#### Scenario: Navigate to new preset page
- **GIVEN** the user is on the device details page
- **WHEN** the user clicks the "+" button in the presets section
- **THEN** the system navigates to the new preset definition page

---

### Requirement: Preset definition page for new presets
The system SHALL provide a form page for creating new presets with required and optional fields.

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
The system SHALL allow users to edit an existing preset's configuration.

#### Scenario: Navigate to edit preset page
- **GIVEN** the user is on the device details page
- **AND** presets are displayed
- **WHEN** the user clicks on a preset (not the play button area)
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

#### Scenario: Update preset
- **GIVEN** the user is on the preset details page
- **AND** modifies preset values
- **WHEN** the user clicks the save button
- **THEN** the system sends an update preset request to the backend
- **AND** on success, navigates back to the device details page
- **AND** the updated preset values are reflected

---

### Requirement: Remove preset from device
The system SHALL allow users to remove a preset from the device.

#### Scenario: Remove preset with confirmation
- **GIVEN** the user is on the preset details page
- **WHEN** the user clicks the delete button
- **THEN** the system prompts for confirmation
- **AND** upon confirmation, sends a delete preset request to the backend
- **AND** on success, navigates back to the device details page
- **AND** the preset is removed from the list

#### Scenario: Cancel preset removal
- **GIVEN** the user is on the preset details page
- **AND** clicks the delete button
- **WHEN** the confirmation dialog appears
- **AND** the user cancels
- **THEN** the preset is not removed
- **AND** the user remains on the preset details page
