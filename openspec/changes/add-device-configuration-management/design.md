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

**Decision**: Add `NetworkMask` to devices.json root and extend device capabilities.

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
        "Port": 8090,
        "IsOnline": true,
        "PowerState": true,
        "Volume": 45,
        "LastSeen": "...",
        "Capabilities": ["power", "volume", "presets", "bluetoothPairing", "ping"],
        "IsNewlyDiscovered": false
      }
    ]
  }
}
```

**Rationale**: Network mask at root level makes it vendor-agnostic. `IsNewlyDiscovered` flag helps UI highlight new devices.

### Ping Implementation

**Decision**: Ping uses adapter-specific health check, not ICMP.

**Implementation**:
- SoundTouch: HTTP GET to `/info` endpoint with timeout
- Returns `{ reachable: boolean, latencyMs: number }`

**Rationale**: ICMP ping may be blocked; HTTP health check confirms device API is responsive.

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

**Decision**: New SoundTouch devices default to:
```
["power", "volume", "presets", "bluetoothPairing", "ping"]
```

**Rationale**: These are the capabilities supported by SoundTouch WebServices API.

### Port Handling

**Decision**: Port is vendor-specific and not user-editable.
- SoundTouch: Always 8090
- Future vendors define their own constants

**Rationale**: Users shouldn't need to know protocol details; vendor adapter handles port.

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
    "ipAddress": "192.168.1.150",
    "capabilities": ["power", "volume", "presets", "bluetoothPairing", "ping"]
  }
  Response: 201 Created with device object

PUT  /api/devices/{id}
  Request: (same as POST, partial updates allowed)
  Response: 200 OK with updated device

DELETE /api/devices/{id}
  Response: 204 No Content
```

### Vendor List Endpoint

```
GET /api/vendors
  Response: [
    { "id": "bose-soundtouch", "name": "Bose SoundTouch", "defaultPort": 8090 }
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

## Open Questions

1. Should we support hostname/FQDN resolution for device addresses? (Proposed: Yes, resolve on save)
2. Should discovered devices require confirmation before saving? (Proposed: Auto-save with highlight)
3. What timeout should ping use? (Proposed: 5 seconds)
