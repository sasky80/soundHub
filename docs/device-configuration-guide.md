# Device Configuration User Guide

This guide explains how to manage your audio devices in SoundHub, including adding, editing, discovering, and removing devices.

## Accessing Device Configuration

1. Open SoundHub in your browser (default: `http://localhost:4200`)
2. Click on **Settings** from the landing page
3. Navigate to **Device Configuration** or click on **Manage Devices**

## Managing Devices

### Viewing Devices

The device configuration page displays all your configured devices in a list. Each device shows:

- **Name** - The user-defined name for the device
- **Vendor** - The device manufacturer (e.g., Bose SoundTouch)
- **Ping button** - For devices that support audible ping

**Newly Added Devices:** Devices added within the last 5 minutes are highlighted to help you identify recently discovered devices.

### Adding a Device Manually

1. Click the **+** or **Add Device** button
2. Fill in the required fields:
   - **Name** - A friendly name for the device (e.g., "Living Room Speaker")
   - **IP Address** - The device's IP address or hostname (e.g., `192.168.1.100`)
   - **Vendor** - Select the device manufacturer from the dropdown
3. Optionally select capabilities (usually auto-detected)
4. Click **Save**

The system will attempt to connect to the device and detect its capabilities automatically.

**Tip:** Use a static IP or DHCP reservation for your devices to ensure the IP address doesn't change.

### Editing a Device

1. Click on the device in the list or the **Edit** button
2. Modify the desired fields:
   - **Name** - Update the display name
   - **IP Address** - Change if the device has moved
   - **Capabilities** - Manually adjust available features
3. Click **Save**

### Removing a Device

1. Click the **Delete** button on the device
2. Confirm the deletion in the dialog
3. The device will be removed from your configuration

**Note:** Deleting a device only removes it from SoundHub's configuration. The physical device is not affected.

## Device Discovery

### Configuring Network Range

Before discovering devices, configure the network range to scan:

1. Find the **Network Mask** input field
2. Enter your network in CIDR notation:
   - Example: `192.168.1.0/24` scans 192.168.1.1 through 192.168.1.254
   - Example: `10.0.0.0/24` scans 10.0.0.1 through 10.0.0.254
3. Click **Save** to store the network mask

**Tip:** Use a `/24` subnet for typical home networks. Larger ranges take longer to scan.

### Running Discovery

1. Ensure your devices are powered on and connected to the network
2. Click **Discover Devices**
3. Wait for the scan to complete (may take a few minutes)
4. Newly discovered devices are automatically saved and highlighted

**What Discovery Does:**
- Scans the configured IP range for devices
- Probes each IP for supported vendor signatures
- Skips devices that are already configured (matched by IP)
- Automatically saves new devices with detected capabilities

## Ping / Connectivity Test

The ping feature helps verify that a device is reachable and responding.

### Using Ping

1. Find a device with a **Ping** button (only available for devices with ping capability)
2. Click the **Ping** button
3. The device will:
   - Emit a double beep sound
   - Show connection status (success/error)
   - Display latency in milliseconds

**Ping States:**
- 🔄 **Pinging** - Request in progress
- ✅ **Success** - Device responded (shows latency)
- ❌ **Error** - Device unreachable

**Note:** Not all devices support audible ping. The button only appears for devices with the "ping" capability.

## Volume Control

The device details page includes controls for adjusting volume and muting the device.

### Volume Slider

1. Navigate to a device's details page by clicking on it from the landing page
2. The **Volume** slider shows the current volume level (0-100)
3. Drag the slider to adjust the volume
4. The volume value updates as you move the slider
5. Volume changes are sent to the device with a 300ms debounce to prevent rapid API calls

### Mute Toggle

1. Click the **Mute** button to toggle mute on/off
2. When muted, the button displays "Unmute" with a red highlight
3. When unmuted, the button displays "Mute"
4. Click again to restore the previous volume

### Volume Controls Disabled State

**Important:** Volume controls are disabled when the device is powered off.

