# Change: Add local preset storage for LOCAL_INTERNET_RADIO stations

## Why
When a SoundTouch preset uses `LOCAL_INTERNET_RADIO` as its source, the device fetches a JSON station definition from an HTTP URL at playback time. Currently there is no local storage for these station files — users must host them elsewhere. Storing station JSON files on the SoundHub server under `/data/presets/` and serving them through Caddy at `/soundhub/presets/<file>.json` makes the system self-contained: the server that controls the speaker also hosts the station definitions it references.

## What Changes
- **Backend**: New API endpoint to create/update station JSON files under `/data/presets/`.
  - On **create** (new preset): write the station `.json` file; fail with 409 if file already exists.
  - On **update** (edit preset): overwrite the existing station file.
  - The `StorePreset` flow sets the preset `location` to `http://<host>/soundhub/presets/<filename>.json` before sending to the SoundTouch device.
- **Backend**: New API endpoint to read a station JSON file (serves the raw file content).
- **Reverse proxy**: Caddy serves `/soundhub/presets/*` as static files from the `/data/presets/` directory on the host, so SoundTouch devices can fetch station definitions over HTTP.
- **Frontend**: When the user selects `LOCAL_INTERNET_RADIO` as the source in the preset form, the form collects `streamUrl` and `stationName` instead of a raw `location` URL. The backend handles file creation and location URL generation.
- **Docker**: Mount `/data/presets` volume so station files persist across container restarts.

## Impact
- Affected specs: `api-device-control`, `web-ui`
- Affected code:
  - `services/SoundHub.Api/Controllers/DevicesController.cs` — new endpoint
  - `services/SoundHub.Application/Services/DeviceService.cs` — station file orchestration
  - `services/SoundHub.Infrastructure/Persistence/` — file I/O for station JSON
  - `frontend/libs/frontend/feature/src/lib/preset-form/` — conditional form fields
  - `frontend/libs/frontend/data-access/src/lib/preset.service.ts` — updated request model
  - `docker-compose.yml` — volume mount (already at `./data:/data`)
  - Caddyfile — new `file_server` block for `/soundhub/presets/*`
