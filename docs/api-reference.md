# SoundHub API Documentation

This document provides detailed documentation for the SoundHub REST API endpoints, including device configuration management.

## Base URL

```
http://localhost:5001/api
```

## Authentication

Currently, the API does not require authentication. Authentication may be added in future versions.

---

## Device Management

### List All Devices

Returns all registered devices.

```http
GET /api/devices
```

**Response**
```json
[
  {
    "id": "abc123",
    "vendor": "bose-soundtouch",
    "name": "Living Room Speaker",
    "ipAddress": "192.168.1.100",
    "capabilities": ["power", "volume", "presets", "ping"],
    "dateTimeAdded": "2025-12-31T12:00:00.000Z"
  }
]
```

### Get Device by ID

```http
GET /api/devices/{id}
```

**Response**
```json
{
  "id": "abc123",
  "vendor": "bose-soundtouch",
  "name": "Living Room Speaker",
  "ipAddress": "192.168.1.100",
  "capabilities": ["power", "volume", "presets", "ping"],
  "dateTimeAdded": "2025-12-31T12:00:00.000Z"
}
```

**Error Responses**
- `404 Not Found` - Device not found

### Add Device

Creates a new device manually. Capabilities are automatically detected based on the vendor adapter.

```http
POST /api/devices
Content-Type: application/json

{
  "name": "Kitchen Speaker",
  "ipAddress": "192.168.1.50",
  "vendor": "bose-soundtouch"
}
```

**Response** (201 Created)
```json
{
  "id": "new-device-id",
  "vendor": "bose-soundtouch",
  "name": "Kitchen Speaker",
  "ipAddress": "192.168.1.50",
  "capabilities": ["power", "volume"],
  "dateTimeAdded": "2025-12-31T15:30:00.000Z"
}
```

**Error Responses**
- `400 Bad Request` - Invalid input (missing required fields, invalid IP/FQDN)

### Update Device

Updates an existing device's configuration.

```http
PUT /api/devices/{id}
Content-Type: application/json

{
  "name": "Updated Speaker Name",
  "ipAddress": "192.168.1.51",
  "capabilities": ["power", "volume", "presets"]
}
```

**Response** (200 OK)
```json
{
  "id": "abc123",
  "vendor": "bose-soundtouch",
  "name": "Updated Speaker Name",
  "ipAddress": "192.168.1.51",
  "capabilities": ["power", "volume", "presets"],
  "dateTimeAdded": "2025-12-31T12:00:00.000Z"
}
```

**Error Responses**
- `404 Not Found` - Device not found
- `400 Bad Request` - Invalid input

### Delete Device

Removes a device from the system.

```http
DELETE /api/devices/{id}
```

**Response**
- `204 No Content` - Successfully deleted

**Error Responses**
- `404 Not Found` - Device not found

---

## Device Discovery

### Discover Devices

Scans the local network for devices and auto-saves newly discovered devices.

```http
POST /api/devices/discover
```

**Response** (200 OK)
```json
{
  "discovered": 5,
  "new": 2,
  "newDevices": [
    {
      "id": "discovered-1",
      "vendor": "bose-soundtouch",
      "name": "Bedroom Speaker",
      "ipAddress": "192.168.1.105",
      "capabilities": ["power", "volume", "presets", "ping"],
      "dateTimeAdded": "2025-12-31T16:00:00.000Z"
    }
  ]
}
```

**Notes**
- Uses the configured network mask from `/api/config/network-mask`
- Skips devices that are already configured (matched by IP address)
- Newly discovered devices are highlighted in the UI for 5 minutes

---

## Device Connectivity

### Ping Device

Sends an audible ping to the device for connectivity verification. The device will emit a double beep sound.

```http
GET /api/devices/{id}/ping
```

**Response** (200 OK)
```json
{
  "reachable": true,
  "latencyMs": 45
}
```

**Notes**
- Only available for devices with the "ping" capability
- Uses the `/playNotification` endpoint on SoundTouch devices
- Timeout is 10 seconds (configurable in appsettings.json)

**Error Responses**
- `404 Not Found` - Device not found
- `501 Not Implemented` - Device vendor does not support ping

---

## Configuration

### Get Network Mask

Returns the configured network mask for device discovery.

```http
GET /api/config/network-mask
```

**Response** (200 OK)
```json
{
  "networkMask": "192.168.1.0/24"
}
```

### Set Network Mask

Sets the network mask for device discovery.

```http
PUT /api/config/network-mask
Content-Type: application/json

{
  "networkMask": "192.168.1.0/24"
}
```

**Response**
- `204 No Content` - Successfully updated

