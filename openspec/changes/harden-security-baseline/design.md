## Context
Security audit identified 9 issues (severity HIGH to LOW) in the API, Docker infrastructure, and nginx configuration. All changes are backward-compatible — no API contract changes, no new endpoints, no schema migrations.

## Goals / Non-Goals
- Goals:
  - Prevent path traversal, SSRF, and oversized-input abuse
  - Harden container and nginx configuration
  - Properly manage cryptographic key material in memory
  - Tighten CORS to explicit headers
- Non-Goals:
  - Adding authentication/authorization (separate effort, issues #1/#2)
  - Adding rate limiting (separate effort, issue #5)
  - Changing encryption algorithm for key wrapping from AES-CBC to AES-GCM (issue #7 — requires key migration)
  - Changing `AllowedHosts` (issue #8 — needs environment-specific config strategy)

## Decisions

### IP Address Validation (SSRF)
- **Decision:** Allowlist private RFC 1918 ranges only (`10.0.0.0/8`, `172.16.0.0/12`, `192.168.0.0/16`). Reject everything else after hostname resolution.
- **Alternatives considered:**
  - Blocklist approach (reject known bad IPs) — fragile, easy to bypass.
  - No validation — current state, allows probing internal services.
- **Rationale:** LAN-first design means only private IPs are valid targets. Allowlisting is more restrictive and aligns with project constraints.

### Path Traversal Protection
- **Decision:** Validate at controller level before passing to service. Reject filenames containing `..`, `/`, `\`, or characters from `Path.GetInvalidFileNameChars()`.
- **Rationale:** Defense in depth — even though `Path.Combine` is used downstream, validating early prevents future regressions if file serving logic changes.

### Non-root Container
- **Decision:** Add `appuser` (UID 1001) in Dockerfile, `chown /data` to that user, `USER appuser`.
- **Rationale:** Principle of least privilege. The app doesn't need root.

### HTTPS Redirect Removal
- **Decision:** Remove `UseHttpsRedirection()` entirely. Document that TLS termination is the responsibility of the reverse proxy (e.g., nginx/Caddy on the host).
- **Rationale:** The API container only binds HTTP port 5001. HTTPS redirect without actual TLS causes 307 redirect loops.

### Key Material Zeroing
- **Decision:** Use `CryptographicOperations.ZeroMemory(span)` on cached key byte arrays in `Dispose()` and during key rotation.
- **Rationale:** .NET best practice for sensitive key material. Limits exposure window if process memory is dumped.

## Risks / Trade-offs
- **IP allowlist may be too restrictive** — If someone uses a non-RFC-1918 private range (unlikely for home use), they'd need to adjust the validator. Mitigation: make the allowed ranges configurable via `appsettings.json` in a follow-up if needed.
- **CSP may break future features** — The nginx Content-Security-Policy is strict. New external resources (CDN fonts, analytics) would need CSP updates. Mitigation: CSP is in the Dockerfile, easy to update.
- **`IDisposable` on singletons** — The DI container disposes singletons at shutdown, which is when key zeroing happens. Keys remain in memory during app lifetime. This is a partial mitigation, not a complete solution.

## Open Questions
- None — all decisions are straightforward for the scope of this change.
