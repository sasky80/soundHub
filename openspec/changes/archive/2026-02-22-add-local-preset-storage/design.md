## Context
When a Bose SoundTouch preset has `source=LOCAL_INTERNET_RADIO`, the device fetches a JSON station definition from an HTTP URL during playback. That URL must be reachable from the device on the LAN. SoundHub already runs on the same LAN and sits behind a Caddy reverse proxy at `/soundhub/`. Hosting station files on the same server removes external hosting dependencies.

## Goals
- Store station JSON files under `/data/presets/` with a deterministic filename derived from the station name.
- Serve station files to SoundTouch devices via Caddy (`/soundhub/presets/<file>.json`).
- Prevent accidental overwrite on create; allow overwrite on edit.
- Keep the frontend simple: collect stream URL + station name when source is `LOCAL_INTERNET_RADIO`.

## Non-Goals
- Support for HTTPS stream URLs (SoundTouch requires HTTP).
- Managing station files for non-SoundTouch devices (future scope).
- A station file browser/listing UI (files are tied to presets; managed through the preset form).

## Decisions

### Station filename convention
- **Decision**: Derive filename by slugifying the station name: lowercase, replace non-alphanumeric with hyphens, collapse consecutive hyphens, trim. Example: `"Jazz FM 91.1"` → `jazz-fm-91-1.json`.
- **Alternatives considered**: UUID-based filenames (less human-readable, harder to debug), user-supplied filenames (error-prone).

### Create vs update semantics
- **Decision**: The `POST /api/devices/{id}/presets` endpoint already handles both create and update. When `source=LOCAL_INTERNET_RADIO`:
  1. Generate the station filename from the station name.
  2. On **create** (no existing file for that slug): write the file; return 409 if file already exists for a different preset.
  3. On **update** (preset already exists in that slot): overwrite the file.
  4. Set `location` to `http://<configuredHost>/soundhub/presets/<slug>.json` before sending `storePreset` to the device.
- **Alternatives considered**: Separate endpoint for station file CRUD (adds complexity without benefit since the lifecycle is tied to presets).

### Serving station files
- **Decision**: Caddy `file_server` directive serves `/soundhub/presets/*` directly from `./data/presets/` on the host filesystem. This avoids routing through the .NET API for static file serving.
- **Fallback**: Also expose `GET /api/devices/presets/{filename}` from the API so station files are accessible even without Caddy (e.g., local dev).
- **Alternatives considered**: Serving through the .NET API only (unnecessary overhead for static JSON files in production).

### Host configuration for location URL
- **Decision**: Introduce an environment variable `PUBLIC_HOST_URL` (e.g., `http://mini.local/soundhub`) used to construct the `location` field. Falls back to `http://localhost:5001` in development.
- **Alternatives considered**: Auto-detect from request headers (fragile with proxies), hardcode (not portable).

## Risks / Trade-offs
- **Filename collisions**: Two stations with names that slugify to the same value would collide. Mitigation: return 409 and prompt user to choose a different name.
- **Orphaned files**: Deleting a preset doesn't delete the station file (another device/preset may reference it). Mitigation: acceptable for now; add a cleanup/garbage-collection mechanism later if needed.
- **Device fetches fail**: If Caddy is down or misconfigured, the device can't fetch the station file. Mitigation: documented in setup guide; health-check existing.

## Resolved Questions
- **Should station file deletion happen automatically when the last preset referencing it is removed?**
  No. Multiple devices may reference the same station file. Station files are never deleted — they are small JSON files and the cost of orphaned files is negligible. A manual cleanup utility may be added in the future if needed.
