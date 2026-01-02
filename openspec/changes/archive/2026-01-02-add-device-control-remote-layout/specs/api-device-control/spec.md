# api-device-control Specification Delta

## ADDED Requirements

### Requirement: Key press endpoint
The system SHALL provide an endpoint to send key press commands to SoundTouch devices.

#### Scenario: Press key on device
- **GIVEN** a valid device ID and key name
- **WHEN** `POST /api/devices/{id}/key` is called with body `{ "key": "KEY_NAME" }`
- **THEN** the system sends a key press to the SoundTouch device
- **AND** the system sends a key release immediately after
- **AND** returns HTTP 200 on success

#### Scenario: Supported key values
- **GIVEN** the key press endpoint
- **WHEN** a request is made with a supported key name
- **THEN** the following keys are supported:
  - `AUX_INPUT` - Switch to AUX input source
  - `PREV_TRACK` - Move to previous track
  - `NEXT_TRACK` - Move to next track
  - `PLAY_PAUSE` - Toggle between play and pause
  - `VOLUME_UP` - Increase volume by one level
  - `VOLUME_DOWN` - Decrease volume by one level

#### Scenario: Invalid key name
- **GIVEN** an invalid or unsupported key name
- **WHEN** `POST /api/devices/{id}/key` is called
- **THEN** the system returns HTTP 400 Bad Request
- **AND** the response includes an error message

#### Scenario: Device not found
- **GIVEN** an invalid device ID
- **WHEN** `POST /api/devices/{id}/key` is called
- **THEN** the system returns HTTP 404 Not Found

### Requirement: Key press implementation for SoundTouch
The system SHALL implement key press operations using the SoundTouch `/key` WebServices API endpoint.

#### Scenario: Send key press and release
- **GIVEN** a key name to be sent to a SoundTouch device
- **WHEN** the key press operation is executed
- **THEN** the system sends an HTTP POST to `http://{deviceIP}:8090/key` with XML body:
  ```xml
  <key state="press" sender="Gabbo">KEY_NAME</key>
  ```
- **AND** immediately follows with another POST with XML body:
  ```xml
  <key state="release" sender="Gabbo">KEY_NAME</key>
  ```
- **AND** waits for both requests to complete successfully

#### Scenario: Key press timeout
- **GIVEN** the SoundTouch device is unreachable or unresponsive
- **WHEN** a key press operation is attempted
- **THEN** the system times out after 5 seconds
- **AND** returns HTTP 504 Gateway Timeout

### Requirement: Bluetooth pairing endpoint
The system SHALL provide an endpoint to enter Bluetooth pairing mode on devices that support it.

#### Scenario: Enter Bluetooth pairing mode
- **GIVEN** a device ID that supports Bluetooth pairing
- **WHEN** `POST /api/devices/{id}/bluetooth/enter-pairing` is called
- **THEN** the system sends a pairing request to the SoundTouch device
- **AND** returns HTTP 200 on success
- **AND** the device enters Bluetooth pairing mode

#### Scenario: Device does not support Bluetooth pairing
- **GIVEN** a device ID that does not support Bluetooth pairing
- **WHEN** `POST /api/devices/{id}/bluetooth/enter-pairing` is called
- **THEN** the system returns HTTP 400 Bad Request
- **AND** the response includes a message: "Device does not support Bluetooth pairing"

#### Scenario: Device not found
- **GIVEN** an invalid device ID
- **WHEN** `POST /api/devices/{id}/bluetooth/enter-pairing` is called
- **THEN** the system returns HTTP 404 Not Found

### Requirement: Bluetooth pairing implementation for SoundTouch
The system SHALL implement Bluetooth pairing using the SoundTouch `/enterBluetoothPairing` WebServices API endpoint.

#### Scenario: Trigger Bluetooth pairing on SoundTouch device
- **GIVEN** a SoundTouch device that supports Bluetooth
- **WHEN** the Bluetooth pairing operation is executed
- **THEN** the system sends an HTTP GET to `http://{deviceIP}:8090/enterBluetoothPairing`
- **AND** waits for the response
- **AND** returns success if the device responds with status `/enterBluetoothPairing`

#### Scenario: Bluetooth pairing mode timeout
- **GIVEN** the SoundTouch device is unreachable or unresponsive
- **WHEN** Bluetooth pairing is attempted
- **THEN** the system times out after 10 seconds
- **AND** returns HTTP 504 Gateway Timeout

### Requirement: Device capabilities include Bluetooth pairing support
The system SHALL report whether a device supports Bluetooth pairing in its capabilities.

#### Scenario: Device capabilities include bluetoothPairing flag
- **GIVEN** a SoundTouch device
- **WHEN** device capabilities are queried
- **THEN** the response includes a `bluetoothPairing` boolean property
- **AND** the value is `true` if the device `/capabilities` response includes `<capability name="bluetoothPairing" .../>`
- **AND** the value is `false` otherwise

#### Scenario: Capabilities cached
- **GIVEN** device capabilities have been fetched
- **WHEN** the capabilities are requested again
- **THEN** the system returns cached capabilities
- **AND** refreshes capabilities only on device restart or cache expiry