- The slider becomes unresponsive and appears dimmed
- The mute button becomes unclickable
- A tooltip indicates "Volume control unavailable when device is off"

To enable volume controls, turn the device on using the **Power** button first.

**Note:** Volume control is only available for devices with the "volume" capability.

## Preset Management

Manage and trigger SoundTouch presets directly from the device details page.

### Viewing Presets

1. Open the landing page and select any configured device.
2. Scroll below the volume controls to the **Presets** grid.
3. Each tile shows:
   - A 64×64 play button with the preset artwork (or default icon)
   - The preset name as a text button (tap/click to edit)
   - A loading spinner when a preset command is in progress
4. The final tile always contains the **+ Add Preset** shortcut.

### Playing Presets

1. Ensure the device is online (SoundHub automatically powers it on if required).
2. Click the circular play button for any preset slot.
3. The UI shows a spinner and disables other preset buttons until the API responds.
4. On success the device begins playback; failures surface as toast/console errors.

### Adding or Editing a Preset

1. From the preset grid click the **+** tile to create a new preset.
2. Alternatively, click an existing preset name to open the edit form (slot field becomes read-only).
3. The form requires:
   - **Preset Slot (1‑6)** – corresponds to SoundTouch physical buttons
   - **Name** – label shown in the UI
   - **Location URL** – stream, playlist, or station URL
4. Optional fields:
   - **Image URL** – artwork/background for the play button
   - **Type** and **Source** – SoundTouch-specific metadata
5. Click **Save** to submit. The UI navigates back to the device page after a successful API response.
6. Use **Cancel** to discard changes at any time.

### SoundTouch-Specific Fields

- **Type** defaults to `stationurl`.
- **Source** defaults to `LOCAL_INTERNET_RADIO`.
- Leave either blank to accept the defaults—SoundHub applies them automatically on the backend.
- Use custom values only when the SoundTouch WebServices API expects a different combination (e.g., TuneIn, Spotify).

### Deleting a Preset

1. Open an existing preset.
2. Click **Delete** to show the confirmation modal.
3. Confirm to remove the preset from the device; cancel keeps the preset intact.
4. The preset list refreshes automatically after deletion.

## Device Capabilities

Capabilities define what features are available for each device:

| Capability | Description |
|------------|-------------|
| **power** | Power on/off control |
| **volume** | Volume adjustment |
| **presets** | Play saved presets (1-6) |
| **bluetoothPairing** | Enter Bluetooth pairing mode |
| **ping** | Audible connectivity test |

### How Capabilities Are Detected

For Bose SoundTouch devices:
- **Base capabilities** (`power`, `volume`) are always present
- **Additional capabilities** are detected by querying the device's `/supportedUrls` endpoint

### Manually Editing Capabilities

1. Edit the device
2. Check/uncheck capability checkboxes
3. Save the device

**Caution:** Enabling a capability that the device doesn't support may cause errors when using that feature.

## Troubleshooting

### Device Not Found During Discovery

1. Verify the device is powered on
2. Check that the device is connected to the same network
3. Ensure the network mask covers the device's IP range
4. Try a larger subnet (e.g., `/16` instead of `/24`)

### Ping Fails

1. Verify the device IP address is correct
2. Check that the device is online
3. Ensure no firewall is blocking port 8090 (for SoundTouch)
4. The device may not support the ping capability

### Cannot Add Device

1. Verify the IP address format is correct
2. Ensure the device is reachable from the server
3. Check that the vendor is supported

### Device Shows Wrong Capabilities

1. Edit the device
2. Manually adjust capabilities
3. Save the device

## Best Practices

1. **Use Static IPs** - Configure DHCP reservations for your audio devices
2. **Meaningful Names** - Use descriptive names like "Living Room Speaker" instead of "Device 1"
3. **Regular Discovery** - Run discovery periodically to find new devices
4. **Test Connectivity** - Use ping to verify devices are reachable before troubleshooting other issues
5. **Backup Configuration** - Keep a backup of your `devices.json` file
