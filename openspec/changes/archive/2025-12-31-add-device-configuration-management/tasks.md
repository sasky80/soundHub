# Tasks: Device Configuration Management

## 1. Backend - Data Model Updates
- [x] 1.1 Add `NetworkMask` property to configuration model
- [x] 1.2 Add `DateTimeAdded` property to Device entity (set on creation, never changes)
- [x] 1.3 Update `JsonDeviceRepository` to read/write NetworkMask from devices.json root
- [x] 1.4 Remove from Device entity: `Port`, `Volume`, `IsOnline`, `PowerState`, `LastSeen`
- [x] 1.5 Move port constant to adapter (SoundTouch = 8090)

## 2. Backend - IDeviceAdapter Updates
- [x] 2.1 Add `PingAsync(string deviceId)` method to `IDeviceAdapter` interface
- [x] 2.2 Add `DiscoverDevicesAsync(string networkMask)` method signature with mask parameter

## 3. Backend - SoundTouchAdapter Implementation
- [x] 3.1 Implement `PingAsync` using HTTP GET to `/playNotification` (audible beep)
- [x] 3.2 Implement `DiscoverDevicesAsync` to scan IP range and probe port 8090
- [x] 3.3 Define base capabilities constant: `["power", "volume"]`
- [x] 3.4 Query `/supportedUrls` to determine additional capabilities:
  - `/presets` → add "presets" capability
  - `/enterBluetoothPairing` → add "bluetoothPairing" capability
  - `/playNotification` → add "ping" capability

## 4. Backend - API Endpoints
- [x] 4.1 Add `GET /api/devices/{id}/ping` endpoint
- [x] 4.2 Add `POST /api/devices/discover` endpoint
- [x] 4.3 Add `GET /api/config/network-mask` endpoint
- [x] 4.4 Add `PUT /api/config/network-mask` endpoint
- [x] 4.5 Add `GET /api/vendors` endpoint to list supported vendors
- [x] 4.6 Implement `POST /api/devices` for creating new device
- [x] 4.7 Implement `PUT /api/devices/{id}` for updating device
- [x] 4.8 Implement `DELETE /api/devices/{id}` for removing device

## 5. Backend - Validation
- [x] 5.1 Add network mask format validation (CIDR notation)
- [x] 5.2 Add IP address / FQDN validation for device creation
- [x] 5.3 Ensure discovery skips already-configured IPs

## 6. Frontend - Data Access Layer
- [x] 6.1 Create `DeviceConfigService` in data-access library
- [x] 6.2 Add `pingDevice(id)` method
- [x] 6.3 Add `discoverDevices()` method
- [x] 6.4 Add `getNetworkMask()` and `updateNetworkMask(mask)` methods
- [x] 6.5 Add `getVendors()` method
- [x] 6.6 Add `createDevice(device)` method
- [x] 6.7 Add `updateDevice(id, device)` method
- [x] 6.8 Add `deleteDevice(id)` method

## 7. Frontend - Device Configuration Feature
- [x] 7.1 Create `device-config` feature library
- [x] 7.2 Create device configuration page component
- [x] 7.3 Create device list component with ping button
- [x] 7.4 Implement ping button visibility based on "ping" capability
- [x] 7.5 Create add device button and navigation
- [x] 7.6 Create device form component (add/edit)
- [x] 7.7 Implement vendor dropdown populated from API
- [x] 7.8 Implement capabilities checkbox list
- [x] 7.9 Implement save functionality for add/edit
- [x] 7.10 Implement delete device with confirmation dialog
- [x] 7.11 Create network mask input field
- [x] 7.12 Create discover devices button with loading state
- [x] 7.13 Highlight devices where `DateTimeAdded` is within last 5 minutes
- [x] 7.14 Add route for device configuration page
- [x] 7.15 Add navigation from settings page to device configuration

## 8. Frontend - UI Components
- [x] 8.1 Create ping button component with status indicator
- [x] 8.2 Create device list item component
- [x] 8.3 Create capability selector component
- [x] 8.4 Create network mask input component with validation

## 9. Testing
- [x] 9.1 Unit tests for network mask parsing/validation
- [x] 9.2 Unit tests for device repository CRUD operations
- [x] 9.3 Unit tests for ping endpoint
- [x] 9.4 Unit tests for discovery endpoint
- [x] 9.5 Integration tests for device configuration API
- [x] 9.6 Frontend unit tests for device config components
- [x] 9.7 E2E test for device add/edit/remove flow

## 10. Documentation
- [x] 10.1 Update API documentation with new endpoints
- [x] 10.2 Update devices.json schema documentation
- [x] 10.3 Add user guide for device configuration
- [x] 10.4 Update README.md with device configuration feature
- [x] 10.5 Update docs/architecture.md with device management components

