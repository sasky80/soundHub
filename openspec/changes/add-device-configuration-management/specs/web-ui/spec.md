# web-ui Specification Delta

## MODIFIED Requirements

### Requirement: Device configuration page lists configured devices
The system SHALL provide a device configuration page that lists configured devices with name, vendor, and ping button, and allows navigation to device details.

#### Scenario: Open device configuration page
- **GIVEN** the backend returns one or more configured devices
- **WHEN** the user opens the device configuration page
- **THEN** the system shows a list of configured devices
- **AND** each device displays its name and vendor
- **AND** devices with "ping" capability show a ping button

#### Scenario: Ping button visibility
- **GIVEN** a device has the "ping" capability in its capabilities list
- **WHEN** the device is displayed in the device list
- **THEN** the ping button is visible for that device

#### Scenario: Ping button hidden for devices without capability
- **GIVEN** a device does not have the "ping" capability
- **WHEN** the device is displayed in the device list
- **THEN** the ping button is not visible for that device

#### Scenario: Navigate to device details
- **GIVEN** the user is on the device configuration page
- **WHEN** the user selects a device
- **THEN** the system navigates to the device definition page for editing

## ADDED Requirements

### Requirement: Ping device from configuration page
The system SHALL allow users to verify device connectivity via the ping button.

#### Scenario: Ping device successfully
- **GIVEN** the user is on the device configuration page
- **AND** a device has the "ping" capability
- **WHEN** the user clicks the ping button
- **THEN** the system sends a ping request to the backend
- **AND** displays a success indicator when the device is reachable

#### Scenario: Ping device fails
- **GIVEN** the user is on the device configuration page
- **AND** a device has the "ping" capability
- **WHEN** the user clicks the ping button
- **AND** the device is not reachable
- **THEN** the system displays a failure indicator

### Requirement: Add new device
The system SHALL allow users to add a new device via a form.

#### Scenario: Open add device form
- **GIVEN** the user is on the device configuration page
- **WHEN** the user clicks the add (+) button
- **THEN** the system opens the new device definition page

#### Scenario: Add device with required fields
- **GIVEN** the user is on the add device form
- **WHEN** the user provides IP address or FQDN, vendor, name, and capabilities
- **AND** clicks the save button
- **THEN** the system creates the device via the backend API
- **AND** the new device appears in the device list

#### Scenario: Vendor selection populates from supported vendors
- **GIVEN** the user is on the add device form
- **WHEN** the user views the vendor dropdown
- **THEN** the dropdown is populated with vendors from `GET /api/vendors`

#### Scenario: Capabilities selection
- **GIVEN** the user is on the add device form
- **WHEN** the user selects capabilities
- **THEN** the user can choose from available capabilities via checkboxes

### Requirement: Edit device configuration
The system SHALL allow users to edit an existing device's configuration.

#### Scenario: Open device for editing
- **GIVEN** a device exists in the configuration
- **WHEN** the user selects the device from the list
- **THEN** the system opens the device definition page with current values populated

#### Scenario: Update device properties
- **GIVEN** the user is editing a device
- **WHEN** the user modifies IP address, vendor, name, or capabilities
- **AND** clicks the save button
- **THEN** the system updates the device via the backend API
- **AND** the device list reflects the changes

### Requirement: Remove device from configuration
The system SHALL allow users to remove a device from configuration.

#### Scenario: Delete device with confirmation
- **GIVEN** the user is viewing a device or on the device list
- **WHEN** the user initiates device removal
- **THEN** the system prompts for confirmation
- **AND** upon confirmation, removes the device via the backend API
- **AND** the device is removed from the device list

### Requirement: Configure network mask for discovery
The system SHALL allow users to specify a network mask for device discovery.

#### Scenario: View current network mask
- **GIVEN** the user is on the device configuration page
- **WHEN** the page loads
- **THEN** the current network mask is displayed in an input field (or empty if not set)

#### Scenario: Update network mask
- **GIVEN** the user is on the device configuration page
- **WHEN** the user enters a valid network mask (CIDR notation)
- **AND** the value is saved
- **THEN** the network mask is persisted via the backend API

#### Scenario: Invalid network mask validation
- **GIVEN** the user is on the device configuration page
- **WHEN** the user enters an invalid network mask format
- **THEN** the system displays a validation error

### Requirement: Launch device discovery
The system SHALL allow users to launch device discovery process.

#### Scenario: Start device discovery
- **GIVEN** the user is on the device configuration page
- **AND** a network mask is configured
- **WHEN** the user clicks the discover devices button
- **THEN** the system initiates discovery via the backend API
- **AND** shows a loading indicator during discovery

#### Scenario: Display discovered devices
- **GIVEN** device discovery has completed
- **WHEN** new devices are found
- **THEN** the newly discovered devices appear in the device list
- **AND** newly discovered devices are highlighted in the UI

#### Scenario: Discovery does not overwrite existing devices
- **GIVEN** devices are already configured
- **WHEN** the discovery process finds devices at the same IP addresses
- **THEN** existing device configurations remain unchanged
- **AND** only truly new devices are added to the list

#### Scenario: Discovery without network mask
- **GIVEN** no network mask is configured
- **WHEN** the user attempts to start discovery
- **THEN** the system displays an error indicating network mask is required
