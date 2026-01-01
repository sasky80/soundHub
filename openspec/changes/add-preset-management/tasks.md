## 1. Backend API Implementation
- [x] 1.1 Create `POST /api/devices/{id}/presets` endpoint for storing/updating presets
- [x] 1.2 Create `DELETE /api/devices/{id}/presets/{presetId}` endpoint for removing presets
- [x] 1.3 Extend `GET /api/devices/{id}/presets` response to include icon URLs and full preset data
- [x] 1.4 Implement SoundTouch adapter methods for `/storePreset` and `/removePreset` WebServices API
- [x] 1.5 Add preset model/DTO with fields: id, name, iconUrl, location, type, source (SoundTouch-specific)

## 2. Frontend - Preset List Component
- [x] 2.1 Create preset list component displaying presets below volume controls
- [x] 2.2 Implement 64x64px play button with preset icon background (or default icon)
- [x] 2.3 Display preset name below/beside each preset button
- [x] 2.4 Add play button click handler that calls play endpoint (and powers on device if off)
- [x] 2.5 Add "+" button to navigate to new preset form

## 3. Frontend - Preset Form/Details Page
- [x] 3.1 Create preset definition page route and component
- [x] 3.2 Implement form fields: name, image URL (optional), location
- [x] 3.3 Add SoundTouch-specific fields: type (default "stationurl"), source (default "LOCAL_INTERNET_RADIO")
- [x] 3.4 Implement save handler for create/update operations
- [x] 3.5 Implement delete handler with confirmation dialog

## 4. Frontend - Navigation and Routing
- [x] 4.1 Add route for preset details page (`/devices/:deviceId/presets/new`, `/devices/:deviceId/presets/:presetId`)
- [x] 4.2 Wire navigation from preset list to preset details
- [x] 4.3 Wire navigation back from preset details to device details

## 5. Integration and Testing
- [ ] 5.1 Write unit tests for preset service (frontend)
- [ ] 5.2 Write unit tests for preset endpoints (backend)
- [ ] 5.3 Write e2e tests for preset management flow
- [ ] 5.4 Test with real SoundTouch device

## 6. Documentation
- [ ] 6.1 Update API reference docs with new preset endpoints (POST, DELETE)
- [ ] 6.2 Update device configuration guide with preset management instructions
- [ ] 6.3 Add preset-related examples to API documentation
- [ ] 6.4 Document SoundTouch-specific preset fields (type, source) and default values
