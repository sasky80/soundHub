## 1. Backend — Station file persistence layer
- [x] 1.1 Create `IStationFileService` interface in `SoundHub.Domain/Interfaces/` with methods: `CreateAsync`, `UpdateAsync`, `ReadAsync`, `ExistsAsync`, `GetPublicUrl`
- [x] 1.2 Create `StationFile` value object in `SoundHub.Domain/Entities/` (Name, StreamUrl, Slug)
- [x] 1.3 Implement `StationFileService` in `SoundHub.Infrastructure/Persistence/` — file I/O under `/data/presets/`, slug generation utility
- [x] 1.4 Add `PUBLIC_HOST_URL` to configuration (`appsettings.json`, environment variable binding)
- [x] 1.5 Register `IStationFileService` in DI (`Program.cs`)
- [x] 1.6 Write unit tests for slug generation (edge cases: special chars, unicode, duplicates)
- [x] 1.7 Write unit tests for `StationFileService` (create, update, conflict detection, read)

## 2. Backend — Integrate station files into preset store flow
- [x] 2.1 Add optional `StreamUrl` property to `StorePresetRequest` in `Preset.cs`
- [x] 2.2 Modify `DeviceService.StorePresetAsync` to detect `LOCAL_INTERNET_RADIO` source and orchestrate station file creation/update before delegating to adapter
- [x] 2.3 Add `GET /api/presets/{filename}` endpoint to `PresetsController` to serve station files
- [x] 2.4 Write unit tests for the updated store preset flow (create with file, update with file, non-LOCAL_INTERNET_RADIO unchanged)
- [x] 2.5 Write integration test: store preset → verify file on disk → verify location URL

## 3. Infrastructure — Caddy & Docker configuration
- [x] 3.1 Add `file_server` block in Caddyfile for `/soundhub/presets/*` serving from `./data/presets/`
- [x] 3.2 Ensure `./data/presets/` directory is created and mounted (already covered by `./data:/data` volume)
- [x] 3.3 Add `PUBLIC_HOST_URL` environment variable to `docker-compose.yml`
- [x] 3.4 Manual verification: create a test `.json` file and confirm it's accessible at `http://<host>/soundhub/presets/test.json`

## 4. Frontend — Preset form conditional fields
- [x] 4.1 Add `streamUrl` field to `StorePresetRequest` interface in `preset.service.ts`
- [x] 4.2 Update preset form component: show/hide `streamUrl` vs `location` based on selected source
- [x] 4.3 Add HTTP validation for stream URL (must start with `http://`)
- [x] 4.4 In edit mode, fetch station file content to pre-populate `streamUrl` when source is `LOCAL_INTERNET_RADIO`
- [x] 4.5 Handle 409 Conflict response: display user-friendly error about duplicate station name
- [x] 4.6 Write unit tests for conditional form logic (show/hide fields, validation)

## 5. Documentation
- [x] 5.1 Update `docs/api-reference.md` with new `GET /api/presets/{filename}` endpoint and `streamUrl` field on store
- [x] 5.2 Update README Caddy section with the `file_server` block for presets
