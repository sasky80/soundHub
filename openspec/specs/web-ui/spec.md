# web-ui Specification

## Purpose
TBD - created by archiving change add-modern-landing-page. Update Purpose after archive.
## Requirements
### Requirement: Landing page shows configured devices
The system SHALL provide a landing page that displays the names of all configured devices.

#### Scenario: Devices are configured
- **GIVEN** the backend returns a non-empty list from `GET /api/devices`
- **WHEN** the user opens the landing page
- **THEN** the page shows a list containing each device name

#### Scenario: No devices are configured
- **GIVEN** the backend returns an empty list from `GET /api/devices`
- **WHEN** the user opens the landing page
- **THEN** the page shows an empty state indicating no devices are configured

### Requirement: Landing page navigation
The system SHALL allow navigation from the landing page to Settings and to a selected device details page.

#### Scenario: Navigate to Settings
- **GIVEN** the user is on the landing page
- **WHEN** the user activates the Settings navigation control
- **THEN** the system navigates to the Settings page

#### Scenario: Navigate to Device details
- **GIVEN** the user is on the landing page
- **AND** at least one device is displayed
- **WHEN** the user selects a device
- **THEN** the system navigates to the Device details page for that device

### Requirement: Settings page language selection
The system SHALL provide a settings page that allows the user to select the UI language between English and Polish.

#### Scenario: Change language to Polish
- **GIVEN** the UI language is English
- **WHEN** the user selects Polish
- **THEN** the UI text updates to Polish
- **AND** the selection is persisted for future sessions

#### Scenario: Change language to English
- **GIVEN** the UI language is Polish
- **WHEN** the user selects English
- **THEN** the UI text updates to English
- **AND** the selection is persisted for future sessions

### Requirement: Settings page navigation to device configuration
The system SHALL provide navigation from the settings page to a device configuration page.

#### Scenario: Navigate to device configuration
- **GIVEN** the user is on the settings page
- **WHEN** the user activates the device configuration navigation control
- **THEN** the system navigates to the device configuration page

### Requirement: Device configuration page lists configured devices
The system SHALL provide a device configuration page that lists configured devices with name, vendor, and ping button, and allows navigation to device details.

#### Scenario: Open device configuration page
- **GIVEN** the backend returns one or more configured devices
- **WHEN** the user opens the device configuration page
- **THEN** the system shows a list of configured devices
- **AND** each device displays its name and vendor
- **AND** devices with "ping" capability show a ping button

#### Scenario: Ping button visibility
- **GIVEN** a device has the "ping" capability in its capabilities list
- **WHEN** the device is displayed in the device list
- **THEN** the ping button is visible for that device

#### Scenario: Ping button hidden for devices without capability
- **GIVEN** a device does not have the "ping" capability
- **WHEN** the device is displayed in the device list
- **THEN** the ping button is not visible for that device

#### Scenario: Navigate to device details
- **GIVEN** the user is on the device configuration page
- **WHEN** the user selects a device
- **THEN** the system navigates to the device definition page for editing

### Requirement: Device details page supports power toggle
The system SHALL provide a device details page that allows the user to toggle power on/off.

#### Scenario: Turn device on
- **GIVEN** the user is on the device details page
- **WHEN** the user turns power on
- **THEN** the UI sends a power-on request to the backend
- **AND** the UI reflects the new power state when the request succeeds

#### Scenario: Turn device off
- **GIVEN** the user is on the device details page
- **WHEN** the user turns power off
- **THEN** the UI sends a power-off request to the backend
- **AND** the UI reflects the new power state when the request succeeds

### Requirement: Ping device from configuration page
The system SHALL allow users to verify device connectivity via the ping button, triggering an audible beep on the device.

#### Scenario: Ping device successfully
- **GIVEN** the user is on the device configuration page
- **AND** a device has the "ping" capability
- **WHEN** the user clicks the ping button
- **THEN** the system sends a ping request to the backend
- **AND** the device emits a double beep sound
- **AND** displays a success indicator when the device is reachable

#### Scenario: Ping device fails
- **GIVEN** the user is on the device configuration page
- **AND** a device has the "ping" capability
- **WHEN** the user clicks the ping button
- **AND** the device is not reachable
- **THEN** the system displays a failure indicator

