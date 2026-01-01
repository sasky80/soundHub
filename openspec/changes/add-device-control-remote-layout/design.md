# Design Document: Remote Controller Layout

## Overview
This document describes the visual design and layout of the remote controller interface for the Control Panel page.

## Layout Philosophy
The remote controller layout mimics a physical remote control, with buttons arranged in a logical grid that prioritizes:
1. **Familiarity**: Users recognize the layout from physical remote controls
2. **Ergonomics**: Frequently used buttons (play/pause) are centrally positioned
3. **Visual hierarchy**: Button groupings (playback, volume, sources) are clearly separated
4. **Accessibility**: All buttons have proper ARIA labels and keyboard navigation support

## Grid Layout

### Desktop/Tablet Layout (min-width: 768px)
```
┌─────────────────────────────────┐
│         CONTROL PANEL           │
└─────────────────────────────────┘

Power: [⏻] ON/OFF

┌─────────────────────────────────┐
│   🔊  [═══════════○═══] [MUTE]  │  ← Volume Section
└─────────────────────────────────┘

┌─────────────────────────────────┐
│           [ ⏮ ] [▶⏸] [ ⏭ ]      │  ← Playback Controls
│                                  │
│           [ 🔉- ] [ 🔊+ ]        │  ← Volume Buttons
│                                  │
│           [AUX]  [🔵 BT]         │  ← Source Controls
└─────────────────────────────────┘

┌─────────────────────────────────┐
│           PRESETS               │
│   [P1] [P2] [P3] [P4] [P5] [+]  │
└─────────────────────────────────┘
```

### Mobile Layout (max-width: 767px)
```
┌──────────────────┐
│  CONTROL PANEL   │
└──────────────────┘

Power: [⏻] ON

┌──────────────────┐
│ 🔊 [═══○═] [🔇]  │
└──────────────────┘

┌──────────────────┐
│   [ ⏮ ][▶⏸][ ⏭ ]│
│                  │
│   [ 🔉-][🔊+]    │
│                  │
│   [AUX][🔵 BT]   │
└──────────────────┘

┌──────────────────┐
│     PRESETS      │
│ [P1][P2][P3][+]  │
└──────────────────┘
```

## Button Specifications

### 1. Power Button
- **Icon**: Power symbol (⏻)
- **Size**: 48x48px
- **State indicators**: Color change (green=ON, gray=OFF)
- **Position**: Top of control section, separate from grid

### 2. Volume Section
- **Icon**: Speaker symbol (🔊) on the left
- **Slider**: Standard HTML range input (0-100)
- **Mute button**: Toggle button with crossed speaker icon when muted
- **Layout**: Horizontal flex layout: `[Icon] [Slider] [Mute Button]`

### 3. Playback Control Buttons (Central Group)
- **Button size**: 56x56px (larger, central buttons)
- **Icons**:
  - Previous Track: ⏮ (or SVG backward-skip)
  - Next Track: ⏭ (or SVG forward-skip)
  - Play/Pause: ▶ (when paused/stopped) or ⏸ (when playing) - toggles dynamically
- **Layout**:
  ```
  [PREV] [PLAY/PAUSE] [NEXT]
  ```
- **CSS Grid**:
  ```css
  grid-template-columns: repeat(3, 1fr);
  gap: 0.75rem;
  
  .prev-btn { grid-column: 1; }
  .play-pause-btn { grid-column: 2; }
  .next-btn { grid-column: 3; }
  ```

### 4. Volume Adjustment Buttons
- **Button size**: 48x48px
- **Icons**:
  - Volume Down: 🔉- (speaker with minus)
  - Volume Up: 🔊+ (speaker with plus)
- **Layout**: Horizontal pair below playback controls
  ```
  [VOL-] [VOL+]
  ```

### 5. Source Control Buttons
- **Button size**: 48x48px
- **Icons**:
  - AUX: "AUX" text or cable icon
  - Bluetooth: 🔵 Bluetooth symbol
- **Layout**: Horizontal pair below volume buttons
  ```
  [AUX] [BT]
  ```
