## ADDED Requirements

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
