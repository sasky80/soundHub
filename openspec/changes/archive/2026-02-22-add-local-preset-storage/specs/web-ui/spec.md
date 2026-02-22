## MODIFIED Requirements

### Requirement: Preset definition page for new presets
The system SHALL provide a form page for creating new presets with required and optional fields. When the source is `LOCAL_INTERNET_RADIO`, the form SHALL collect a stream URL instead of a raw location, and the backend SHALL manage the station file automatically.

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

#### Scenario: Display stream URL field for LOCAL_INTERNET_RADIO source
- **GIVEN** the user is on the new preset form
- **WHEN** the source field value is `LOCAL_INTERNET_RADIO`
- **THEN** the form SHALL hide the Location field
- **AND** display a Stream URL field (required, must start with `http://`)
- **AND** the station name SHALL be derived from the Name field

#### Scenario: Save new LOCAL_INTERNET_RADIO preset
- **GIVEN** the user is on the new preset form
- **AND** source is `LOCAL_INTERNET_RADIO`
- **AND** the user has filled in Name and Stream URL
- **WHEN** the user clicks save
- **THEN** the system sends a create preset request with `streamUrl` to the backend
- **AND** on success, navigates back to the device details page
- **AND** the new preset appears in the preset list

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

### Requirement: Edit existing preset
The system SHALL allow users to edit an existing preset's configuration. For `LOCAL_INTERNET_RADIO` presets, the form SHALL display the stream URL field pre-populated from the existing station file.

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

#### Scenario: Display edit form for LOCAL_INTERNET_RADIO preset
- **GIVEN** the user navigates to an existing preset's details page
- **AND** the preset source is `LOCAL_INTERNET_RADIO`
- **WHEN** the page loads
- **THEN** the form SHALL display the Stream URL field instead of Location
- **AND** the Stream URL field SHALL be pre-populated with the stream URL from the station file

#### Scenario: Update preset
- **GIVEN** the user is on the preset details page
- **AND** modifies preset values
- **WHEN** the user clicks the save button
- **THEN** the system sends an update preset request to the backend
- **AND** on success, navigates back to the device details page
- **AND** the updated preset values are reflected