- **Conditional rendering**: Bluetooth button only shown if device has `bluetoothPairing` capability

## Visual States

### Button States
1. **Normal**: Default appearance, slight shadow
2. **Hover**: Background color lightens by 10%, cursor changes to pointer
3. **Active/Pressed**: Background darkens, slight scale transform (0.95)
4. **Disabled**: Opacity 0.5, cursor not-allowed, no hover effects
5. **Loading**: Spinner overlay, button content dimmed

### Color Scheme (Dark Theme)
- **Background**: `#1e1e1e` (dark gray)
- **Button background**: `#2a2a2a` (slightly lighter gray)
- **Button hover**: `#3a3a3a`
- **Button active**: `#1a1a1a`
- **Primary accent**: `#4a9eff` (blue - for Bluetooth, active states)
- **Icon color**: `#ffffff` (white)
- **Disabled text**: `#888888` (medium gray)
- **Border**: `1px solid #444` (subtle border on buttons)

## Spacing and Padding
- **Grid gap**: 0.75rem (12px) between buttons
- **Section margin**: 1.5rem (24px) between sections
- **Button padding**: 0.75rem (12px) internal padding
- **Border radius**: 8px for buttons, 12px for section containers

## Accessibility Requirements

### ARIA Labels
- All icon-only buttons MUST have `aria-label` attributes
- Example: `<button aria-label="Play media">▶</button>`
- Disabled buttons should include disabled state in label: `aria-label="Volume up (device must be powered on)"`

### Keyboard Navigation
- Tab order: Power → Volume slider → Mute → Prev → Play → Next → Pause → Vol- → Vol+ → AUX → BT → Presets
- Enter/Space to activate buttons
- Arrow keys for volume slider
- Focus indicators: 2px solid outline on focused button

### Screen Reader Support
- Button groups wrapped in `<section>` with `aria-labelledby`
- Loading states announced: `aria-live="polite"` region for status updates
- Tooltips with descriptive text for icon-only buttons

## Responsive Breakpoints

### Mobile (<768px)
- Reduce button size to 44x44px (minimum touch target)
- Stack sections vertically
- Full-width grid layout
- Larger gaps (1rem) for easier touch targeting

### Tablet (768px - 1023px)
- Button size: 48x48px
- Two-column layout for source buttons if space allows

### Desktop (≥1024px)
- Button size: 56x56px for playback, 48x48px for others
- Three-column layout for playback controls
- Optional: Side-by-side preset grid with more columns

## Implementation Notes

### CSS Grid Structure
```scss
.remote-control-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  grid-template-rows: auto;
  gap: 0.75rem;
  max-width: 400px;
  margin: 0 auto;
  padding: 1.5rem;
  background: var(--surface-color, #1e1e1e);
  border-radius: 12px;

  // Playback controls
  .playback-group {
    grid-column: 1 / -1;
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 0.75rem;
  }

  // Volume buttons
  .volume-buttons {
    grid-column: 1 / -1;
    display: flex;
    justify-content: center;
    gap: 1rem;
  }

  // Source buttons
  .source-buttons {
    grid-column: 1 / -1;
    display: flex;
    justify-content: center;
    gap: 1rem;
  }
}
```

### Icon Resources
- Use SVG icons for scalability and theming
- Fallback to Unicode characters if SVG unavailable
- Icon library suggestions: Material Icons, Font Awesome, or custom SVG set
- Store icons in `/assets/icons/` directory

### Loading States
- Show spinner overlay on button during API request
- Disable all other control buttons while one operation is in progress
- Display subtle pulse animation to indicate activity
- Clear loading state after 5 seconds if no response (timeout)

## Future Enhancements
- Customizable button layout (user preferences)
- Haptic feedback on mobile devices
- Gesture support (swipe for volume, double-tap for quick actions)
- Persistent button state indicators (e.g., highlight AUX when active source)
- Keyboard shortcuts (e.g., Space = Play/Pause, Arrow keys = Prev/Next)
