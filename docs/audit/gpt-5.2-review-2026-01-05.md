# Backend Review Tasks (2026-01-05)

This document turns the backend review findings into actionable, testable tasks.

## Context
- Solution: `services/SoundHub.sln`
- Projects: `SoundHub.Api`, `SoundHub.Application`, `SoundHub.Infrastructure`, `SoundHub.Domain`
- Verification run: `dotnet build` + `dotnet test` (125 tests passing)

## Findings → Tasks

### 1) Fix HttpResponseMessage disposal (HIGH)
**Problem**: `SoundTouchAdapter` performs HTTP calls without disposing `HttpResponseMessage`, which can lead to socket/connection exhaustion under load.

**Where**
- `services/SoundHub.Infrastructure/Adapters/SoundTouchAdapter.cs`

**Tasks**
- [x] Update `GetAsync` to dispose the response:
  - Use `using var response = await _httpClient.GetAsync(url, ct);`
  - Keep `response.EnsureSuccessStatusCode();`
  - Prefer `await response.Content.ReadAsStringAsync(ct)`.
- [x] Update `PostXmlAsync` to dispose the response:
  - `using var response = await _httpClient.PostAsync(url, content, ct);`
- [x] Update discovery call in `TryDiscoverDeviceAtIpAsync` to dispose the response:
  - `using var response = await _httpClient.GetAsync(url, cts.Token);`
- [ ] Consider `HttpCompletionOption.ResponseHeadersRead` only if you later stream content (not required now).

**Status**: Completed (disposed responses in `GetAsync`, `PostXmlAsync`, and discovery HTTP call).

**Acceptance criteria**
- No functional behavior change for existing endpoints.
- `dotnet build .\services\SoundHub.sln -c Release` succeeds.
- `dotnet test .\services\tests\SoundHub.Tests\SoundHub.Tests.csproj -c Release` succeeds.

---

### 2) Add authenticated encryption for secrets (HIGH)
**Problem**: `EncryptedSecretsService` uses AES (CBC default) without authentication. This makes stored ciphertext malleable (tampering not detectable).

**Where**
- `services/SoundHub.Infrastructure/Services/EncryptedSecretsService.cs`

**Approach (recommended)**
- Use AEAD via `AesGcm`.
- Store a versioned envelope format to support migration.

**Tasks**
- [x] Define an envelope format for secret values, e.g.
  - `v1:<base64(nonce|tag|ciphertext)>` (or JSON with fields `v`, `nonce`, `tag`, `ct`).
- [x] Implement `EncryptValueAsync` with `AesGcm`:
  - Generate `nonce` (12 bytes typical).
  - Encrypt bytes of the plaintext (UTF-8).
  - Produce `tag` (16 bytes typical).
- [x] Implement `DecryptValueAsync`:
  - If value starts with `v1:`: parse envelope and decrypt using `AesGcm`.
  - Else: treat it as legacy (current format), decrypt using existing AES-CBC path.
- [x] Add a migration behavior:
  - On successful legacy decrypt, re-encrypt as v1 and persist (implemented as migration-on-read).
- [x] Add tests in `services/tests/SoundHub.Tests`:
  - Round-trip v1 encrypt/decrypt.
  - Tampering test: modify one byte and verify decrypt throws (or returns error) and is handled.
  - Legacy decrypt still works (you can generate a legacy ciphertext with the old method inside the test).

**Status**: Completed.

**Notes**
- New secrets are stored as `v1:<base64(nonce|tag|ciphertext)>` using `AesGcm`.
- Legacy values (base64 of `IV|ciphertext`) are still supported and are migrated to `v1:` on successful read.
- Tests added: `services/tests/SoundHub.Tests/Infrastructure/EncryptedSecretsServiceTests.cs`.

**Acceptance criteria**
- Existing secrets file continues to work (backward compatible).
- New secrets are stored in v1 format.
- Tampering fails reliably.
- Tests cover both v1 and legacy.

---

### 3) Fail fast when master password is not configured (HIGH)
**Problem**: API can start with a hardcoded default master password (`default-dev-password`) when no config is provided.

