# Design: Device Configuration Management

## Context
SoundHub requires a full device configuration management solution. Users need to:
1. View, add, edit, and remove devices from the web UI
2. Discover devices on the local network using a configurable IP range
3. Verify device connectivity via ping
4. Have sensible defaults for device capabilities

The solution must integrate with the existing adapter pattern and devices.json persistence layer.

## Goals / Non-Goals

### Goals
- Provide complete CRUD operations for device configuration
- Enable network-based device discovery with user-defined IP range
- Support ping/connectivity verification per device
- Highlight newly discovered devices in the UI
- Persist network mask configuration in devices.json
- Apply sensible default capabilities for SoundTouch devices

### Non-Goals
- Automatic periodic discovery (manual trigger only)
- Device grouping or zones
- Real-time device status updates via WebSocket
- Multi-user access control

## Decisions

### Device Data Model Updates

**Decision**: Add `NetworkMask` to devices.json root. Simplify device schema to only store configuration data.

Updated devices.json structure:
```json
{
  "NetworkMask": "192.168.1.0/24",
  "SoundTouch": {
    "Devices": [
      {
        "Id": "...",
        "Vendor": "bose-soundtouch",
        "Name": "Living Room Speaker",
        "IpAddress": "192.168.1.197",
        "Capabilities": ["power", "volume", "presets", "bluetoothPairing", "ping"],
        "DateTimeAdded": "2025-12-31T12:00:00.000Z"
      }
    ]
  }
}
```

**Removed fields** (read on-demand from device or defined by adapter):
- `Port` - vendor-specific, defined in adapter (e.g., SoundTouch = 8090)
- `Volume` - read on-demand via device API
- `IsOnline` - read on-demand via device API
- `PowerState` - read on-demand via device API
- `LastSeen` - not persisted, can be tracked at runtime if needed

**Added fields**:
- `DateTimeAdded` - timestamp when device was added (static, never changes)

**Rationale**: Network mask at root level makes it vendor-agnostic. Device state is dynamic and should be queried from the device, not stored. UI can highlight newly added devices by comparing `DateTimeAdded` against current time (e.g., added within last 5 minutes).

### Ping Implementation

**Decision**: Ping uses adapter-specific audible notification, not ICMP.

**Implementation**:
- SoundTouch: HTTP GET to `/playNotification` endpoint
  - Pauses currently playing media
  - Emits a double beep sound on the device
  - Resumes media playback
- Returns `{ reachable: boolean, latencyMs: number }`

**API Reference**: [Play Notification Beep](https://github.com/thlucas1/homeassistantcomponent_soundtouchplus/wiki/SoundTouch-WebServices-API#play-notification-beep)

**Note**: Only devices with "ping" capability support `/playNotification`. Devices that don't support it should not have "ping" in their capabilities list.

**Rationale**: Audible feedback provides immediate physical confirmation that the device is reachable and responding, rather than just a silent API check.

### Discovery Process

**Decision**: Discovery iterates over IP range and probes each address for known vendor signatures.

**Algorithm**:
1. Parse network mask to get IP range
2. For each IP in range, probe vendor-specific endpoints (e.g., SoundTouch `/info` on port 8090)
3. If response matches vendor signature, create device record
4. Skip IPs that already have a configured device (by IP match)
5. Mark new devices with `IsNewlyDiscovered: true`

**Rationale**: Vendor-specific probing ensures accurate device detection. Existing devices are preserved.

### Default Capabilities for SoundTouch

**Decision**: Base capabilities are static, additional capabilities are determined dynamically.

**Base capabilities** (always present):
```
["power", "volume"]
```

**Dynamic capabilities** (queried from `/supportedUrls`):
| URL in supportedUrls | Capability added |
|---------------------|------------------|
| `/presets` | "presets" |
| `/enterBluetoothPairing` | "bluetoothPairing" |
| `/playNotification` | "ping" |

**API Reference**: [SupportedURLs](https://github.com/thlucas1/homeassistantcomponent_soundtouchplus/wiki/SoundTouch-WebServices-API#supportedurls)

**Rationale**: Different SoundTouch models have different capabilities. Querying `/supportedUrls` ensures accurate capability detection per device.

### Port Handling

**Decision**: Port is vendor-specific, defined as a constant in each adapter, and not stored in devices.json.
- SoundTouch: 8090 (constant in `SoundTouchAdapter`)
- Future vendors define their own constants in their adapters

**Rationale**: Port is a protocol detail that users shouldn't need to configure. Keeping it in the adapter simplifies the device schema and prevents misconfiguration.

## API Design

### New Endpoints

```
GET  /api/devices/{id}/ping
  Response: { "reachable": true, "latencyMs": 45 }

POST /api/devices/discover
  Request: (empty, uses configured NetworkMask)
  Response: { "discovered": 3, "new": 2, "devices": [...] }

GET  /api/config/network-mask
  Response: { "networkMask": "192.168.1.0/24" }

PUT  /api/config/network-mask
  Request: { "networkMask": "192.168.1.0/24" }
  Response: 204 No Content

POST /api/devices
  Request: { 
    "vendor": "bose-soundtouch",
    "name": "Kitchen Speaker",
    "ipAddress": "192.168.1.150"
  }
  Response: 201 Created with device object (capabilities auto-detected)

PUT  /api/devices/{id}
  Request: { "name": "...", "ipAddress": "...", "capabilities": [...] }
  Response: 200 OK with updated device

DELETE /api/devices/{id}
  Response: 204 No Content
```

### Vendor List Endpoint

```
GET /api/vendors
  Response: [
    { "id": "bose-soundtouch", "name": "Bose SoundTouch" }
  ]
```

## UI Design

### Device Configuration Page

Layout:
```
+----------------------------------+
| Device Configuration       [+]   |
+----------------------------------+
| Network Mask: [192.168.1.0/24]   |
| [Discover Devices]               |
+----------------------------------+
| Device List:                     |
| +------------------------------+ |
| | Living Room Speaker          | |
| | bose-soundtouch     [Ping]   | |
| +------------------------------+ |
| | * NEW: Kitchen Speaker       | |   <- highlighted
| | bose-soundtouch     [Ping]   | |
| +------------------------------+ |
+----------------------------------+
```

### Add/Edit Device Form

```
+----------------------------------+
| Add Device                       |
+----------------------------------+
| Name:        [                 ] |
| IP/FQDN:     [                 ] |
| Vendor:      [bose-soundtouch ▼] |
| Type:        [                 ] |
| Capabilities:                    |
|   [x] power                      |
|   [x] volume                     |
|   [x] presets                    |
|   [x] bluetoothPairing           |
|   [x] ping                       |
+----------------------------------+
| [Cancel]              [Save]     |
+----------------------------------+
```

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| Discovery may take long on large networks | Limit range to /24; show progress; allow cancellation |
| False positives during discovery | Validate device signature (e.g., check XML response format) |
| User enters invalid network mask | Validate format on frontend and backend |
| Concurrent discovery requests | Disable button during discovery; reject if already running |

## Migration Plan

1. Update devices.json schema to include `NetworkMask` field (optional, default: empty)
2. Add `IsNewlyDiscovered` field to device records
3. Existing devices.json files remain valid (backward compatible)
4. No data migration required

## Resolved Questions

1. **Hostname/FQDN resolution**: Yes, supported. Resolved to IP address on save.
2. **Discovery confirmation**: No confirmation required. Auto-save with highlight for 5 minutes.
3. **Ping timeout**: 10 seconds, configurable in application settings (appsettings.json).
