## MODIFIED Requirements

### Requirement: Presets list endpoint
The system SHALL expose an endpoint to list all configured presets on a device including full preset details.

#### Scenario: Get presets list
- **GIVEN** a configured device exists with id `{id}` with presets configured
- **WHEN** a client sends `GET /api/devices/{id}/presets`
- **THEN** the system queries the device adapter for preset list
- **AND** returns an array of presets with id, name, iconUrl, location, type, source, and isPresetable

#### Scenario: No presets configured
- **GIVEN** a configured device exists with id `{id}` with no presets
- **WHEN** a client sends `GET /api/devices/{id}/presets`
- **THEN** the system returns an empty array

#### Scenario: Presets include icon URLs
- **GIVEN** a configured device exists with id `{id}` with presets that have containerArt
- **WHEN** a client sends `GET /api/devices/{id}/presets`
- **THEN** each preset includes an `iconUrl` field with the container art URL

---

## ADDED Requirements

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

## MODIFIED Requirements

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
