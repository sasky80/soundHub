# api-device-control Specification Delta

## ADDED Requirements

### Requirement: Ping device endpoint
The system SHALL expose an endpoint to verify device connectivity with audible feedback.

#### Scenario: Device is reachable
- **GIVEN** a configured device exists with id `id`
- **AND** the device has the "ping" capability
- **WHEN** a client sends `GET /api/devices/{id}/ping`
- **THEN** the system invokes `/playNotification` on the device
- **AND** the device emits a double beep sound
- **AND** the system returns `{ "reachable": true, "latencyMs": <number> }`

#### Scenario: Device is not reachable
- **GIVEN** a configured device exists with id `id`
- **AND** the device is not responding on the network
- **WHEN** a client sends `GET /api/devices/{id}/ping`
- **THEN** the system returns `{ "reachable": false, "latencyMs": null }`

#### Scenario: Device not found
- **GIVEN** no configured device exists with id `id`
- **WHEN** a client sends `GET /api/devices/{id}/ping`
- **THEN** the system returns a 404 response

### Requirement: Device discovery endpoint
The system SHALL expose an endpoint to discover devices on the local network.

#### Scenario: Discover devices with configured network mask
- **GIVEN** a network mask is configured in the system
- **WHEN** a client sends `POST /api/devices/discover`
- **THEN** the system scans the IP range defined by the network mask
- **AND** probes each IP for known vendor device signatures
- **AND** returns discovered devices with counts

#### Scenario: Discover devices adds new devices only
- **GIVEN** some devices are already configured
- **AND** discovery finds devices including already-configured ones
- **WHEN** the discovery process completes
- **THEN** existing device configurations are preserved unchanged
- **AND** only newly discovered devices are added with `IsNewlyDiscovered: true`

#### Scenario: No network mask configured
- **GIVEN** no network mask is configured
- **WHEN** a client sends `POST /api/devices/discover`
- **THEN** the system returns a 400 response indicating network mask is required

### Requirement: Network mask configuration endpoints
The system SHALL expose endpoints to get and set the network mask for device discovery.

#### Scenario: Get network mask
- **WHEN** a client sends `GET /api/config/network-mask`
- **THEN** the system returns `{ "networkMask": "<configured-mask>" }` or `{ "networkMask": null }` if not set

#### Scenario: Set network mask with valid CIDR
- **GIVEN** a valid CIDR network mask (e.g., `192.168.1.0/24`)
- **WHEN** a client sends `PUT /api/config/network-mask` with `{ "networkMask": "192.168.1.0/24" }`
- **THEN** the system persists the network mask to devices.json
- **AND** returns 204 No Content

#### Scenario: Set network mask with invalid format
- **GIVEN** an invalid network mask format
- **WHEN** a client sends `PUT /api/config/network-mask`
- **THEN** the system returns a 400 response with validation error

### Requirement: Create device endpoint
The system SHALL expose an endpoint to create a new device configuration.

#### Scenario: Create device with required fields
- **GIVEN** a valid device payload with vendor, name, ipAddress, and capabilities
- **WHEN** a client sends `POST /api/devices`
- **THEN** the system creates the device with a generated ID
- **AND** the port is set automatically based on vendor
- **AND** returns 201 Created with the created device

#### Scenario: Create device with invalid IP address
- **GIVEN** a device payload with an invalid IP address format
- **WHEN** a client sends `POST /api/devices`
- **THEN** the system returns a 400 response with validation error

#### Scenario: Create SoundTouch device determines capabilities dynamically
- **GIVEN** a device payload with vendor `bose-soundtouch` and no capabilities specified
- **WHEN** a client sends `POST /api/devices`
- **THEN** the device is created with base capabilities `["power", "volume"]`
- **AND** the system queries the device's `/supportedUrls` endpoint
- **AND** adds "presets" if `/presets` is supported
- **AND** adds "bluetoothPairing" if `/enterBluetoothPairing` is supported
- **AND** adds "ping" if `/playNotification` is supported

### Requirement: Update device endpoint
The system SHALL expose an endpoint to update an existing device configuration.

#### Scenario: Update device properties
- **GIVEN** a configured device exists with id `id`
- **WHEN** a client sends `PUT /api/devices/{id}` with updated properties
- **THEN** the system updates the device configuration
- **AND** returns 200 OK with the updated device

#### Scenario: Update device not found
- **GIVEN** no configured device exists with id `id`
- **WHEN** a client sends `PUT /api/devices/{id}`
- **THEN** the system returns a 404 response

### Requirement: Delete device endpoint
The system SHALL expose an endpoint to remove a device from configuration.

#### Scenario: Delete existing device
- **GIVEN** a configured device exists with id `id`
- **WHEN** a client sends `DELETE /api/devices/{id}`
- **THEN** the system removes the device from configuration
- **AND** returns 204 No Content

#### Scenario: Delete device not found
- **GIVEN** no configured device exists with id `id`
- **WHEN** a client sends `DELETE /api/devices/{id}`
- **THEN** the system returns a 404 response

### Requirement: List vendors endpoint
The system SHALL expose an endpoint to list supported device vendors.

#### Scenario: Get vendor list
- **WHEN** a client sends `GET /api/vendors`
- **THEN** the system returns a list of supported vendors with id, name, and default port
