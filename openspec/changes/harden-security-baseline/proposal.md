# Change: Harden Security Baseline

## Why
A security review identified multiple vulnerabilities across the API, Docker infrastructure, and nginx configuration. These range from HIGH (path traversal, SSRF) to LOW (missing `.dockerignore` entries) severity and need to be addressed to bring the project to a reasonable security posture for a LAN-deployed service.

## What Changes
- **#3 Path traversal in preset file serving** — Validate `filename` parameter in `PresetsController` to reject path traversal sequences (`..`, `/`, `\`).
- **#4 SSRF via user-supplied IP addresses** — Validate IP addresses in `AddDevice` and `UpdateDevice` to reject loopback, link-local, cloud metadata, and non-LAN ranges.
- **#6 Container runs as root** — Add non-root user to `Dockerfile.api`, fix `/data` directory permissions (remove `chmod 777`).
- **#9 Broad CORS policy** — Tighten CORS: replace `AllowAnyHeader()` with an explicit allowlist; keep `AllowCredentials` only if needed.
- **#10 Missing security headers in nginx** — Add `X-Content-Type-Options`, `X-Frame-Options`, `Content-Security-Policy`, `Referrer-Policy`, and `Permissions-Policy` to the nginx config in `Dockerfile.web`.
- **#11 HTTPS redirect without TLS** — Remove `app.UseHttpsRedirection()` since the container serves HTTP only; TLS is expected to be terminated by a reverse proxy upstream.
- **#14 No input length validation** — Add max-length checks on device names, IP addresses, preset names, stream URLs, and network masks.
- **#15 Encryption keys never cleared from memory** — Zero out cached encryption key bytes in `EncryptionKeyStore` and `EncryptedSecretsService` when rotated or on disposal; implement `IDisposable`.
- **#16 secrets/ not in `.dockerignore`** — Add `secrets/` to `.dockerignore` to prevent secrets directory from being sent to the Docker build context.

## Impact
- Affected specs: `api-device-control`
- Affected code:
  - `services/SoundHub.Api/Controllers/PresetsController.cs`
  - `services/SoundHub.Api/Controllers/DevicesController.cs`
  - `services/SoundHub.Api/Program.cs`
  - `services/SoundHub.Application/Services/DeviceService.cs`
  - `services/SoundHub.Infrastructure/Services/EncryptionKeyStore.cs`
  - `services/SoundHub.Infrastructure/Services/EncryptedSecretsService.cs`
  - `Dockerfile.api`
  - `Dockerfile.web`
  - `.dockerignore`
