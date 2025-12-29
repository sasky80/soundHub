## ADDED Requirements

### Requirement: Device Adapter Interface
The system SHALL define an abstract device adapter interface that encapsulates vendor-specific device control logic.

#### Scenario: Define IDeviceAdapter contract
- **WHEN** a developer reviews the `IDeviceAdapter` interface
- **THEN** it includes methods for `GetStatusAsync`, `SetPowerAsync`, `SetVolumeAsync`, `EnterPairingModeAsync`, `ListPresetsAsync`, `ConfigurePresetAsync`, `PlayPresetAsync`
- **AND** all methods accept a device ID, optional parameters, and a CancellationToken

#### Scenario: Implement a vendor-specific adapter
- **WHEN** a developer creates a new adapter (e.g., `SoundTouchAdapter : IDeviceAdapter`)
- **THEN** the adapter implements all interface methods and handles vendor-specific API calls
- **AND** the implementation is testable via dependency injection

### Requirement: Device Adapter Registry
The system SHALL maintain a registry to resolve device adapters by vendor ID at runtime.

#### Scenario: Register an adapter
- **WHEN** the application starts, adapters are registered in a factory or service collection
- **THEN** the registry maps vendor IDs (e.g., "bose-soundtouch") to adapter implementations
- **AND** new adapters can be added without modifying existing device management code

#### Scenario: Resolve adapter for a device
- **WHEN** a device control request is made for a device with vendor "bose-soundtouch"
- **THEN** the registry returns the `SoundTouchAdapter` instance
- **AND** all subsequent calls use the correct adapter without manual branching

### Requirement: Device Repository Pattern
The system SHALL provide a repository interface for persisting and retrieving device metadata.

#### Scenario: Add a device to the repository
- **WHEN** the API calls `repository.AddDeviceAsync(device)`
- **THEN** the device is stored and assigned a unique ID
- **AND** subsequent calls to `repository.GetDeviceAsync(id)` return the device

#### Scenario: List all devices
- **WHEN** the API calls `repository.GetAllDevicesAsync()`
- **THEN** all registered devices are returned as a collection
- **AND** devices can be filtered by vendor or status

#### Scenario: Remove a device
- **WHEN** the API calls `repository.RemoveDeviceAsync(id)`
- **THEN** the device is deleted from storage
- **AND** the ID is no longer valid for subsequent queries

### Requirement: SoundTouch Adapter Stub
The system SHALL provide a minimal SoundTouch adapter implementation with placeholder methods for testing.

#### Scenario: SoundTouch adapter exists
- **WHEN** the application starts with SoundTouch registered
- **THEN** a `SoundTouchAdapter` class exists and implements `IDeviceAdapter`
- **AND** all methods return sensible defaults (e.g., GetStatusAsync returns a mock status with power on, volume 50)

#### Scenario: Mock SoundTouch device control
- **WHEN** a control endpoint calls the SoundTouch adapter (e.g., SetPowerAsync)
- **THEN** the adapter logs the operation and returns success
- **AND** the actual device integration is deferred to a follow-up change

### Requirement: Adapter Testing Support
The system SHALL support unit testing of adapters with mock HTTP clients and repositories.

#### Scenario: Unit test adapter in isolation
- **WHEN** a test creates a `SoundTouchAdapter` with a mocked HTTP client
- **THEN** the adapter method calls the mocked client and returns expected results
- **AND** the test verifies correct endpoint calls and parameter passing

#### Scenario: Integration test adapter with repository
- **WHEN** an integration test registers a device and calls adapter methods
- **THEN** the device state is updated in the repository
- **AND** the device's status reflects the adapter's response
