# web-ui Specification Delta

## ADDED Requirements

### Requirement: Now Playing LCD display
The system SHALL display a retro-style LCD panel showing the currently playing media information in the device info section, visible only when the device is powered on.

#### Scenario: Display Now Playing when device is on and playing
- **GIVEN** the user is on the control panel page
- **AND** the device is powered on
- **AND** the device is playing media with station name, artist, and track
- **WHEN** the status is retrieved
- **THEN** the LCD display shows text in format `{StationName}: {Artist}, {Track}`
- **AND** the LCD display is positioned below the device vendor info

#### Scenario: Hide Now Playing when device is off
- **GIVEN** the user is on the control panel page
- **AND** the device is powered off (standby)
- **WHEN** the control section is rendered
- **THEN** the Now Playing LCD display is not visible

#### Scenario: Now Playing with partial information
- **GIVEN** the user is on the control panel page
- **AND** the device is powered on
- **AND** the device is playing media with only station name available
- **WHEN** the status is retrieved
- **THEN** the LCD display shows only the available information
- **AND** omits missing fields gracefully

#### Scenario: Now Playing with no media information
- **GIVEN** the user is on the control panel page
- **AND** the device is powered on
- **AND** the device has no current media information
- **WHEN** the status is retrieved
- **THEN** the LCD display shows an empty state indicator (e.g., `---`)

---

### Requirement: Now Playing LCD auto-scrolling animation
The system SHALL automatically scroll the Now Playing text from right to left when the text exceeds the display width.

#### Scenario: Long text scrolls horizontally
- **GIVEN** the Now Playing LCD is displayed
- **AND** the formatted text exceeds the container width
- **WHEN** the text is rendered
- **THEN** the text scrolls continuously from right to left
- **AND** the animation loops indefinitely

#### Scenario: Short text does not scroll
- **GIVEN** the Now Playing LCD is displayed
- **AND** the formatted text fits within the container width
- **WHEN** the text is rendered
- **THEN** the text is displayed statically without scrolling

---

### Requirement: Now Playing LCD configurable scroll speed
The system SHALL allow users to configure the scroll speed of the Now Playing LCD animation.

#### Scenario: Default scroll speed
- **GIVEN** the user has not configured a scroll speed preference
- **WHEN** the Now Playing LCD displays scrolling text
- **THEN** the text scrolls at the default speed (medium)

#### Scenario: Change scroll speed to slow
- **GIVEN** the user is on the settings page
- **WHEN** the user selects "Slow" scroll speed option
- **THEN** the preference is persisted
- **AND** the Now Playing LCD scrolls at a slower pace

#### Scenario: Change scroll speed to fast
- **GIVEN** the user is on the settings page
- **WHEN** the user selects "Fast" scroll speed option
- **THEN** the preference is persisted
- **AND** the Now Playing LCD scrolls at a faster pace

#### Scenario: Scroll speed options available
- **GIVEN** the user is on the settings page
- **WHEN** the scroll speed setting is displayed
- **THEN** three options are available: Slow, Medium (default), Fast

---

### Requirement: Now Playing LCD color theme options
The system SHALL allow users to select a color theme for the Now Playing LCD display from predefined options.

#### Scenario: Default color theme
- **GIVEN** the user has not configured a color theme preference
- **WHEN** the Now Playing LCD is displayed
- **THEN** the LCD uses the default green phosphor theme

#### Scenario: Select green theme
- **GIVEN** the user is on the settings page
- **WHEN** the user selects the "Green" LCD theme
- **THEN** the preference is persisted
- **AND** the Now Playing LCD displays green text on dark background

#### Scenario: Select amber theme
- **GIVEN** the user is on the settings page
- **WHEN** the user selects the "Amber" LCD theme
- **THEN** the preference is persisted
- **AND** the Now Playing LCD displays amber/orange text on dark background

#### Scenario: Select blue theme
- **GIVEN** the user is on the settings page
- **WHEN** the user selects the "Blue" LCD theme
- **THEN** the preference is persisted
- **AND** the Now Playing LCD displays blue text on dark background

#### Scenario: Color theme options available
- **GIVEN** the user is on the settings page
- **WHEN** the LCD theme setting is displayed
- **THEN** three color options are available: Green (default), Amber, Blue
- **AND** each option shows a preview of the color

---

### Requirement: Device status polling
The system SHALL poll the device status including Now Playing information at regular intervals while the device is powered on.

#### Scenario: Polling starts when device is on
- **GIVEN** the user is on the control panel page
- **AND** the device is powered on
- **WHEN** the page loads or device powers on
- **THEN** the system begins polling device status every 10 seconds

#### Scenario: Polling stops when device powers off
- **GIVEN** the system is polling device status
- **AND** the device powers off
- **WHEN** the power state changes to off
- **THEN** the system stops polling device status

#### Scenario: Polling resumes when device powers on
- **GIVEN** the system has stopped polling (device was off)
- **AND** the device powers on
- **WHEN** the power state changes to on
- **THEN** the system resumes polling device status every 10 seconds

#### Scenario: Polling updates Now Playing display
- **GIVEN** the system is polling device status
- **WHEN** a new status response is received
- **THEN** the Now Playing display is updated with new media information
- **AND** the current source is updated

#### Scenario: Polling cleanup on navigation
- **GIVEN** the system is polling device status
- **WHEN** the user navigates away from the control panel page
- **THEN** the polling is stopped to prevent memory leaks

---

## MODIFIED Requirements

### Requirement: Bluetooth pairing button
The system SHALL provide a Bluetooth pairing button that is visible only if the device supports the "bluetoothPairing" capability, and displays active state when the current source is Bluetooth.

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
- **THEN** the system calls `POST /api/devices/{id}/bluetooth/pairing`
- **AND** the button shows a loading state during the request

#### Scenario: Bluetooth button active when source is Bluetooth
- **GIVEN** the user is on the control panel page
- **AND** the device is powered on
- **AND** the current source is `BLUETOOTH`
- **WHEN** the control section is rendered
- **THEN** the Bluetooth button displays active styling
- **AND** the button has `aria-pressed="true"`

#### Scenario: Bluetooth button inactive when source is not Bluetooth
- **GIVEN** the user is on the control panel page
- **AND** the device is powered on
- **AND** the current source is not `BLUETOOTH`
- **WHEN** the control section is rendered
- **THEN** the Bluetooth button displays normal (inactive) styling
- **AND** the button has `aria-pressed="false"`

#### Scenario: Bluetooth pairing tooltip
- **GIVEN** the user is on the control panel page
- **AND** the Bluetooth button is visible
- **WHEN** the user hovers over the Bluetooth button
- **THEN** a tooltip explains "Enter Bluetooth pairing mode"

---

### Requirement: AUX input button
The system SHALL provide an icon-based button to switch the device source to AUX input, displaying active state when the current source is AUX.

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

#### Scenario: AUX button active when source is AUX
- **GIVEN** the user is on the control panel page
- **AND** the device is powered on
- **AND** the current source is `AUX` or `AUX_INPUT`
- **WHEN** the control section is rendered
- **THEN** the AUX button displays active styling
- **AND** the button has `aria-pressed="true"`

#### Scenario: AUX button inactive when source is not AUX
- **GIVEN** the user is on the control panel page
- **AND** the device is powered on
- **AND** the current source is not `AUX` or `AUX_INPUT`
- **WHEN** the control section is rendered
- **THEN** the AUX button displays normal (inactive) styling
- **AND** the button has `aria-pressed="false"`
