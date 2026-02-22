## MODIFIED Requirements

### Requirement: Create device endpoint
The system SHALL expose an endpoint to create a new device configuration. The system SHALL validate that all input fields are within acceptable length limits and that the resolved IP address is a valid private LAN address.

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

#### Scenario: Reject non-LAN IP address
- **GIVEN** a device payload with ipAddress resolving to a non-private address (e.g., `169.254.169.254`, `127.0.0.1`, `8.8.8.8`)
- **WHEN** a client sends `POST /api/devices`
- **THEN** the system returns 400 Bad Request with message indicating only private LAN addresses are allowed

#### Scenario: Reject oversized device name
- **GIVEN** a device payload with name exceeding 100 characters
- **WHEN** a client sends `POST /api/devices`
- **THEN** the system returns 400 Bad Request with validation error

#### Scenario: Reject oversized IP address
- **GIVEN** a device payload with ipAddress exceeding 45 characters
- **WHEN** a client sends `POST /api/devices`
- **THEN** the system returns 400 Bad Request with validation error

### Requirement: Update device endpoint
The system SHALL expose an endpoint to update an existing device configuration. The system SHALL validate input lengths and that the resolved IP address is a valid private LAN address.

#### Scenario: Update device properties
- **GIVEN** a configured device exists with id `id`
- **WHEN** a client sends `PUT /api/devices/{id}` with updated properties
- **THEN** the system updates the device configuration
- **AND** returns 200 OK with the updated device

#### Scenario: Update device not found
- **GIVEN** no configured device exists with id `id`
- **WHEN** a client sends `PUT /api/devices/{id}`
- **THEN** the system returns a 404 response

#### Scenario: Reject non-LAN IP address on update
- **GIVEN** a configured device exists with id `id`
- **WHEN** a client sends `PUT /api/devices/{id}` with ipAddress resolving to a non-private address
- **THEN** the system returns 400 Bad Request with message indicating only private LAN addresses are allowed

#### Scenario: Reject oversized fields on update
- **GIVEN** a configured device exists with id `id`
- **WHEN** a client sends `PUT /api/devices/{id}` with name exceeding 100 characters or ipAddress exceeding 45 characters
- **THEN** the system returns 400 Bad Request with validation error

### Requirement: Store preset endpoint
The system SHALL expose an endpoint to create or update a preset on a device. When the preset source is `LOCAL_INTERNET_RADIO`, the system SHALL also manage a local station JSON file and derive the `location` field automatically. The system SHALL validate input field lengths.

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

#### Scenario: Reject oversized preset fields
- **GIVEN** a configured device exists with id `{id}`
- **WHEN** a client sends `POST /api/devices/{id}/presets` with name exceeding 100 characters, or location/iconUrl/streamUrl exceeding 2048 characters
- **THEN** the system returns 400 Bad Request with validation error

### Requirement: Local station file storage endpoint
The system SHALL expose an endpoint to create and serve local internet radio station definition files under `/data/presets/`. The system SHALL validate that the requested filename does not contain path traversal sequences.

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

#### Scenario: Reject path traversal in station file request
- **GIVEN** an attacker sends `GET /api/presets/../../etc/passwd`
- **WHEN** the filename contains path traversal sequences (`..`, `/`, `\`)
- **THEN** the system SHALL return 400 Bad Request
- **AND** SHALL NOT attempt to read any file outside the presets directory

### Requirement: Network mask configuration endpoints
The system SHALL expose endpoints to get and set the network mask used for device discovery. The system SHALL validate input length.

#### Scenario: Get network mask
- **GIVEN** a network mask `192.168.1.0/24` is configured
- **WHEN** a client sends `GET /api/config/network-mask`
- **THEN** the system returns the configured mask

#### Scenario: Set network mask
- **GIVEN** a valid CIDR mask `10.0.0.0/24`
- **WHEN** a client sends `PUT /api/config/network-mask` with body `{ "networkMask": "10.0.0.0/24" }`
- **THEN** the system stores the mask for future discovery

#### Scenario: Set invalid network mask
- **GIVEN** an invalid CIDR mask `not-a-mask`
- **WHEN** a client sends `PUT /api/config/network-mask`
- **THEN** the system returns 400 Bad Request

#### Scenario: Reject oversized network mask
- **GIVEN** a network mask value exceeding 18 characters
- **WHEN** a client sends `PUT /api/config/network-mask`
- **THEN** the system returns 400 Bad Request with validation error

## ADDED Requirements

### Requirement: IP address validation for LAN safety
The system SHALL validate that all device IP addresses (after hostname resolution) are within private RFC 1918 ranges. The system SHALL reject loopback, link-local, cloud metadata, and public IP addresses to prevent SSRF attacks.

#### Scenario: Accept private RFC 1918 address
- **GIVEN** a device payload with ipAddress `192.168.1.50`
- **WHEN** the system validates the IP address
- **THEN** the address is accepted

#### Scenario: Reject loopback address
- **GIVEN** a device payload with ipAddress `127.0.0.1`
- **WHEN** the system validates the IP address
- **THEN** the system returns 400 Bad Request indicating only private LAN addresses are allowed

#### Scenario: Reject link-local metadata address
- **GIVEN** a device payload with ipAddress `169.254.169.254`
- **WHEN** the system validates the IP address
- **THEN** the system returns 400 Bad Request

#### Scenario: Reject public IP address
- **GIVEN** a device payload with ipAddress `8.8.8.8`
- **WHEN** the system validates the IP address
- **THEN** the system returns 400 Bad Request

### Requirement: Secure encryption key lifecycle
The `EncryptionKeyStore` and `EncryptedSecretsService` SHALL implement `IDisposable` and zero out cached encryption key material on disposal and during key rotation using `CryptographicOperations.ZeroMemory`.

#### Scenario: Key material zeroed on disposal
- **GIVEN** the application is shutting down
- **WHEN** the DI container disposes `EncryptionKeyStore`
- **THEN** the cached encryption key bytes SHALL be overwritten with zeros

#### Scenario: Old key zeroed on rotation
- **GIVEN** a key rotation is triggered
- **WHEN** `RotateKeyAsync` generates a new key
- **THEN** the previous cached key bytes SHALL be overwritten with zeros before the new key replaces them
