# api-device-control Specification Delta

## MODIFIED Requirements

### Requirement: Now playing status endpoint
The system SHALL expose an endpoint to retrieve the current playback status including station name, artist, and track information.

#### Scenario: Get now playing status
- **GIVEN** a configured device exists with id `{id}` and is playing media
- **WHEN** a client sends `GET /api/devices/{id}/nowPlaying`
- **THEN** the system queries the device adapter for now playing information
- **AND** returns playback details including source, stationName, artist, track, and play status

#### Scenario: Get now playing with radio station
- **GIVEN** a configured device exists with id `{id}` and is playing internet radio
- **WHEN** a client sends `GET /api/devices/{id}/nowPlaying`
- **THEN** the response includes `stationName`, `artist`, `track` fields
- **AND** the `source` field is set to `LOCAL_INTERNET_RADIO` or similar

#### Scenario: Get now playing with Bluetooth source
- **GIVEN** a configured device exists with id `{id}` and is playing via Bluetooth
- **WHEN** a client sends `GET /api/devices/{id}/nowPlaying`
- **THEN** the response includes available metadata (`artist`, `track`)
- **AND** the `source` field is set to `BLUETOOTH`

#### Scenario: Get now playing with AUX source
- **GIVEN** a configured device exists with id `{id}` and is playing via AUX input
- **WHEN** a client sends `GET /api/devices/{id}/nowPlaying`
- **THEN** the response indicates source is `AUX`
- **AND** metadata fields may be empty (no metadata available for AUX)

#### Scenario: Device in standby
- **GIVEN** a configured device exists with id `{id}` and is in standby
- **WHEN** a client sends `GET /api/devices/{id}/nowPlaying`
- **THEN** the system returns a response indicating source is `STANDBY`

---

## ADDED Requirements

### Requirement: Device status includes now playing information
The system SHALL include now playing information in the device status response to support polling for real-time playback updates.

#### Scenario: Get device status with now playing
- **GIVEN** a configured device exists with id `{id}` and is playing media
- **WHEN** a client sends `GET /api/devices/{id}/status`
- **THEN** the response includes `nowPlaying` object with `stationName`, `artist`, `track`
- **AND** the response includes `currentSource` field indicating the active source

#### Scenario: Get device status with current source
- **GIVEN** a configured device exists with id `{id}`
- **WHEN** a client sends `GET /api/devices/{id}/status`
- **THEN** the response includes `currentSource` field
- **AND** the value is one of: `STANDBY`, `AUX`, `BLUETOOTH`, `LOCAL_INTERNET_RADIO`, or other source identifiers

#### Scenario: Status response structure
- **GIVEN** a configured device exists with id `{id}`
- **WHEN** a client sends `GET /api/devices/{id}/status`
- **THEN** the response follows the structure:
  ```json
  {
    "isOnline": true,
    "powerState": true,
    "volume": 45,
    "currentSource": "LOCAL_INTERNET_RADIO",
    "nowPlaying": {
      "stationName": "Jazz FM",
      "artist": "Miles Davis",
      "track": "So What"
    }
  }
  ```

#### Scenario: Status with device in standby
- **GIVEN** a configured device exists with id `{id}` and is in standby
- **WHEN** a client sends `GET /api/devices/{id}/status`
- **THEN** `currentSource` is `STANDBY`
- **AND** `nowPlaying` is null or omitted
