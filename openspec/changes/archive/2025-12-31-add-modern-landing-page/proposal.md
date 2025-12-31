# Change: Add modern landing page with settings, device list, and power control

## Why
SoundHub currently lacks a usable web entry point: there are no routes and no UI flows to view configured devices, change language, or control a device. A modern landing page and minimal device details view unlocks the first end-user workflow.

## What Changes
- Add a **Landing page** that:
  - Displays the list of configured device names.
  - Allows navigation to **Settings**.
  - Allows navigation to **Device details** for a selected device.
- Add a **Settings page** that:
  - Allows selecting UI language (**English** and **Polish** only for now).
  - Provides navigation to a **Device configuration page**.
- Add a **Device configuration page** (initial navigation + basic list) reachable from Settings.
- Add a **Device details page** that supports a single control: **power on/off**.
- Add a minimal API endpoint to toggle power on/off so the device details page can perform real control.

## Impact
- **Affected specs (new):**
  - `web-ui` (landing/settings/device configuration/device details + i18n selector)
  - `api-device-control` (power toggle endpoint)
- **Affected code (expected):**
  - Frontend routing and UI under `frontend/src/app/**` and libraries under `frontend/libs/frontend/**`
  - Backend device controller/service additions under `services/SoundHub.Api/**` and `services/SoundHub.Application/**`
- **Breaking changes:** None
