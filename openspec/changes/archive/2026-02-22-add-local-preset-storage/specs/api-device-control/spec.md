## ADDED Requirements

### Requirement: Local station file storage endpoint
The system SHALL expose an endpoint to create and serve local internet radio station definition files under `/data/presets/`.

#### Scenario: Create station file for new LOCAL_INTERNET_RADIO preset
- **GIVEN** a configured device exists with id `{id}`
- **WHEN** a client sends `POST /api/devices/{id}/presets` with `source=LOCAL_INTERNET_RADIO` and body containing `streamUrl` and `name`
- **THEN** the system SHALL generate a station filename by slugifying the station name (lowercase, non-alphanumeric replaced with hyphens, consecutive hyphens collapsed)
- **AND** write a JSON file to `/data/presets/<slug>.json` with the structure:
  ```json
  {
    "audio": {
      "hasPlaylist": false,
      "isRealtime": true,
      "streamUrl": "http://stream.example.com/radio"
    },
    "name": "Station Name",
    "streamType": "liveRadio"
  }
  ```
- **AND** set the preset `location` to `{PUBLIC_HOST_URL}/presets/<slug>.json`
- **AND** send the `storePreset` command to the device with the generated location

#### Scenario: Reject duplicate station file on create
- **GIVEN** a station file `/data/presets/jazz-fm.json` already exists
- **WHEN** a client sends `POST /api/devices/{id}/presets` to create a new preset with a name that slugifies to `jazz-fm`
- **AND** the request is a create (not an edit of the preset that owns that file)
- **THEN** the system SHALL return 409 Conflict with message indicating a station with that name already exists

#### Scenario: Overwrite station file on preset edit
- **GIVEN** a preset in slot 3 was previously created with `source=LOCAL_INTERNET_RADIO` and station file `jazz-fm.json`
- **WHEN** a client sends `POST /api/devices/{id}/presets` with id 3 and updated `streamUrl` or `name`
- **THEN** the system SHALL overwrite `/data/presets/jazz-fm.json` with updated content
- **AND** send the updated `storePreset` command to the device

#### Scenario: Serve station file via API
- **GIVEN** a station file `/data/presets/jazz-fm.json` exists
- **WHEN** a client sends `GET /api/presets/jazz-fm.json`
- **THEN** the system SHALL return the file content with `Content-Type: application/json`

#### Scenario: Station file not found
- **GIVEN** no station file `/data/presets/nonexistent.json` exists
- **WHEN** a client sends `GET /api/presets/nonexistent.json`
- **THEN** the system SHALL return 404 Not Found

### Requirement: Public host URL configuration
The system SHALL support a configurable `PUBLIC_HOST_URL` environment variable used to construct the `location` field for locally-stored station presets.

#### Scenario: PUBLIC_HOST_URL configured
- **GIVEN** the environment variable `PUBLIC_HOST_URL` is set to `http://mini.local/soundhub`
- **WHEN** a LOCAL_INTERNET_RADIO preset is stored with station name "Jazz FM"
- **THEN** the preset location sent to the device SHALL be `http://mini.local/soundhub/presets/jazz-fm.json`

#### Scenario: PUBLIC_HOST_URL not configured
- **GIVEN** the environment variable `PUBLIC_HOST_URL` is not set
- **WHEN** a LOCAL_INTERNET_RADIO preset is stored
- **THEN** the system SHALL fall back to `http://localhost:5001` as the host URL

## MODIFIED Requirements

### Requirement: Store preset endpoint
The system SHALL expose an endpoint to create or update a preset on a device. When the preset source is `LOCAL_INTERNET_RADIO`, the system SHALL also manage a local station JSON file and derive the `location` field automatically.

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

#### Scenario: Create preset with LOCAL_INTERNET_RADIO source
- **GIVEN** a configured device exists with id `{id}`
- **AND** the device supports presets
- **WHEN** a client sends `POST /api/devices/{id}/presets` with `source=LOCAL_INTERNET_RADIO` and `streamUrl` provided
- **THEN** the system creates a station JSON file under `/data/presets/`
- **AND** sets the `location` to the public URL of the station file
- **AND** invokes the device adapter to store the preset with the derived location
- **AND** returns 201 Created

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
