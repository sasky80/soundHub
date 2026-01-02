# Design Document: Now Playing LCD Display

## Overview
Add a retro-style LCD display to the device control panel that shows the currently playing media information. The display uses a scrolling marquee animation for long text and provides real-time feedback through polling the device status every 10 seconds.

## Layout

### Position in Control Panel
```
┌─────────────────────────────────┐
│         CONTROL PANEL           │
└─────────────────────────────────┘
       ← Back
       Device Name
       Device Vendor (e.g., SoundTouch)
┌─────────────────────────────────┐
│ ▓▓▓ Radio Station: Artist, Track│ ← LCD Display (NEW)
└─────────────────────────────────┘

Power: [⏻] ON/OFF
... rest of controls ...
```

## LCD Display Specifications

### Visual Design
- **Background**: Dark charcoal/black (#1a1a1a) with subtle texture
- **Text color**: Configurable theme (default: green)
  - **Green**: Phosphor green (#39ff14) - classic terminal look
  - **Amber**: Warm amber (#ffbf00) - vintage display style
  - **Blue**: Cool blue (#00d4ff) - modern tech aesthetic
- **Font**: Monospace font (e.g., 'VT323', 'Press Start 2P', or fallback to system monospace)
- **Border**: Subtle inset shadow to simulate recessed display
- **Container**: Rounded corners (4-6px), slight inner shadow

### Dimensions
- **Width**: 100% of device info section
- **Height**: ~40-48px (single line)
- **Padding**: 8-12px horizontal, 4-8px vertical

### Text Format
```
{StationName}: {Artist}, {Track}
```

**Fallback scenarios:**
- No station: `{Artist} - {Track}`
- No artist: `{StationName}: {Track}`
- No track: `{StationName}: {Artist}`
- Only station: `{StationName}`
- Nothing playing: `---` or empty display

## Scrolling Animation

### CSS Marquee Implementation
```scss
.lcd-display {
  overflow: hidden;
  white-space: nowrap;
  position: relative;
  
  .lcd-text {
    display: inline-block;
    padding-left: 100%; // Start off-screen right
    animation: marquee 15s linear infinite;
    
    &.no-scroll {
      padding-left: 0;
      animation: none;
    }
  }
}

@keyframes marquee {
  0% {
    transform: translateX(0);
  }
  100% {
    transform: translateX(-100%);
  }
}
```

### Animation Behavior
- **Speed**: Configurable via settings
  - **Slow**: ~20 seconds for full scroll cycle
  - **Medium**: ~15 seconds (default)
  - **Fast**: ~8 seconds for full scroll cycle
- **Direction**: Right to left
- **Trigger**: Only when text exceeds container width
- **Pause**: Optional pause on hover for readability
- **Gap**: Add spacing between repeated text cycles

### CSS Custom Properties for Theming
```scss
:root {
  // Scroll speed
  --lcd-scroll-duration: 15s; // slow: 20s, medium: 15s, fast: 8s
  
  // Color themes
  --lcd-text-color: #39ff14; // green (default)
  --lcd-glow-color: rgba(57, 255, 20, 0.5);
  
  // Amber theme
  &[data-lcd-theme="amber"] {
    --lcd-text-color: #ffbf00;
    --lcd-glow-color: rgba(255, 191, 0, 0.5);
  }
  
  // Blue theme
  &[data-lcd-theme="blue"] {
    --lcd-text-color: #00d4ff;
    --lcd-glow-color: rgba(0, 212, 255, 0.5);
  }
}
```

## Polling Implementation

### RxJS Polling Strategy
```typescript
private statusPolling$ = new Subject<void>();
private readonly POLL_INTERVAL = 10000; // 10 seconds

private startPolling(deviceId: string): void {
  interval(this.POLL_INTERVAL)
    .pipe(
      startWith(0), // Immediate first poll
      switchMap(() => this.deviceService.getDeviceStatus(deviceId)),
      takeUntil(this.statusPolling$),
      takeUntil(this.destroy$)
    )
    .subscribe({
      next: (status) => this.updateStatus(status),
      error: (err) => console.error('Polling error:', err)
    });
}

private stopPolling(): void {
  this.statusPolling$.next();
}
```

### Polling Lifecycle
1. Start polling when component initializes and device is reachable
2. Continue polling while device is powered on
3. Stop polling when device powers off
4. Restart polling when device powers back on
5. Clean up on component destroy

## Button State Management

### Source-Based Toggle States
```typescript
// Derived from polled status.currentSource
protected readonly isBluetoothActive = computed(() => 
  this.status()?.currentSource === 'BLUETOOTH'
);

protected readonly isAuxActive = computed(() => 
  this.status()?.currentSource === 'AUX' || 
  this.status()?.currentSource === 'AUX_INPUT'
);
```

### Bluetooth Button Changes
**Before:**
- Shows loading → displays "Bluetooth pairing started" message

**After:**
- Shows loading during pairing request
- Active styling when source is BLUETOOTH
- No message display after pairing (device will switch source if successful)

### AUX Button Changes
**Before:**
- Manual `activeSource.set('AUX_INPUT')` on button click

**After:**
- Active state derived from `status().currentSource`
- No manual state management needed

## Data Structures

### Extended DeviceStatus Interface
```typescript
export interface DeviceStatus {
  isOnline: boolean;
  powerState: boolean;
  volume: number;
  currentSource?: string; // 'BLUETOOTH' | 'AUX' | 'LOCAL_INTERNET_RADIO' | etc.
  nowPlaying?: NowPlayingInfo;
}

export interface NowPlayingInfo {
  stationName?: string;
  artist?: string;
  track?: string;
  album?: string; // Optional, for future use
  artUrl?: string; // Optional, for future use
}
```

### API Response Example
```json
{
  "isOnline": true,
  "powerState": true,
  "volume": 45,
  "currentSource": "LOCAL_INTERNET_RADIO",
  "nowPlaying": {
    "stationName": "Jazz FM",
    "artist": "Miles Davis",
    "track": "So What"
  }
}
```

## Accessibility

### LCD Display
- `role="status"` for live region updates
- `aria-live="polite"` for screen reader announcements
- `aria-label="Now playing information"`
- High contrast text (green/amber on black meets WCAG)

### Button States
- `aria-pressed="true/false"` for toggle buttons
- Clear focus indicators maintained
- Tooltips explain current state

## Performance Considerations

### Polling Efficiency
- Use `switchMap` to cancel in-flight requests on new poll
- Consider exponential backoff on repeated errors
- Stop polling when tab is not visible (Page Visibility API)

### Animation Performance
- Use `transform` for GPU-accelerated scrolling
- `will-change: transform` for optimization
- Pause animation when not visible

## Future Enhancements

### Potential Additions
- Album art thumbnail in LCD display
- Progress bar for track position
- Larger touch target for mobile
- Additional color themes (red, purple, white)
