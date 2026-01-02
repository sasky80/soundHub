# Implementation Tasks

## 1. Backend - Extend Device Status API

### 1.1 Update Status Response Model
- [x] 1.1.1 Add `nowPlaying` object to status response model with fields: `stationName`, `artist`, `track`, `source`
- [x] 1.1.2 Update `DevicesController.GetStatus` to include now playing data
- [x] 1.1.3 Ensure `currentSource` is included in status response (AUX, BLUETOOTH, LOCAL_INTERNET_RADIO, etc.)

### 1.2 SoundTouch Adapter Updates
- [x] 1.2.1 Modify `GetStatusAsync` to fetch now playing information from device
- [x] 1.2.2 Parse station name, artist, and track from `/nowPlaying` endpoint response
- [x] 1.2.3 Include source type in status response

## 2. Frontend - Data Access Layer

### 2.1 Update DeviceStatus Interface
- [x] 2.1.1 Add `nowPlaying` property to `DeviceStatus` interface
- [x] 2.1.2 Define `NowPlayingInfo` interface with `stationName`, `artist`, `track` fields
- [x] 2.1.3 Ensure `currentSource` field is typed with known source values

## 3. Frontend - Device Details Component

### 3.1 Now Playing LCD Display
- [x] 3.1.1 Add LCD display section in device-details.component.html below device vendor info
- [x] 3.1.2 Conditionally render LCD display only when device is powered on
- [x] 3.1.3 Format display text as `{StationName}: {Artist}, {Track}`
- [x] 3.1.4 Handle missing fields gracefully (show available info only)

### 3.2 LCD Styling and Animation
- [x] 3.2.1 Create LCD container with retro display appearance (dark background, themed text color)
- [x] 3.2.2 Implement CSS marquee/scroll animation for text overflow
- [x] 3.2.3 Animation scrolls text from right to left continuously
- [x] 3.2.4 Ensure animation only runs when text exceeds container width
- [x] 3.2.5 Add subtle glow effect for authentic LCD look
- [x] 3.2.6 Implement configurable scroll speed (slow/medium/fast) via CSS custom properties
- [x] 3.2.7 Implement color theme options (green/amber/blue) via CSS custom properties

### 3.3 Status Polling Implementation
- [x] 3.3.1 Create polling interval (10 seconds) using RxJS `interval` and `switchMap`
- [x] 3.3.2 Start polling when component initializes and device is powered on
- [x] 3.3.3 Stop polling when device powers off
- [x] 3.3.4 Restart polling when device powers back on
- [x] 3.3.5 Clean up polling subscription on component destroy
- [x] 3.3.6 Update status signal with polled data

### 3.4 Bluetooth Button Behavior Update
- [x] 3.4.1 Remove `pairingMessage` display after successful pairing
- [x] 3.4.2 Add `[class.active]` binding based on `currentSource === 'BLUETOOTH'`
- [x] 3.4.3 Add `[attr.aria-pressed]` for accessibility when Bluetooth is active source
- [x] 3.4.4 Keep click handler for entering pairing mode

### 3.5 AUX Button State Update
- [x] 3.5.1 Update `activeSource` signal to derive from polled `currentSource`
- [x] 3.5.2 Ensure AUX button shows active when `currentSource === 'AUX'` (or `AUX_INPUT`)
- [x] 3.5.3 Remove manual `activeSource.set('AUX_INPUT')` in key press handler

## 4. Frontend - Settings Page

### 4.1 LCD Display Settings
- [x] 4.1.1 Add LCD settings section to settings page
- [x] 4.1.2 Add scroll speed dropdown (Slow, Medium, Fast)
- [x] 4.1.3 Add color theme selector with preview swatches (Green, Amber, Blue)
- [x] 4.1.4 Persist settings to local storage
- [x] 4.1.5 Apply settings reactively to LCD display component

## 5. Frontend - Translations

### 5.1 Add Translation Keys
- [x] 5.1.1 Add `controlPanel.nowPlaying` key for section label (if needed)
- [x] 5.1.2 Add `controlPanel.noPlayback` for empty state display
- [x] 5.1.3 Add `settings.lcdScrollSpeed` and speed option labels
- [x] 5.1.4 Add `settings.lcdColorTheme` and theme option labels
- [x] 5.1.5 Update both English and Polish translation files

## 6. Testing

### 6.1 Unit Tests
- [x] 6.1.1 Test Now Playing display formatting with various data combinations
- [x] 6.1.2 Test polling start/stop based on power state
- [x] 6.1.3 Test Bluetooth button active state based on source
- [x] 6.1.4 Test AUX button active state based on source
- [x] 6.1.5 Test scroll speed setting persistence and application
- [x] 6.1.6 Test color theme setting persistence and application

### 6.2 E2E Tests
- [x] 6.2.1 Add test for Now Playing display visibility when device is on
- [x] 6.2.2 Add test for Now Playing hidden when device is off
- [x] 6.2.3 Add test for Bluetooth button active state
- [x] 6.2.4 Add test for AUX button active state
- [x] 6.2.5 Add test for LCD settings on settings page

## 7. Documentation

- [ ] 7.1 Update API reference with extended status response
- [ ] 7.2 Add Now Playing feature to device configuration guide
- [ ] 7.3 Document LCD customization options (scroll speed, color themes)
