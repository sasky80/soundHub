## Context
The Angular app currently has no routes (`frontend/src/app/app.routes.ts` is empty). The backend supports listing devices and device status, but does not yet expose a dedicated power-toggle endpoint.

## Decisions

### 1) Runtime language switching
**Decision:** Implement runtime language switching with a translation library (e.g., `@ngx-translate/core`) and persist the selected language in `localStorage`.

**Rationale:** Angular compile-time i18n typically requires separate builds per locale; this change needs in-app switching between English and Polish.

**Non-goals:** Full translation coverage beyond the new pages; additional languages.

### 2) Navigation and routes
**Decision:** Add explicit routes for the new flows:
- `/` landing
- `/settings` settings
- `/settings/devices` (or `/devices`) device configuration
- `/devices/:id` device details

**Rationale:** Keeps URLs predictable and supports direct linking.

### 3) Power control API
**Decision:** Add a minimal endpoint to control power:
- `POST /api/devices/{id}/power` with body `{ "on": boolean }`

The handler calls into the application layer, which resolves the correct adapter and invokes `IDeviceAdapter.SetPowerAsync`.

**Rationale:** The UI must be able to actually toggle device power. Existing backend abstractions already include `SetPowerAsync` on adapters.

**Non-goals:** Volume/presets/pairing controls in this change.
