## ADDED Requirements

### Requirement: Landing page shows configured devices
The system SHALL provide a landing page that displays the names of all configured devices.

#### Scenario: Devices are configured
- **GIVEN** the backend returns a non-empty list from `GET /api/devices`
- **WHEN** the user opens the landing page
- **THEN** the page shows a list containing each device name

#### Scenario: No devices are configured
- **GIVEN** the backend returns an empty list from `GET /api/devices`
- **WHEN** the user opens the landing page
- **THEN** the page shows an empty state indicating no devices are configured

### Requirement: Landing page navigation
The system SHALL allow navigation from the landing page to Settings and to a selected device details page.

#### Scenario: Navigate to Settings
- **GIVEN** the user is on the landing page
- **WHEN** the user activates the Settings navigation control
- **THEN** the system navigates to the Settings page

#### Scenario: Navigate to Device details
- **GIVEN** the user is on the landing page
- **AND** at least one device is displayed
- **WHEN** the user selects a device
- **THEN** the system navigates to the Device details page for that device

### Requirement: Settings page language selection
The system SHALL provide a settings page that allows the user to select the UI language between English and Polish.

#### Scenario: Change language to Polish
- **GIVEN** the UI language is English
- **WHEN** the user selects Polish
- **THEN** the UI text updates to Polish
- **AND** the selection is persisted for future sessions

#### Scenario: Change language to English
- **GIVEN** the UI language is Polish
- **WHEN** the user selects English
- **THEN** the UI text updates to English
- **AND** the selection is persisted for future sessions

### Requirement: Settings page navigation to device configuration
The system SHALL provide navigation from the settings page to a device configuration page.

#### Scenario: Navigate to device configuration
- **GIVEN** the user is on the settings page
- **WHEN** the user activates the device configuration navigation control
- **THEN** the system navigates to the device configuration page

### Requirement: Device configuration page lists configured devices
The system SHALL provide a device configuration page that lists configured devices and allows navigation to device details.

#### Scenario: Open device configuration and navigate to device
- **GIVEN** the backend returns one or more configured devices
- **WHEN** the user opens the device configuration page
- **THEN** the system shows the list of configured device names
- **AND** the user can navigate to a selected device details page

### Requirement: Device details page supports power toggle
The system SHALL provide a device details page that allows the user to toggle power on/off.

#### Scenario: Turn device on
- **GIVEN** the user is on the device details page
- **WHEN** the user turns power on
- **THEN** the UI sends a power-on request to the backend
- **AND** the UI reflects the new power state when the request succeeds

#### Scenario: Turn device off
- **GIVEN** the user is on the device details page
- **WHEN** the user turns power off
- **THEN** the UI sends a power-off request to the backend
- **AND** the UI reflects the new power state when the request succeeds