### Requirement: Add new device
The system SHALL allow users to add a new device via a form.

#### Scenario: Open add device form
- **GIVEN** the user is on the device configuration page
- **WHEN** the user clicks the add (+) button
- **THEN** the system opens the new device definition page

#### Scenario: Add device with required fields
- **GIVEN** the user is on the add device form
- **WHEN** the user provides IP address or FQDN, vendor, name, and capabilities
- **AND** clicks the save button
- **THEN** the system creates the device via the backend API
- **AND** the new device appears in the device list

#### Scenario: Vendor selection populates from supported vendors
- **GIVEN** the user is on the add device form
- **WHEN** the user views the vendor dropdown
- **THEN** the dropdown is populated with vendors from `GET /api/vendors`

#### Scenario: Capabilities selection
- **GIVEN** the user is on the add device form
- **WHEN** the user selects capabilities
- **THEN** the user can choose from available capabilities via checkboxes

### Requirement: Edit device configuration
The system SHALL allow users to edit an existing device's configuration.

#### Scenario: Open device for editing
- **GIVEN** a device exists in the configuration
- **WHEN** the user selects the device from the list
- **THEN** the system opens the device definition page with current values populated

#### Scenario: Update device properties
- **GIVEN** the user is editing a device
- **WHEN** the user modifies IP address, vendor, name, or capabilities
- **AND** clicks the save button
- **THEN** the system updates the device via the backend API
- **AND** the device list reflects the changes

### Requirement: Remove device from configuration
The system SHALL allow users to remove a device from configuration.

#### Scenario: Delete device with confirmation
- **GIVEN** the user is viewing a device or on the device list
- **WHEN** the user initiates device removal
- **THEN** the system prompts for confirmation
- **AND** upon confirmation, removes the device via the backend API
- **AND** the device is removed from the device list

### Requirement: Configure network mask for discovery
The system SHALL allow users to specify a network mask for device discovery.

#### Scenario: View current network mask
- **GIVEN** the user is on the device configuration page
- **WHEN** the page loads
- **THEN** the current network mask is displayed in an input field (or empty if not set)

#### Scenario: Update network mask
- **GIVEN** the user is on the device configuration page
- **WHEN** the user enters a valid network mask (CIDR notation)
- **AND** the value is saved
- **THEN** the network mask is persisted via the backend API

#### Scenario: Invalid network mask validation
- **GIVEN** the user is on the device configuration page
- **WHEN** the user enters an invalid network mask format
- **THEN** the system displays a validation error

### Requirement: Launch device discovery
The system SHALL allow users to launch device discovery process.

#### Scenario: Start device discovery
- **GIVEN** the user is on the device configuration page
- **AND** a network mask is configured
- **WHEN** the user clicks the discover devices button
- **THEN** the system initiates discovery via the backend API
- **AND** shows a loading indicator during discovery

#### Scenario: Display discovered devices
- **GIVEN** device discovery has completed
- **WHEN** new devices are found
- **THEN** the newly discovered devices appear in the device list
- **AND** devices added within the last 5 minutes are highlighted in the UI

#### Scenario: Highlight recently added devices
- **GIVEN** a device has a `DateTimeAdded` within the last 5 minutes
- **WHEN** the device list is displayed
- **THEN** the device is visually highlighted as recently added

#### Scenario: Discovery does not overwrite existing devices
- **GIVEN** devices are already configured
- **WHEN** the discovery process finds devices at the same IP addresses
- **THEN** existing device configurations remain unchanged
- **AND** only truly new devices are added to the list

#### Scenario: Discovery without network mask
- **GIVEN** no network mask is configured
- **WHEN** the user attempts to start discovery
- **THEN** the system displays an error indicating network mask is required

### Requirement: Device details page supports volume control
The system SHALL provide volume control on the device details page, including a volume slider and mute button.

#### Scenario: Display volume slider with current level
- **GIVEN** the user is on the device details page
- **AND** the device is powered on
- **WHEN** the page loads
- **THEN** the UI displays a volume slider
- **AND** the slider shows the current volume level (0-100) fetched from the backend

