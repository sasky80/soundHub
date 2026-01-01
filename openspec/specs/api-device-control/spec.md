# api-device-control Specification

## Purpose
TBD - created by archiving change add-modern-landing-page. Update Purpose after archive.
## Requirements
### Requirement: Power control endpoint
The system SHALL expose an endpoint to toggle a device power state.

#### Scenario: Turn device on
- **GIVEN** a configured device exists with id `id`
- **WHEN** a client sends `POST /api/devices/{id}/power` with body `{ "on": true }`
- **THEN** the system invokes the resolved vendor adapter to set device power on
- **AND** the endpoint returns a success response

#### Scenario: Turn device off
- **GIVEN** a configured device exists with id `id`
- **WHEN** a client sends `POST /api/devices/{id}/power` with body `{ "on": false }`
- **THEN** the system invokes the resolved vendor adapter to set device power off
- **AND** the endpoint returns a success response

#### Scenario: Device not found
- **GIVEN** no configured device exists with id `id`
- **WHEN** a client sends `POST /api/devices/{id}/power`
- **THEN** the system returns a 404 response

#### Scenario: Operation not supported
- **GIVEN** the device vendor adapter does not support power control
- **WHEN** a client sends `POST /api/devices/{id}/power`
- **THEN** the system returns a 501 response with an explanatory error

### Requirement: Volume control endpoint
The system SHALL expose an endpoint to get and set a device's volume level.

#### Scenario: Get current volume
- **GIVEN** a configured device exists with id `{id}`
- **WHEN** a client sends `GET /api/devices/{id}/volume`
- **THEN** the system queries the device adapter for current volume
- **AND** returns `{ "targetVolume": 50, "actualVolume": 50, "isMuted": false }`

#### Scenario: Set volume level
- **GIVEN** a configured device exists with id `{id}`
- **WHEN** a client sends `POST /api/devices/{id}/volume` with body `{ "level": 75 }`
- **THEN** the system invokes the device adapter to set volume to 75
- **AND** returns a success response

#### Scenario: Invalid volume level
- **GIVEN** a configured device exists with id `{id}`
- **WHEN** a client sends `POST /api/devices/{id}/volume` with body `{ "level": 150 }`
- **THEN** the system returns a 400 response with validation error

#### Scenario: Volume control not supported
- **GIVEN** the device vendor adapter does not support volume control
- **WHEN** a client sends `POST /api/devices/{id}/volume`
- **THEN** the system returns a 501 response with an explanatory error

---

### Requirement: Device info endpoint
The system SHALL expose an endpoint to retrieve detailed device information.

#### Scenario: Get device info
- **GIVEN** a configured device exists with id `{id}`
- **WHEN** a client sends `GET /api/devices/{id}/info`
- **THEN** the system queries the device adapter for device information
- **AND** returns device details including name, type, firmware version, and network info

#### Scenario: Device offline
- **GIVEN** a configured device exists with id `{id}` but is unreachable
- **WHEN** a client sends `GET /api/devices/{id}/info`
- **THEN** the system returns a 503 response indicating device is offline

---

### Requirement: Now playing status endpoint
The system SHALL expose an endpoint to retrieve the current playback status.

#### Scenario: Get now playing status
- **GIVEN** a configured device exists with id `{id}` and is playing media
- **WHEN** a client sends `GET /api/devices/{id}/nowPlaying`
- **THEN** the system queries the device adapter for now playing information
- **AND** returns playback details including source, track, artist, and play status

#### Scenario: Device in standby
- **GIVEN** a configured device exists with id `{id}` and is in standby
- **WHEN** a client sends `GET /api/devices/{id}/nowPlaying`
- **THEN** the system returns a response indicating source is `STANDBY`

---

### Requirement: Bluetooth pairing mode endpoint
The system SHALL expose an endpoint to put a device into Bluetooth pairing mode.

#### Scenario: Enter Bluetooth pairing mode
- **GIVEN** a configured device exists with id `{id}` that supports Bluetooth
- **WHEN** a client sends `POST /api/devices/{id}/bluetooth/pairing`
- **THEN** the system invokes the device adapter to enter pairing mode
- **AND** the device becomes discoverable for Bluetooth pairing
- **AND** returns a success response

#### Scenario: Bluetooth not supported
- **GIVEN** a device exists that does not support Bluetooth
- **WHEN** a client sends `POST /api/devices/{id}/bluetooth/pairing`
- **THEN** the system returns a 501 response with an explanatory error

---

### Requirement: Presets list endpoint
The system SHALL expose an endpoint to list all configured presets on a device.

#### Scenario: Get presets list
- **GIVEN** a configured device exists with id `{id}` with presets configured
- **WHEN** a client sends `GET /api/devices/{id}/presets`
- **THEN** the system queries the device adapter for preset list
- **AND** returns an array of presets with id, name, and source information

#### Scenario: No presets configured
- **GIVEN** a configured device exists with id `{id}` with no presets
- **WHEN** a client sends `GET /api/devices/{id}/presets`
- **THEN** the system returns an empty array

---

### Requirement: Play preset endpoint
The system SHALL expose an endpoint to activate a preset on a device, powering on the device if necessary.

