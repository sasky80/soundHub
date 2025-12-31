# Tasks: SoundTouch Extended Device Control

## Phase 1: Domain Model Updates
- [x] **Task 1.1**: Add `DeviceInfo` entity to `SoundHub.Domain/Entities/`
  - Include properties: DeviceId, Name, Type, MacAddress, IpAddress, SoftwareVersion
  - Verification: Project compiles, entity is referenceable from Application layer

- [x] **Task 1.2**: Add `NowPlayingInfo` DTO to Domain layer
  - Include properties: Source, Track, Artist, Album, StationName, PlayStatus
  - Verification: Project compiles

- [x] **Task 1.3**: Add `VolumeInfo` DTO to Domain layer
  - Include properties: TargetVolume, ActualVolume, IsMuted
  - Verification: Project compiles

## Phase 2: Adapter Interface Updates
- [x] **Task 2.1**: Add `GetDeviceInfoAsync` method to `IDeviceAdapter`
  - Signature: `Task<DeviceInfo> GetDeviceInfoAsync(string deviceId, CancellationToken ct = default)`
  - Verification: Interface compiles, adapter implementations require update

- [x] **Task 2.2**: Add `GetNowPlayingAsync` method to `IDeviceAdapter`
  - Signature: `Task<NowPlayingInfo> GetNowPlayingAsync(string deviceId, CancellationToken ct = default)`
  - Verification: Interface compiles

- [x] **Task 2.3**: Add `GetVolumeAsync` method to `IDeviceAdapter`
  - Signature: `Task<VolumeInfo> GetVolumeAsync(string deviceId, CancellationToken ct = default)`
  - Verification: Interface compiles

## Phase 3: SoundTouchAdapter Implementation
- [x] **Task 3.1**: Implement helper method `SendKeyPressAsync(ip, keyName)`
  - Handles press/release pattern with 100ms delay
  - Uses XML body: `<key state="press|release" sender="Gabbo">{keyName}</key>`
  - Verification: Helper method is private, compiles

- [x] **Task 3.2**: Implement `GetDeviceInfoAsync`
  - GET `/info` endpoint, parse XML response
  - Extract name, type, deviceID, network info, software version
  - Verification: Unit test with mocked HTTP response

- [x] **Task 3.3**: Implement `GetNowPlayingAsync`
  - GET `/nowPlaying` endpoint, parse XML response
  - Extract source, track, artist, album, playStatus
  - Verification: Unit test with mocked HTTP response

- [x] **Task 3.4**: Implement `GetVolumeAsync`
  - GET `/volume` endpoint, parse XML response
  - Extract targetvolume, actualvolume, muteenabled
  - Verification: Unit test with mocked HTTP response

- [x] **Task 3.5**: Implement real `SetVolumeAsync`
  - POST `/volume` with body `<volume>{level}</volume>`
  - Replace existing mock implementation
  - Verification: Unit test with mocked HTTP response

- [x] **Task 3.6**: Implement real `SetPowerAsync`
  - If `on=true`: Send POWER key press/release
  - If `on=false`: GET `/standby`
  - Replace existing mock implementation
  - Verification: Unit test with mocked HTTP response

- [x] **Task 3.7**: Implement real `EnterPairingModeAsync`
  - GET `/enterBluetoothPairing` endpoint
  - Replace existing mock implementation
  - Verification: Unit test with mocked HTTP response

- [x] **Task 3.8**: Implement real `ListPresetsAsync`
  - GET `/presets` endpoint, parse XML response
  - Map to `Preset` entities with id, name, source info
  - Replace existing mock implementation
  - Verification: Unit test with mocked HTTP response

- [x] **Task 3.9**: Implement real `PlayPresetAsync`
  - Send `PRESET_{n}` key press/release where n = presetId (1-6)
  - Validate presetId is 1-6, throw if invalid
  - Replace existing mock implementation
  - Verification: Unit test with mocked HTTP response

- [x] **Task 3.10**: Implement real `GetStatusAsync`
  - Combine volume, nowPlaying, and power state into DeviceStatus
  - Replace existing mock implementation
  - Verification: Unit test with mocked HTTP response

## Phase 4: API Controller Updates
- [x] **Task 4.1**: Add `GET /api/devices/{id}/volume` endpoint
  - Call adapter's GetVolumeAsync, return VolumeInfo as JSON
  - Verification: Manual API test or integration test

- [x] **Task 4.2**: Add `POST /api/devices/{id}/volume` endpoint
  - Accept `{ "level": int }`, validate 0-100, call SetVolumeAsync
  - Verification: Manual API test or integration test

- [x] **Task 4.3**: Add `GET /api/devices/{id}/info` endpoint
  - Call adapter's GetDeviceInfoAsync, return DeviceInfo as JSON
  - Verification: Manual API test or integration test

- [x] **Task 4.4**: Add `GET /api/devices/{id}/nowPlaying` endpoint
  - Call adapter's GetNowPlayingAsync, return NowPlayingInfo as JSON
  - Verification: Manual API test or integration test

- [x] **Task 4.5**: Add `POST /api/devices/{id}/bluetooth/pairing` endpoint
  - Call adapter's EnterPairingModeAsync, return success
  - Verification: Manual API test or integration test

- [x] **Task 4.6**: Add `GET /api/devices/{id}/presets` endpoint
  - Call adapter's ListPresetsAsync, return array of presets
  - Verification: Manual API test or integration test

- [x] **Task 4.7**: Add `POST /api/devices/{id}/presets/{presetNumber}/play` endpoint
  - Validate presetNumber 1-6, call PlayPresetAsync
  - Verification: Manual API test or integration test

## Phase 5: Testing & Documentation
- [x] **Task 5.1**: Add unit tests for SoundTouchAdapter XML parsing
  - Use sample XML responses from API documentation
  - Cover happy path and edge cases (empty responses, missing elements)
  - Verification: All tests pass (27 tests)

- [x] **Task 5.2**: Add integration test fixtures (optional)
  - Document how to run tests against real SoundTouch device
  - Verification: README updated with test instructions

- [x] **Task 5.3**: Update API documentation
  - Document new endpoints in OpenAPI spec or README
  - Verification: Documentation reflects all new endpoints

## Dependencies
- Phase 2 depends on Phase 1 completion
- Phase 3 depends on Phase 2 completion
- Phase 4 depends on Phase 3 completion
- Phase 5 can begin in parallel with Phase 4

## Parallelization Notes
- Tasks within Phase 1 can be done in parallel
- Tasks within Phase 3 can largely be done in parallel (independent endpoints)
- Tasks within Phase 4 can be done in parallel
