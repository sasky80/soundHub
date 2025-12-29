## ADDED Requirements

### Requirement: REST API Foundation
The system SHALL expose a RESTful Web API on .NET 8 with support for device management, control, and status endpoints.

#### Scenario: API starts and reports health
- **WHEN** the API container starts
- **THEN** it listens on port 5000 (http) and 5001 (https)
- **AND** a GET request to `/health` returns a 200 OK response with status "Healthy"

#### Scenario: List all devices
- **WHEN** a client calls GET `/devices`
- **THEN** the API returns a 200 OK with a JSON array of device objects
- **AND** each device object includes `id`, `vendor`, `name`, `status`, `volume`, `power`

#### Scenario: Add a device manually
- **WHEN** a client calls POST `/devices` with a JSON body containing `name`, `ipAddress`, and `vendor`
- **THEN** the API validates the input and returns 201 Created
- **AND** the device is stored in the repository and visible in subsequent GET /devices calls

#### Scenario: Remove a device
- **WHEN** a client calls DELETE `/devices/{id}`
- **THEN** the API returns 204 No Content
- **AND** the device is no longer listed in GET /devices

### Requirement: Device Control Endpoints
The system SHALL provide endpoints to control power, volume, presets, and pairing mode on registered devices.

#### Scenario: Power on a device
- **WHEN** a client calls POST `/devices/{id}/power` with body `{"on": true}`
- **THEN** the API invokes the device adapter's `SetPowerAsync` method
- **AND** returns 200 OK with the updated device status

#### Scenario: Set device volume
- **WHEN** a client calls POST `/devices/{id}/volume` with body `{"level": 50}` (0–100)
- **THEN** the API validates the level range and invokes the device adapter's `SetVolumeAsync`
- **AND** returns 200 OK with the updated volume

#### Scenario: Enter Bluetooth pairing mode
- **WHEN** a client calls POST `/devices/{id}/pairing`
- **THEN** the API invokes the device adapter's `EnterPairingModeAsync`
- **AND** returns 200 OK with confirmation

### Requirement: Preset Management
The system SHALL support querying, configuring, and playing presets (e.g., internet radio stations) on devices.

#### Scenario: List presets on a device
- **WHEN** a client calls GET `/devices/{id}/presets`
- **THEN** the API returns 200 OK with an array of preset objects
- **AND** each preset includes `id`, `name`, `url`, `type`

#### Scenario: Configure a new preset
- **WHEN** a client calls POST `/devices/{id}/presets` with name, URL, and type
- **THEN** the API validates the input and stores the preset
- **AND** returns 201 Created with the preset object

#### Scenario: Play a preset
- **WHEN** a client calls POST `/devices/{id}/presets/{presetId}/play`
- **THEN** the API invokes the device adapter's `PlayPresetAsync`
- **AND** returns 200 OK when the preset begins playing

### Requirement: Device Status Endpoint
The system SHALL provide a status endpoint that returns the current state of a device.

#### Scenario: Get device status
- **WHEN** a client calls GET `/devices/{id}/status`
- **THEN** the API returns 200 OK with a JSON object including power state, volume, current preset/input, and connection status
- **AND** the response is cached for up to 5 seconds to avoid excessive device polling

### Requirement: Structured Logging
The system SHALL log API requests, errors, and device adapter interactions with structured (JSON) output for observability.

#### Scenario: API logs a device control request
- **WHEN** a client calls POST `/devices/{id}/power`
- **THEN** the API logs an entry with timestamp, request ID, method, endpoint, and outcome
- **AND** the log output includes device ID and vendor for tracing

#### Scenario: Device adapter logs an error
- **WHEN** a device adapter call fails (e.g., network timeout)
- **THEN** the API logs the error with stack trace, device ID, and failure reason
- **AND** the request returns 500 Internal Server Error to the client

### Requirement: CORS Configuration
The system SHALL accept cross-origin requests from the frontend application and future mobile/MCP clients.

#### Scenario: Frontend calls API from different origin
- **WHEN** a browser running on port 4200 calls POST `/devices`
- **THEN** the API returns the response with appropriate CORS headers (Access-Control-Allow-Origin, etc.)
- **AND** the request succeeds if the origin is in the allowed list

### Requirement: API Error Handling
The system SHALL return consistent error responses with meaningful status codes and messages.

#### Scenario: Invalid device ID
- **WHEN** a client calls GET `/devices/invalid-id`
- **THEN** the API returns 404 Not Found with a JSON error object including code and message

#### Scenario: Validation error
- **WHEN** a client calls POST `/devices` with missing required fields
- **THEN** the API returns 400 Bad Request with validation errors for each field

### Requirement: OpenAPI Documentation
The system SHALL publish an OpenAPI 3.1 specification and provide interactive documentation.

#### Scenario: Developer accesses OpenAPI specification
- **WHEN** a client requests GET `/openapi.json` or `/swagger/v1/swagger.json`
- **THEN** the API returns the OpenAPI schema in JSON format
- **AND** an interactive UI (e.g., Swagger UI) is available at `/swagger`