**Error Responses**
- `400 Bad Request` - Invalid network mask format (must be CIDR notation)

**Valid Formats**
- `192.168.1.0/24` - Class C network (254 hosts)
- `10.0.0.0/8` - Class A network
- `172.16.0.0/12` - Class B private network

---

## Vendors

### List Supported Vendors

Returns the list of supported device vendors.

```http
GET /api/vendors
```

**Response** (200 OK)
```json
[
  {
    "id": "bose-soundtouch",
    "name": "Bose SoundTouch"
  }
]
```

---

## Device Status & Control

### Get Device Status

Returns the current operational status of a device.

```http
GET /api/devices/{id}/status
```

**Response** (200 OK)
```json
{
  "isOnline": true,
  "powerState": true,
  "volume": 45,
  "currentSource": "SPOTIFY",
  "nowPlaying": {
    "source": "SPOTIFY",
    "stationName": "My Playlist",
    "artist": "Queen",
    "track": "Bohemian Rhapsody"
  }
}
```

**Notes**
- `currentSource` represents the device's active source (for example: `AUX`, `BLUETOOTH`, `LOCAL_INTERNET_RADIO`, `SPOTIFY`).
- `nowPlaying` may be `null` (or contain missing fields) when the device is idle or the vendor does not provide metadata.
- For richer playback details (e.g., `album`, `playStatus`, `artUrl`), use the dedicated **Get Now Playing** endpoint.

### Set Power

Controls the power state of a device.

```http
POST /api/devices/{id}/power
Content-Type: application/json

{
  "on": true
}
```

**Response**
- `204 No Content` - Successfully set

### Get Device Info

Returns detailed information about a device.

```http
GET /api/devices/{id}/info
```

**Response** (200 OK)
```json
{
  "deviceId": "C8DF84AE0B6E",
  "name": "Living Room Speaker",
  "type": "SoundTouch 10",
  "macAddress": "C8:DF:84:AE:0B:6E",
  "ipAddress": "192.168.1.100",
  "softwareVersion": "27.0.6.1"
}
```

### Get Now Playing

Returns current playback information.

```http
GET /api/devices/{id}/nowPlaying
```

**Response** (200 OK)
```json
{
  "source": "SPOTIFY",
  "track": "Bohemian Rhapsody",
  "artist": "Queen",
  "album": "A Night at the Opera",
  "stationName": "My Playlist",
  "playStatus": "PLAY_STATE",
  "artUrl": "https://example.com/art.jpg"
}
```

### Get Volume

Returns volume information.

```http
GET /api/devices/{id}/volume
```

**Response** (200 OK)
```json
{
  "targetVolume": 45,
  "actualVolume": 45,
  "isMuted": false
}
```

### Set Volume

Sets the volume level.

```http
POST /api/devices/{id}/volume
Content-Type: application/json

{
  "level": 50
}
```

**Response**
- `204 No Content` - Successfully set

### Toggle Mute

Toggles the mute state of a device. If muted, unmutes; if unmuted, mutes.

```http
POST /api/devices/{id}/mute
```

**Request Body**
- No body required

**Response**
- `204 No Content` - Successfully toggled

**Error Responses**
- `404 Not Found` - Device not found
- `501 Not Implemented` - Device vendor does not support mute

**Notes**
- For SoundTouch devices, sends a MUTE key press via the `/key` endpoint
- Mute state can be retrieved via `GET /api/devices/{id}/volume` (check `isMuted` field)

### Send Remote Key Press

Sends a press-and-release gesture to the device. This is the API backing the remote controller grid in the UI.

```http
POST /api/devices/{id}/key
Content-Type: application/json

{
  "key": "PLAY_PAUSE"
}
```

**Supported key values**

| Key | Description |
|-----|-------------|
| `PREV_TRACK` | Skip to the previous track |
| `PLAY_PAUSE` | Toggle playback (always sends a toggle press) |
| `NEXT_TRACK` | Skip to the next track |
| `VOLUME_DOWN` | Decrease volume by one native SoundTouch step |
| `VOLUME_UP` | Increase volume by one native SoundTouch step |
| `AUX_INPUT` | Switch active source to AUX |

**Response**
- `200 OK` – Key was delivered to the adapter

**Error Responses**
- `400 Bad Request` – Missing `key` field or unsupported key value
- `404 Not Found` – Device ID not registered
- `501 Not Implemented` – Vendor adapter does not expose `/key`
- `503 Service Unavailable` – Device unreachable
- `504 Gateway Timeout` – Device did not acknowledge in time

**Notes**
- Requests are idempotent from the server perspective; each call still results in a physical button tap, so throttle appropriately.
- The backend automatically issues the required `press` + `release` XML payloads expected by SoundTouch hardware.

