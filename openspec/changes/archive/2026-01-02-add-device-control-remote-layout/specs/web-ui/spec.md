# web-ui Specification Delta

## MODIFIED Requirements

### Requirement: Device details page header
The system SHALL display "Control panel" as the header instead of "Device Details".

#### Scenario: Display control panel header
- **GIVEN** the user navigates to the device details page
- **WHEN** the page loads
- **THEN** the header displays "Control panel"

## ADDED Requirements

### Requirement: Volume control displays icon instead of text label
The system SHALL display a volume icon on the left side of the volume slider instead of a text label.

#### Scenario: Volume icon displayed
- **GIVEN** the user is on the control panel page
- **WHEN** the volume section is rendered
- **THEN** a volume icon (speaker symbol) is displayed on the left side of the slider
- **AND** no "Volume" text label is shown

### Requirement: Remote controller button layout
The system SHALL display control buttons in a grid layout resembling a physical remote controller, using icon-only buttons without text labels.

#### Scenario: Remote layout grid structure
- **GIVEN** the user is on the control panel page
- **WHEN** the control buttons section is rendered
- **THEN** buttons are arranged in a CSS grid layout
- **AND** buttons use icons only (no text labels on buttons)
- **AND** buttons have aria-labels for accessibility
- **AND** the layout adapts responsively to screen size

### Requirement: Playback control buttons
The system SHALL provide icon-based buttons for controlling media playback: previous track, next track, and play/pause toggle.

#### Scenario: Display playback controls
- **GIVEN** the user is on the control panel page
- **WHEN** the control section is rendered
- **THEN** the UI displays three playback buttons with icons:
  - Previous Track button with backward icon (⏮)
  - Next Track button with forward icon (⏭)
  - Play/Pause toggle button with dynamic icon (▶ when paused, ⏸ when playing)
- **AND** each button has an accessible label

#### Scenario: Display Play icon when device is paused or stopped
- **GIVEN** the user is on the control panel page
- **AND** the device playback state is paused or stopped
- **WHEN** the control section is rendered
- **THEN** the Play/Pause button displays a play icon (▶)
- **AND** the button has aria-label "Play"

#### Scenario: Display Pause icon when device is playing
- **GIVEN** the user is on the control panel page
- **AND** the device playback state is playing
- **WHEN** the control section is rendered
- **THEN** the Play/Pause button displays a pause icon (⏸)
- **AND** the button has aria-label "Pause"

#### Scenario: Trigger previous track
- **GIVEN** the user is on the control panel page
- **WHEN** the user clicks the Previous Track button
- **THEN** the system calls `POST /api/devices/{id}/key` with key `PREV_TRACK`
- **AND** the button shows a loading state during the request

#### Scenario: Trigger next track
- **GIVEN** the user is on the control panel page
- **WHEN** the user clicks the Next Track button
- **THEN** the system calls `POST /api/devices/{id}/key` with key `NEXT_TRACK`
- **AND** the button shows a loading state during the request

#### Scenario: Trigger play/pause toggle
- **GIVEN** the user is on the control panel page
- **WHEN** the user clicks the Play/Pause button
- **THEN** the system calls `POST /api/devices/{id}/key` with key `PLAY_PAUSE`
- **AND** the button shows a loading state during the request
- **AND** the button icon toggles after successful response

### Requirement: Volume adjustment buttons
The system SHALL provide icon-based buttons for volume up and volume down operations.

#### Scenario: Display volume buttons
- **GIVEN** the user is on the control panel page
- **WHEN** the control section is rendered
- **THEN** the UI displays volume up button with icon (🔊+ or speaker-plus)
- **AND** the UI displays volume down button with icon (🔉- or speaker-minus)
- **AND** each button has an accessible label

#### Scenario: Trigger volume up
- **GIVEN** the user is on the control panel page
- **AND** the device is powered on
- **WHEN** the user clicks the Volume Up button
- **THEN** the system calls `POST /api/devices/{id}/key` with key `VOLUME_UP`
- **AND** the button shows a loading state during the request

#### Scenario: Trigger volume down
- **GIVEN** the user is on the control panel page
- **AND** the device is powered on
- **WHEN** the user clicks the Volume Down button
- **THEN** the system calls `POST /api/devices/{id}/key` with key `VOLUME_DOWN`
- **AND** the button shows a loading state during the request

#### Scenario: Volume buttons disabled when device off
- **GIVEN** the device is powered off
- **WHEN** the control panel is displayed
- **THEN** volume up and down buttons are disabled

### Requirement: AUX input button
The system SHALL provide an icon-based button to switch the device source to AUX input.

#### Scenario: Display AUX button
- **GIVEN** the user is on the control panel page
- **WHEN** the control section is rendered
- **THEN** the UI displays an AUX button with text/icon (e.g., "AUX" or cable icon)
- **AND** the button has an accessible label

#### Scenario: Trigger AUX input switch
- **GIVEN** the user is on the control panel page
- **WHEN** the user clicks the AUX button
- **THEN** the system calls `POST /api/devices/{id}/key` with key `AUX_INPUT`
- **AND** the button shows a loading state during the request

### Requirement: Bluetooth pairing button
The system SHALL provide a Bluetooth pairing button that is visible only if the device supports the "bluetoothPairing" capability.

#### Scenario: Display Bluetooth button if capability exists
- **GIVEN** the user is on the control panel page
- **AND** the device has "bluetoothPairing" capability
- **WHEN** the control section is rendered
- **THEN** the UI displays a Bluetooth button with Bluetooth icon
- **AND** the button has an accessible label

#### Scenario: Hide Bluetooth button if capability not present
- **GIVEN** the user is on the control panel page
- **AND** the device does not have "bluetoothPairing" capability
- **WHEN** the control section is rendered
- **THEN** the Bluetooth button is not displayed

#### Scenario: Trigger Bluetooth pairing mode
- **GIVEN** the user is on the control panel page
- **AND** the Bluetooth button is visible
- **WHEN** the user clicks the Bluetooth button
- **THEN** the system calls `POST /api/devices/{id}/bluetooth/enter-pairing`
- **AND** the button shows a loading state during the request
- **AND** the system displays a confirmation message on success

#### Scenario: Bluetooth pairing tooltip
- **GIVEN** the user is on the control panel page
- **AND** the Bluetooth button is visible
- **WHEN** the user hovers over the Bluetooth button
- **THEN** a tooltip explains "Enter Bluetooth pairing mode"

### Requirement: Control buttons disabled when device off
The system SHALL disable all control buttons (except power button) when the device is powered off.

#### Scenario: Buttons disabled in standby
- **GIVEN** the device is powered off (standby mode)
- **WHEN** the control panel is displayed
- **THEN** all playback, volume, AUX, and Bluetooth buttons are disabled
- **AND** the power button remains enabled
- **AND** tooltips explain "Device must be powered on"
