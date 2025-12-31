# web-ui Specification Delta

## ADDED Requirements

### Requirement: Device details page supports volume control
The system SHALL provide volume control on the device details page, including a volume slider and mute button.

#### Scenario: Display volume slider with current level
- **GIVEN** the user is on the device details page
- **AND** the device is powered on
- **WHEN** the page loads
- **THEN** the UI displays a volume slider
- **AND** the slider shows the current volume level (0-100) fetched from the backend

#### Scenario: Adjust volume using slider
- **GIVEN** the user is on the device details page
- **AND** the device is powered on
- **WHEN** the user moves the volume slider to a new position
- **THEN** the UI sends a volume change request to the backend
- **AND** the slider reflects the new volume level when the request succeeds

#### Scenario: Display mute button with current state
- **GIVEN** the user is on the device details page
- **AND** the device is powered on
- **WHEN** the page loads
- **THEN** the UI displays a mute button
- **AND** the button shows the current mute state (muted or unmuted)

#### Scenario: Toggle mute
- **GIVEN** the user is on the device details page
- **AND** the device is powered on
- **WHEN** the user clicks the mute button
- **THEN** the UI sends a mute toggle request to the backend
- **AND** the mute button reflects the new state when the request succeeds

#### Scenario: Volume controls disabled when device is off
- **GIVEN** the user is on the device details page
- **AND** the device is powered off (standby)
- **WHEN** the page loads
- **THEN** the volume slider is disabled
- **AND** the mute button is disabled
- **AND** a visual indication shows controls are unavailable

#### Scenario: Volume slider shows actual volume on page load
- **GIVEN** the user is on the device details page
- **AND** the device is powered on with volume set to 42
- **WHEN** the page loads
- **THEN** the volume slider is positioned at 42
- **AND** the displayed volume value shows 42