#### Scenario: Adjust volume using slider
- **GIVEN** the user is on the device details page
- **AND** the device is powered on
- **WHEN** the user moves the volume slider to a new position
- **THEN** the UI sends a volume change request to the backend
- **AND** the slider reflects the new volume level when the request succeeds

#### Scenario: Display mute button with current state
- **GIVEN** the user is on the device details page
- **AND** the device is powered on
- **WHEN** the page loads
- **THEN** the UI displays a mute button
- **AND** the button shows the current mute state (muted or unmuted)

#### Scenario: Toggle mute
- **GIVEN** the user is on the device details page
- **AND** the device is powered on
- **WHEN** the user clicks the mute button
- **THEN** the UI sends a mute toggle request to the backend
- **AND** the mute button reflects the new state when the request succeeds

#### Scenario: Volume controls disabled when device is off
- **GIVEN** the user is on the device details page
- **AND** the device is powered off (standby)
- **WHEN** the page loads
- **THEN** the volume slider is disabled
- **AND** the mute button is disabled
- **AND** a visual indication shows controls are unavailable

#### Scenario: Volume slider shows actual volume on page load
- **GIVEN** the user is on the device details page
- **AND** the device is powered on with volume set to 42
- **WHEN** the page loads
- **THEN** the volume slider is positioned at 42
- **AND** the displayed volume value shows 42

### Requirement: Device details page displays preset list
The system SHALL display a list of configured presets on the device details page below the volume controls section.

#### Scenario: Display presets with icons
- **GIVEN** the user is on the device details page
- **AND** the device has presets configured
- **WHEN** the page loads
- **THEN** the UI displays a preset list section below volume controls
- **AND** each preset shows a 64x64px play button
- **AND** if the preset has an iconUrl, the icon is used as the button background
- **AND** the preset name is displayed alongside the play button

#### Scenario: Display presets without icons
- **GIVEN** the user is on the device details page
- **AND** a preset does not have an iconUrl
- **WHEN** the preset is displayed
- **THEN** the play button shows a default play icon
- **AND** the preset name is displayed

#### Scenario: No presets configured
- **GIVEN** the user is on the device details page
- **AND** the device has no presets configured
- **WHEN** the page loads
- **THEN** the UI shows an empty state for presets
- **AND** displays a message indicating no presets are configured

#### Scenario: Display add preset button
- **GIVEN** the user is on the device details page
- **WHEN** the preset section is displayed
- **THEN** a "+" button is visible to add a new preset

---

### Requirement: Play preset from device details page
The system SHALL allow users to play a preset by clicking the play button, powering on the device if necessary.

#### Scenario: Play preset when device is on
- **GIVEN** the user is on the device details page
- **AND** the device is powered on
- **AND** presets are displayed
- **WHEN** the user clicks a preset play button
- **THEN** the system sends a play preset request to the backend
- **AND** the device begins playing the preset content

#### Scenario: Play preset powers on device if off
- **GIVEN** the user is on the device details page
- **AND** the device is powered off (standby)
- **AND** presets are displayed
- **WHEN** the user clicks a preset play button
- **THEN** the system sends a play preset request to the backend
- **AND** the backend powers on the device first
- **AND** the device begins playing the preset content
- **AND** the UI reflects the new power state

#### Scenario: Show loading state while playing preset
- **GIVEN** the user clicks a preset play button
- **WHEN** the request is in progress
- **THEN** the play button shows a loading indicator
- **AND** the button is disabled until the request completes

---

### Requirement: Navigate to add new preset page
The system SHALL allow users to navigate to a new preset definition page.

#### Scenario: Navigate to new preset page
- **GIVEN** the user is on the device details page
- **WHEN** the user clicks the "+" button in the presets section
- **THEN** the system navigates to the new preset definition page

---

### Requirement: Preset definition page for new presets
The system SHALL provide a form page for creating new presets with required and optional fields.

#### Scenario: Display new preset form
- **GIVEN** the user navigates to the new preset definition page
- **WHEN** the page loads
- **THEN** the form displays fields for:
  - Preset slot (1-6, required)
  - Name (required)
  - Image URL (optional)
  - Location (required, the stream/content URL)
