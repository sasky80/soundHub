# Design: SoundTouch Extended Device Control

## Overview
This document describes the technical design for implementing real SoundTouch WebServices API calls in the `SoundTouchAdapter`. The adapter communicates with SoundTouch devices over HTTP on port 8090 using XML payloads.

## Architecture

### Communication Pattern
```
┌────────────────┐     ┌──────────────────────┐     ┌─────────────────┐
│  DeviceService │ ──▶ │  SoundTouchAdapter   │ ──▶ │ SoundTouch      │
│                │     │  (IDeviceAdapter)    │     │ Device :8090    │
└────────────────┘     └──────────────────────┘     └─────────────────┘
                              │
                              ▼
                       HttpClient + XML parsing
```

### SoundTouch API Endpoints Used

| Feature              | HTTP Method | Endpoint                  | Notes                                      |
|----------------------|-------------|---------------------------|--------------------------------------------|
| Device Info          | GET         | `/info`                   | Returns device name, type, MAC, firmware   |
| Now Playing Status   | GET         | `/nowPlaying`             | Current source, track, artist, play state  |
| Volume Get           | GET         | `/volume`                 | Returns targetvolume, actualvolume, mute   |
| Volume Set           | POST        | `/volume`                 | Body: `<volume>{level}</volume>`           |
| Power Toggle         | POST        | `/key`                    | Body: `<key state="press" sender="Gabbo">POWER</key>` |
| Power Off (Standby)  | GET         | `/standby`                | Puts device into standby mode              |
| Bluetooth Pairing    | GET         | `/enterBluetoothPairing`  | Enters BT pairing mode                     |
| Get Presets          | GET         | `/presets`                | Returns up to 6 configured presets         |
| Play Preset          | POST        | `/key`                    | Body: `<key state="press" sender="Gabbo">PRESET_N</key>` |

### Key Press/Release Pattern
SoundTouch `/key` endpoint requires two calls:
1. **Press**: `<key state="press" sender="Gabbo">{KEY}</key>`
2. **Release**: `<key state="release" sender="Gabbo">{KEY}</key>`

A small delay (~100ms) is recommended between press and release.

### XML Response Parsing
All SoundTouch responses are XML. Use `System.Xml.Linq.XDocument` for parsing:

```csharp
var doc = XDocument.Parse(xmlContent);
var name = doc.Root?.Element("name")?.Value;
```

## Data Models

### DeviceInfo (new domain entity)
Extends device information beyond basic `Device` entity:
```csharp
public class DeviceInfo
{
    public string DeviceId { get; init; }      // From deviceID attribute
    public string Name { get; init; }          // From <name>
    public string Type { get; init; }          // From <type> (e.g., "SoundTouch 10")
    public string MacAddress { get; init; }    // From <networkInfo>
    public string IpAddress { get; init; }     // From <networkInfo>
    public string SoftwareVersion { get; init; }// From <components>
}
```

### NowPlayingInfo (new DTO)
Captures current playback state:
```csharp
public class NowPlayingInfo
{
    public string Source { get; init; }        // TUNEIN, SPOTIFY, BLUETOOTH, etc.
    public string? Track { get; init; }
    public string? Artist { get; init; }
    public string? Album { get; init; }
    public string? StationName { get; init; }
    public string PlayStatus { get; init; }    // PLAY_STATE, PAUSE_STATE, etc.
}
```

### VolumeInfo (new DTO)
```csharp
public class VolumeInfo
{
    public int TargetVolume { get; init; }
    public int ActualVolume { get; init; }
    public bool IsMuted { get; init; }
}
```

### SoundTouchPreset (internal DTO for parsing)
Maps to XML preset structure:
```xml
<preset id="1">
  <ContentItem source="TUNEIN" type="stationurl" location="..." isPresetable="true">
    <itemName>K-LOVE Radio</itemName>
    <containerArt>...</containerArt>
  </ContentItem>
</preset>
```

## Implementation Approach

### HttpClient Configuration
- Named client: `"SoundTouch"`
- Timeout: 10 seconds (devices on slow networks may respond slowly)
- No authentication required (LAN-only access)

### Error Handling Strategy
1. **Device unreachable**: Catch `HttpRequestException`, mark device as offline
2. **Invalid response**: Catch XML parse exceptions, log and return fallback
3. **Timeouts**: Handle `TaskCanceledException`, retry once before failing
4. **HTTP 4xx/5xx**: Log error, throw meaningful exception to caller

### Thread Safety
- `HttpClient` is thread-safe and should be reused
- Each method call is independent; no shared mutable state

## Security Considerations
- SoundTouch API has no authentication; relies on LAN isolation
- Do not expose device IP addresses in public API responses
- Log device operations with device ID, not IP addresses

## Testing Strategy
- Unit tests: Mock `IHttpClientFactory` to return fake responses
- Integration tests: Optional, against real device (manual verification)
- Use captured XML samples from API documentation for test fixtures

## Decisions

### Why XDocument over XmlDocument?
- LINQ-to-XML provides cleaner query syntax
- Better null handling with `?.` operators
- Lighter memory footprint for small payloads

### Why POST for key presses instead of GET?
- SoundTouch API requires POST with XML body for `/key` endpoint
- GET is used for `/standby` as it's a simple trigger

### Why separate press/release calls?
- SoundTouch hardware simulates physical remote button behavior
- Some operations (like volume hold) require distinguishing press duration
- For simple key presses, immediate release after press is standard pattern
