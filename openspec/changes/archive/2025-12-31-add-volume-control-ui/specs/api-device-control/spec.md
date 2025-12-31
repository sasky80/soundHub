# api-device-control Specification Delta

## ADDED Requirements

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
