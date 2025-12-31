# api-device-control Specification Delta

## ADDED Requirements

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
The system SHALL expose an endpoint to activate a preset on a device.

#### Scenario: Play preset by number
- **GIVEN** a configured device exists with id `{id}` with preset 1 configured
- **WHEN** a client sends `POST /api/devices/{id}/presets/1/play`
- **THEN** the system invokes the device adapter to play preset 1
- **AND** the device begins playback of the preset content
- **AND** returns a success response

#### Scenario: Invalid preset number
- **GIVEN** a configured device exists with id `{id}`
- **WHEN** a client sends `POST /api/devices/{id}/presets/7/play`
- **THEN** the system returns a 400 response indicating preset number must be 1-6

#### Scenario: Preset not configured
- **GIVEN** a configured device exists with id `{id}` but preset 3 is empty
- **WHEN** a client sends `POST /api/devices/{id}/presets/3/play`
- **THEN** the system returns a 404 response indicating preset is not configured
