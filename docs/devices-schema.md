# devices.json Schema Documentation

This document describes the structure and schema of the `devices.json` configuration file used by SoundHub to store device metadata and discovery settings.

## File Location

- Default: `/data/devices.json` (Docker)
- Configurable via `FileDeviceRepository:FilePath` in `appsettings.json`

## Schema Overview

```json
{
  "NetworkMask": "192.168.1.0/24",
  "SoundTouch": {
    "Devices": [
      {
        "Id": "uuid-string",
        "Vendor": "bose-soundtouch",
        "Name": "Living Room Speaker",
        "IpAddress": "192.168.1.100",
        "Capabilities": ["power", "volume", "presets", "ping"],
        "DateTimeAdded": "2025-12-31T12:00:00.000Z"
      }
    ]
  }
}
```

## Root Properties

### NetworkMask

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `NetworkMask` | `string` | No | CIDR notation for device discovery range |

**Format:** `<ip-address>/<prefix-length>`

**Examples:**
- `192.168.1.0/24` - Scans 192.168.1.1 to 192.168.1.254
- `10.0.0.0/8` - Scans entire 10.x.x.x range
- `172.16.0.0/12` - Private network range

**Notes:**
- If not set, discovery uses auto-detected local subnet
- Larger ranges (smaller prefix) take longer to scan

## Vendor Groups

Devices are organized by vendor into separate groups. The group key corresponds to the vendor display name.

### SoundTouch Group

Contains all Bose SoundTouch devices.

```json
{
  "SoundTouch": {
    "Devices": [...]
  }
}
```

## Device Properties

### Id

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Id` | `string` | Yes | Unique identifier (UUID) |

**Generated:** Automatically on device creation

### Vendor

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Vendor` | `string` | Yes | Vendor identifier |

**Supported Values:**
- `bose-soundtouch` - Bose SoundTouch speakers

### Name

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Name` | `string` | Yes | User-friendly device name |

**Notes:**
- Displayed in the UI
- Editable by the user
- Not read from device (user-defined)

### IpAddress

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `IpAddress` | `string` | Yes | Device IP address or resolved FQDN |

**Format:** IPv4 address (e.g., `192.168.1.100`)

**Notes:**
- FQDN hostnames are resolved to IP on save
- Used to communicate with the device
- Should be static/reserved in your router

### Capabilities

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Capabilities` | `string[]` | No | List of supported device capabilities |

**Available Capabilities:**
| Capability | Description |
|------------|-------------|
| `power` | Power on/off control (all devices) |
| `volume` | Volume control (all devices) |
| `presets` | Preset playback (determined by `/supportedUrls`) |
| `bluetoothPairing` | Bluetooth pairing mode (determined by `/supportedUrls`) |
| `ping` | Audible ping via `/playNotification` (determined by `/supportedUrls`) |

**Notes:**
- Base capabilities (`power`, `volume`) are always present
- Additional capabilities are queried from device's `/supportedUrls` endpoint on add
- Can be manually edited by the user

### DateTimeAdded

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `DateTimeAdded` | `string` | No | ISO 8601 timestamp when device was added |

**Format:** ISO 8601 (e.g., `2025-12-31T12:00:00.000Z`)

**Notes:**
- Set automatically on device creation
- Never changes after creation
- Used by UI to highlight newly added devices (within 5 minutes)

## Removed Properties (v2.0+)

The following properties are no longer stored in devices.json:

| Property | Reason | Alternative |
|----------|--------|-------------|
| `Port` | Vendor-specific constant | Defined in adapter (SoundTouch = 8090) |
| `Volume` | Dynamic state | Read on-demand via `/volume` endpoint |
| `IsOnline` | Dynamic state | Read on-demand via status check |
| `PowerState` | Dynamic state | Read on-demand via `/nowPlaying` |
| `LastSeen` | Transient data | Not persisted |

## Example: Empty Configuration

```json
{}
```

## Example: Single Device

```json
{
  "NetworkMask": "192.168.1.0/24",
  "SoundTouch": {
    "Devices": [
      {
        "Id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "Vendor": "bose-soundtouch",
        "Name": "Living Room Speaker",
        "IpAddress": "192.168.1.100",
        "Capabilities": ["power", "volume", "presets", "bluetoothPairing", "ping"],
        "DateTimeAdded": "2025-12-31T12:00:00.000Z"
      }
    ]
  }
}
```

## Example: Multiple Devices

```json
{
  "NetworkMask": "192.168.1.0/24",
  "SoundTouch": {
    "Devices": [
      {
        "Id": "device-1",
        "Vendor": "bose-soundtouch",
        "Name": "Living Room Speaker",
        "IpAddress": "192.168.1.100",
        "Capabilities": ["power", "volume", "presets", "ping"],
        "DateTimeAdded": "2025-12-31T12:00:00.000Z"
      },
      {
        "Id": "device-2",
        "Vendor": "bose-soundtouch",
        "Name": "Kitchen Speaker",
        "IpAddress": "192.168.1.101",
        "Capabilities": ["power", "volume"],
        "DateTimeAdded": "2025-12-31T14:30:00.000Z"
      },
      {
        "Id": "device-3",
        "Vendor": "bose-soundtouch",
        "Name": "Bedroom Speaker",
        "IpAddress": "192.168.1.102",
        "Capabilities": ["power", "volume", "presets", "bluetoothPairing", "ping"],
        "DateTimeAdded": "2025-12-31T16:45:00.000Z"
      }
    ]
  }
}
```

## File Permissions

- The API needs read/write access to the file
- In Docker: mounted as a volume at `/data/devices.json`
- Ensure the directory exists before starting the application

## Hot Reload

The file is monitored for changes. External edits to `devices.json` are automatically detected and reloaded by the application.

**Configuration:**
```json
{
  "FileDeviceRepository": {
    "FilePath": "/data/devices.json",
    "EnableHotReload": true
  }
}
```