#### Scenario: Play preset by number
- **GIVEN** a configured device exists with id `{id}` with preset 1 configured
- **WHEN** a client sends `POST /api/devices/{id}/presets/1/play`
- **THEN** the system invokes the device adapter to play preset 1
- **AND** the device begins playback of the preset content
- **AND** returns a success response

#### Scenario: Play preset powers on device if off
- **GIVEN** a configured device exists with id `{id}` with preset 2 configured
- **AND** the device is in standby mode
- **WHEN** a client sends `POST /api/devices/{id}/presets/2/play`
- **THEN** the system first invokes the device adapter to power on the device
- **AND** then invokes the device adapter to play preset 2
- **AND** the device begins playback
- **AND** returns a success response

#### Scenario: Invalid preset number
- **GIVEN** a configured device exists with id `{id}`
- **WHEN** a client sends `POST /api/devices/{id}/presets/7/play`
- **THEN** the system returns a 400 response indicating preset number must be 1-6

#### Scenario: Preset not configured
- **GIVEN** a configured device exists with id `{id}` but preset 3 is empty
- **WHEN** a client sends `POST /api/devices/{id}/presets/3/play`
- **THEN** the system returns a 404 response indicating preset is not configured

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

### Requirement: Mute toggle endpoint
The system SHALL expose an endpoint to toggle a device's mute state.

#### Scenario: Toggle mute on
- **GIVEN** a configured device exists with id `{id}`
- **AND** the device is not muted
- **WHEN** a client sends `POST /api/devices/{id}/mute`
- **THEN** the system invokes the device adapter to toggle mute
- **AND** the device becomes muted
- **AND** returns a success response

#### Scenario: Toggle mute off
- **GIVEN** a configured device exists with id `{id}`
- **AND** the device is currently muted
- **WHEN** a client sends `POST /api/devices/{id}/mute`
- **THEN** the system invokes the device adapter to toggle mute
- **AND** the device becomes unmuted
- **AND** returns a success response

#### Scenario: Device not found
- **GIVEN** no configured device exists with id `{id}`
- **WHEN** a client sends `POST /api/devices/{id}/mute`
- **THEN** the system returns a 404 response

#### Scenario: Mute not supported
- **GIVEN** the device vendor adapter does not support mute control
- **WHEN** a client sends `POST /api/devices/{id}/mute`
- **THEN** the system returns a 501 response with an explanatory error

### Requirement: Store preset endpoint
The system SHALL expose an endpoint to create or update a preset on a device.

#### Scenario: Create new preset
- **GIVEN** a configured device exists with id `{id}`
- **AND** the device supports presets
- **WHEN** a client sends `POST /api/devices/{id}/presets` with body:
  ```json
  {
    "id": 3,
    "name": "My Radio Station",
    "location": "http://stream.example.com/radio",
    "iconUrl": "http://example.com/icon.png",
    "type": "stationurl",
    "source": "LOCAL_INTERNET_RADIO"
  }
  ```
- **THEN** the system invokes the device adapter to store the preset
- **AND** returns 201 Created with the stored preset details

#### Scenario: Update existing preset
- **GIVEN** a configured device exists with id `{id}`
- **AND** preset slot 2 already has a preset configured
- **WHEN** a client sends `POST /api/devices/{id}/presets` with id 2 and updated values
- **THEN** the system invokes the device adapter to update preset slot 2
- **AND** returns 200 OK with the updated preset details

#### Scenario: Invalid preset ID
- **GIVEN** a configured device exists with id `{id}`
- **WHEN** a client sends `POST /api/devices/{id}/presets` with id 7
- **THEN** the system returns 400 Bad Request indicating preset ID must be 1-6

#### Scenario: Device does not support presets
- **GIVEN** a configured device exists with id `{id}`
- **AND** the device does not have "presets" capability
- **WHEN** a client sends `POST /api/devices/{id}/presets`
- **THEN** the system returns 501 Not Implemented

#### Scenario: SoundTouch preset requires type and source
- **GIVEN** a SoundTouch device exists with id `{id}`
- **WHEN** a client sends `POST /api/devices/{id}/presets` without type or source
- **THEN** the system uses default values: type="stationurl", source="LOCAL_INTERNET_RADIO"

---

### Requirement: Remove preset endpoint
The system SHALL expose an endpoint to remove a preset from a device.

#### Scenario: Remove existing preset
- **GIVEN** a configured device exists with id `{id}`
- **AND** preset slot 3 has a preset configured
- **WHEN** a client sends `DELETE /api/devices/{id}/presets/3`
- **THEN** the system invokes the device adapter to remove preset 3
- **AND** returns 204 No Content

#### Scenario: Remove preset that does not exist
- **GIVEN** a configured device exists with id `{id}`
- **AND** preset slot 5 is empty
- **WHEN** a client sends `DELETE /api/devices/{id}/presets/5`
- **THEN** the system returns 404 Not Found

#### Scenario: Invalid preset ID for removal
- **GIVEN** a configured device exists with id `{id}`
- **WHEN** a client sends `DELETE /api/devices/{id}/presets/0`
- **THEN** the system returns 400 Bad Request indicating preset ID must be 1-6

#### Scenario: Device not found for preset removal
- **GIVEN** no configured device exists with id `{id}`
- **WHEN** a client sends `DELETE /api/devices/{id}/presets/1`
- **THEN** the system returns 404 Not Found

---