### Enter Bluetooth Pairing

Places a device into Bluetooth pairing mode (when supported).

```http
POST /api/devices/{id}/bluetooth/enter-pairing
```

**Response**
- `200 OK` – Pairing mode activated

**Error Responses**
- `400 Bad Request` – Device exists but is not flagged with the `bluetoothPairing` capability
- `404 Not Found` – Unknown device ID
- `501 Not Implemented` – Adapter does not implement Bluetooth pairing
- `503 Service Unavailable` – Device unreachable (offline or not responding)
- `504 Gateway Timeout` – Device failed to acknowledge pairing mode before timeout

**Notes**
- This endpoint does not return structured payloads; the UI shows confirmation text using the HTTP status alone.
- SoundTouch devices exit pairing mode automatically after ~60 seconds if no client connects.

---

## Presets

### List Presets

Returns the configured presets (slots 1-6) for a device, including metadata used by the UI.

```http
GET /api/devices/{id}/presets
```

**Response** (200 OK)
```json
[
  {
    "id": 1,
    "deviceId": "abc123",
    "name": "BBC Radio 1",
    "location": "https://stream.live.bbc.co.uk/",
    "iconUrl": "https://cdn.example.com/bbc.png",
    "type": "stationurl",
    "source": "LOCAL_INTERNET_RADIO",
    "isPresetable": true
  },
  {
    "id": 2,
    "deviceId": "abc123",
    "name": "Morning Playlist",
    "location": "spotify:playlist:123",
    "iconUrl": null,
    "type": "playlist",
    "source": "SPOTIFY",
    "isPresetable": true
  }
]
```

**Error Responses**
- `404 Not Found` - Device does not exist
- `501 Not Implemented` - Vendor does not expose preset APIs

### Store Preset (Create/Update)

Creates or updates a preset slot on the device. If the slot already holds a preset, it is overwritten.

```http
POST /api/devices/{id}/presets
Content-Type: application/json

{
  "id": 3,
  "name": "Evening Jazz",
  "location": "https://radio.example.com/jazz",
  "iconUrl": "https://cdn.example.com/jazz.png",
  "type": "stationurl",
  "source": "LOCAL_INTERNET_RADIO"
}
```

**Behavior**
- `id` must be between 1 and 6 for SoundTouch devices
- `name` and `location` are required
- `type` and `source` are optional; defaults are `stationurl` and `LOCAL_INTERNET_RADIO` when omitted (SoundTouch requirement)

**Response**
- `201 Created` - Preset stored successfully (body contains the stored preset)
- `200 OK` - Returned by some adapters when updating an existing slot

```json
{
  "id": 3,
  "deviceId": "abc123",
  "name": "Evening Jazz",
  "location": "https://radio.example.com/jazz",
  "iconUrl": "https://cdn.example.com/jazz.png",
  "type": "stationurl",
  "source": "LOCAL_INTERNET_RADIO",
  "isPresetable": true
}
```

**Error Responses**
- `400 Bad Request` - Invalid slot id, missing fields, or slot outside 1-6
- `404 Not Found` - Device not found
- `501 Not Implemented` - Vendor adapter missing

### Remove Preset

Deletes a preset from a slot.

```http
DELETE /api/devices/{id}/presets/{presetId}
```

**Response**
- `204 No Content` - Preset removed
- `404 Not Found` - Device missing or slot empty

**Error Responses**
- `400 Bad Request` - `presetId` outside 1-6
- `501 Not Implemented` - Vendor does not support preset removal

### Play Preset

Activates a preset. The backend powers on the device first when necessary.

```http
POST /api/devices/{id}/presets/{presetNumber}/play
```

**Response**
- `204 No Content` - Request accepted

**Error Responses**
- `400 Bad Request` - Invalid preset number
- `404 Not Found` - Device or preset absent
- `501 Not Implemented` - Vendor adapter missing

---

## Health Check

### Health Status

```http
GET /health
```

**Response** (200 OK)
```json
{
  "status": "Healthy"
}
```

---

## Error Codes

| Code | Description |
|------|-------------|
| `DEVICE_NOT_FOUND` | The specified device ID does not exist |
| `INVALID_INPUT` | Request body contains invalid data |
| `NOT_SUPPORTED` | The operation is not supported for this device/vendor |
| `DEVICE_UNREACHABLE` | Cannot connect to the device |
| `INTERNAL_ERROR` | An unexpected error occurred |

## Common Response Formats

**Success Response**
```json
{
  "data": { ... }
}
```

**Error Response**
```json
{
  "code": "ERROR_CODE",
  "message": "Human-readable error message"
}
```