- **AND** for SoundTouch devices, additional fields:
  - Type (default: "stationurl")
  - Source (default: "LOCAL_INTERNET_RADIO")

#### Scenario: Save new preset
- **GIVEN** the user is on the new preset form
- **AND** all required fields are filled
- **WHEN** the user clicks the save button
- **THEN** the system sends a create preset request to the backend
- **AND** on success, navigates back to the device details page
- **AND** the new preset appears in the preset list

#### Scenario: Cancel new preset creation
- **GIVEN** the user is on the new preset form
- **WHEN** the user clicks cancel or navigates back
- **THEN** the system navigates back to the device details page
- **AND** no preset is created

#### Scenario: Validation error on new preset
- **GIVEN** the user is on the new preset form
- **AND** required fields are missing
- **WHEN** the user attempts to save
- **THEN** the system displays validation errors
- **AND** does not submit the form

---

### Requirement: Edit existing preset
The system SHALL allow users to edit an existing preset's configuration.

#### Scenario: Navigate to edit preset page
- **GIVEN** the user is on the device details page
- **AND** presets are displayed
- **WHEN** the user clicks on a preset (not the play button area)
- **THEN** the system navigates to the preset details page with current values populated

#### Scenario: Display edit preset form
- **GIVEN** the user navigates to an existing preset's details page
- **WHEN** the page loads
- **THEN** the form displays current values for:
  - Preset slot (read-only)
  - Name
  - Image URL
  - Location
- **AND** for SoundTouch devices: Type and Source

#### Scenario: Update preset
- **GIVEN** the user is on the preset details page
- **AND** modifies preset values
- **WHEN** the user clicks the save button
- **THEN** the system sends an update preset request to the backend
- **AND** on success, navigates back to the device details page
- **AND** the updated preset values are reflected

---

### Requirement: Remove preset from device
The system SHALL allow users to remove a preset from the device.

#### Scenario: Remove preset with confirmation
- **GIVEN** the user is on the preset details page
- **WHEN** the user clicks the delete button
- **THEN** the system prompts for confirmation
- **AND** upon confirmation, sends a delete preset request to the backend
- **AND** on success, navigates back to the device details page
- **AND** the preset is removed from the list

#### Scenario: Cancel preset removal
- **GIVEN** the user is on the preset details page
- **AND** clicks the delete button
- **WHEN** the confirmation dialog appears
- **AND** the user cancels
- **THEN** the preset is not removed
- **AND** the user remains on the preset details page

---

### Requirement: Device control panel header displays "Control panel"
The system SHALL display "Control panel" as the header instead of "Device Details" on the device details page.

#### Scenario: Display control panel header
- **GIVEN** the user navigates to the device details page
- **WHEN** the page loads
- **THEN** the header displays "Control panel"

---

### Requirement: Volume control displays icon instead of text label
The system SHALL display a volume icon on the left side of the volume slider instead of a text label.

#### Scenario: Volume icon displayed
- **GIVEN** the user is on the control panel page
- **WHEN** the volume section is rendered
- **THEN** a volume icon (speaker symbol) is displayed on the left side of the slider
- **AND** no "Volume" text label is shown

---

### Requirement: Remote controller button layout
The system SHALL display control buttons in a grid layout resembling a physical remote controller, using icon-only buttons without text labels.

#### Scenario: Remote layout grid structure
- **GIVEN** the user is on the control panel page
- **WHEN** the control buttons section is rendered
- **THEN** buttons are arranged in a CSS grid layout
- **AND** buttons use icons only (no text labels on buttons)
- **AND** buttons have aria-labels for accessibility
- **AND** the layout adapts responsively to screen size

---

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

---

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

### Requirement: Control buttons disabled when device off
The system SHALL disable all control buttons (except power button) when the device is powered off.

#### Scenario: Buttons disabled in standby
- **GIVEN** the device is powered off (standby mode)
- **WHEN** the control panel is displayed
- **THEN** all playback, volume, AUX, and Bluetooth buttons are disabled
- **AND** the power button remains enabled
- **AND** tooltips explain "Device must be powered on"

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