**Where**
- `services/SoundHub.Api/Program.cs`
- `services/SoundHub.Infrastructure/Services/EncryptionKeyStore.cs`
- `services/SoundHub.Infrastructure/Services/EncryptionKeyStoreOptions` (defaults)

**Tasks**
- [ ] In `Program.cs`, remove/avoid setting a default master password for non-development.
- [ ] Add a startup guard:
  - If `!app.Environment.IsDevelopment()` AND no `MasterPasswordFile` exists AND `MasterPassword` is null/empty: throw `InvalidOperationException` with a clear message.
- [ ] Ensure the message is actionable (points to env vars / secrets).

**Acceptance criteria**
- Development remains easy to run.
- Production-like environment won’t silently use a weak default.

---

### 4) Add concurrency limiting to device discovery (MEDIUM)
**Problem**: `DiscoverDevicesAsync` creates a task per IP (often 254) with no concurrency cap.

**Where**
- `services/SoundHub.Infrastructure/Adapters/SoundTouchAdapter.cs`

**Tasks**
- [x] Add a concurrency limit (e.g., `int maxConcurrency = 25;`).
- [x] Implement a throttled discovery loop:
  - Option A: `SemaphoreSlim` + `Task.WhenAll`.
  - Option B: `Parallel.ForEachAsync(ipRange, new ParallelOptions { MaxDegreeOfParallelism = ... })`.
- [x] Ensure cancellation works properly.

**Status**: Completed (uses `Parallel.ForEachAsync` with `MaxDegreeOfParallelism = 25`).

**Acceptance criteria**
- Discovery still finds devices.
- Socket spikes reduced (bounded parallelism).

---

### 5) Normalize error-handling semantics across endpoints (MEDIUM)
**Problem**: Some adapter methods return “offline” objects while others throw; controllers map exceptions to 503/504. This can lead to inconsistent client experience.

**Where**
- `services/SoundHub.Api/Controllers/DevicesController.cs`
- `services/SoundHub.Infrastructure/Adapters/SoundTouchAdapter.cs`

**Tasks (choose one policy)**
- [ ] Policy A (prefer exceptions): adapter throws `InvalidOperationException` for unreachable; controller maps to 503.
- [ ] Policy B (prefer status objects): adapter returns `DeviceStatus { IsOnline=false }` and controller always returns 200 for status endpoints.
- [ ] Apply policy consistently to: status, now playing, volume, presets.

**Acceptance criteria**
- Client behavior is predictable and documented.

---

## Implementation Prompt (copy/paste)

Use this prompt to drive an implementation pass:

> You are working in the `soundHub` repo. Implement the backend review fixes from `review-2026-01-05.md`.
> 
> Scope:
> 1) Fix `HttpResponseMessage` disposal in `services/SoundHub.Infrastructure/Adapters/SoundTouchAdapter.cs` (`GetAsync`, `PostXmlAsync`, and discovery HTTP calls). Keep behavior the same.
> 2) Upgrade secrets encryption to authenticated encryption in `services/SoundHub.Infrastructure/Services/EncryptedSecretsService.cs` using `AesGcm` with a versioned envelope format (e.g., `v1:` prefix). Maintain backward compatibility by still decrypting legacy values.
> 3) Add a production safety guard to avoid using `default-dev-password` outside Development (fail fast with a clear message when master password/secret is not configured).
> 4) Add bounded concurrency to SoundTouch discovery.
> 
> Constraints:
> - Keep diffs minimal and consistent with existing conventions.
> - Don’t add new abstraction layers.
> - Add/adjust tests in `services/tests/SoundHub.Tests` for the new crypto behavior.
> - Don’t change TFMs or package versions unless necessary.
> 
> Validate:
> - `dotnet build .\\services\\SoundHub.sln -c Release`
> - `dotnet test .\\services\\tests\\SoundHub.Tests\\SoundHub.Tests.csproj -c Release`
> 
> Deliverables:
> - Code changes implementing all scoped items.
> - Tests for AEAD encrypt/decrypt + tamper detection + legacy compatibility.
> - A short summary of what changed and why.
