## 1. Path Traversal Protection (Issue #3)
- [x] 1.1 Add filename validation to `PresetsController.GetStationFile` — reject if `filename` contains `..`, `/`, `\`, or any `Path.GetInvalidFileNameChars()`
- [x] 1.2 Add unit test: path traversal attempts return 400
- [x] 1.3 Add unit test: valid filenames still resolve correctly

## 2. SSRF Prevention (Issue #4)
- [x] 2.1 Create a shared `IpAddressValidator` utility in `SoundHub.Domain` (or `SoundHub.Application`) with a method `IsAllowedLanAddress(string ip)` that rejects:
  - Loopback (`127.x.x.x`)
  - Link-local (`169.254.x.x`)
  - Cloud metadata (`169.254.169.254`)
  - IPv6 loopback / link-local
  - Non-private ranges (only allow `10.x.x.x`, `172.16-31.x.x`, `192.168.x.x`) — configurable
- [x] 2.2 Apply validation in `DeviceService.AddDeviceAsync` after hostname resolution
- [x] 2.3 Apply validation in `DeviceService.UpdateDeviceAsync` after hostname resolution
- [x] 2.4 Return 400 from controller when validation fails
- [x] 2.5 Add unit tests for rejected and allowed IP ranges

## 3. Non-root Docker Container (Issue #6)
- [x] 3.1 In `Dockerfile.api`: add a non-root user (`appuser`), `chown /data` to that user, switch to `USER appuser`
- [x] 3.2 Remove `chmod 777 /data`
- [x] 3.3 Verify container starts correctly with `docker-compose up --build`

## 4. Tighten CORS Policy (Issue #9)
- [x] 4.1 In `Program.cs`: replace `AllowAnyHeader()` with `.WithHeaders("Content-Type", "Accept", "Authorization")`
- [x] 4.2 Verify frontend requests still work (Content-Type for JSON, Accept)

## 5. Nginx Security Headers (Issue #10)
- [x] 5.1 In `Dockerfile.web` nginx config, add headers to the `server` block:
  - `add_header X-Content-Type-Options "nosniff" always;`
  - `add_header X-Frame-Options "DENY" always;`
  - `add_header Referrer-Policy "strict-origin-when-cross-origin" always;`
  - `add_header Permissions-Policy "camera=(), microphone=(), geolocation=()" always;`
  - `add_header Content-Security-Policy "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; connect-src 'self' http://api:5001" always;`
- [x] 5.2 Verify frontend loads correctly after adding headers

## 6. Remove HTTPS Redirect (Issue #11)
- [x] 6.1 Remove `app.UseHttpsRedirection();` from `Program.cs`
- [x] 6.2 Add a comment noting TLS is expected to be terminated by reverse proxy

## 7. Input Length Validation (Issue #14)
- [x] 7.1 Add max-length constants (e.g., `MaxDeviceNameLength = 100`, `MaxIpAddressLength = 45`, `MaxPresetNameLength = 100`, `MaxUrlLength = 2048`, `MaxNetworkMaskLength = 18`)
- [x] 7.2 Apply validation in `DevicesController.AddDevice` and `UpdateDevice` for name and IP address
- [x] 7.3 Apply validation in `DevicesController.StorePreset` for preset name, location, icon URL, stream URL
- [x] 7.4 Apply validation in `ConfigController.SetNetworkMask` for network mask
- [x] 7.5 Add unit tests for oversized inputs returning 400

## 8. Secure Key Material in Memory (Issue #15)
- [x] 8.1 Implement `IDisposable` on `EncryptionKeyStore`; in `Dispose()`, zero out `_cachedKey` with `CryptographicOperations.ZeroMemory`
- [x] 8.2 Implement `IDisposable` on `EncryptedSecretsService`; in `Dispose()`, zero out `_encryptionKey`
- [x] 8.3 In `EncryptionKeyStore.RotateKeyAsync`, zero out the old key before replacing
- [x] 8.4 Register both services so the DI container disposes them on shutdown

## 9. Exclude secrets from Docker build context (Issue #16)
- [x] 9.1 Add `secrets/` to `.dockerignore`
- [x] 9.2 Verify build context does not include secrets: `docker image build` should not send `secrets/` files
