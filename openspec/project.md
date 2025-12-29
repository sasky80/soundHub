# Project Context

## Purpose
SoundHub provides a unified way to control home sound devices, starting with Bose SoundTouch speakers. Initial goals:
- Offer a simple web UI and API to manage devices on a local network.
- Support for Internationalization.
- Support core controls: power, volume, Bluetooth pairing mode, presets (configure + play), and device status.
- Provide administration tools for adding/removing devices and discovering devices on LAN.
- Design the platform to be API-first so future mobile apps and an MCP server can integrate without changes to the backend.
- Keep the architecture extensible to support additional brands in future iterations.

## Tech Stack
- Frontend: Angular (Nx monorepo)
- Backend: .NET 8+ (Web API)
- Containerization: Docker (dev and deployment), optional Docker Compose for local setup
- API Contract: OpenAPI (planned)
- CI/CD: TBD (GitHub Actions/Azure DevOps, to be chosen)

## Project Conventions

### Code Style
- TypeScript: ESLint (Nx presets), Prettier, strict TypeScript settings.
- .NET: `nullable enable`, EditorConfig-based formatting, analyzers (treat warnings as build warnings at minimum).
- Commit messages: Conventional Commits (feat, fix, chore, docs, refactor, test, ci, build, perf, revert).
- Documentation: Keep architecture and decisions in `openspec/` with concise markdown; prefer ADR-style entries under `openspec/changes/` when introducing significant changes.

### Architecture Patterns
- API-first: Backend exposes REST endpoints designed for both the Angular app and future consumers (mobile app, MCP server).
- Separation of concerns:
	- Backend: device-agnostic domain with device adapters for specific vendors (start with Bose SoundTouch).
	- Frontend: Nx libraries by domain (feature, data-access, ui) and shared utilities.
- Extensibility: plugin-like device adapter layer to add more vendors later without impacting core API routes.
- Configuration: support manual device registration and LAN discovery; persist device metadata (implementation detail TBD).
- Observability: structured logging; basic request tracing (IDs); health endpoints.

### Testing Strategy
- Frontend: unit tests for components/services (Jest via Nx); lightweight e2e for critical flows (Cypress or Playwright, TBD).
- Backend: unit tests for domain/services (xUnit/NUnit, TBD); integration tests against a mock SoundTouch API or a controllable test double.
- Contract tests: generate and validate OpenAPI; basic schema checks in CI.
- Smoke tests: containerized environment boots, API health, and a no-op device list returns successfully.

### Git Workflow
- Branching: trunk-based with short-lived feature branches.
- Reviews: PRs required for main; status checks green before merge.
- Releases: semantic versioning; tag releases; changelog derived from Conventional Commits.
- Protections: do not commit secrets; use environment variables and secret stores in CI/CD.

## Domain Context
- Scope (initial): Bose SoundTouch speakers only.
- Capabilities:
	- Administration: add/remove devices; discover devices on LAN.
	- Control: power, volume, enter Bluetooth pairing mode, configure/play presets (e.g., internet radio), view device status.
- Consumers: Angular web app now; future mobile app and MCP server will consume the same REST API.
- Networking: Local LAN control; devices must be reachable from backend host. Discovery methods are vendor-specific; goal is to support discovery for SoundTouch and fall back to manual entry.

Key API Reference
- SoundTouch WebServices API: https://github.com/thlucas1/homeassistantcomponent_soundtouchplus/wiki/SoundTouch-WebServices-API

## Important Constraints
- Backend must run on .NET 8+ and expose a stable REST API suitable for third-party clients.
- LAN-first design: no cloud dependency assumed for core controls.
- Security: local-first; protect write operations (e.g., auth to be defined); enable CORS for allowed clients.
- Extensibility: architect device support behind an adapter interface; avoid leaking vendor specifics into public API shapes.
- Containers: both frontend and backend run in Docker for dev and deployment; ensure zero-configuration local start where possible.

## External Dependencies
- Bose SoundTouch devices present on the same LAN as the backend host.
- SoundTouch WebServices API (linked above) for device control and status.
- Discovery: support a LAN discovery mechanism compatible with SoundTouch (exact approach to be validated) with manual device entry as a fallback.
- Frontend tooling: Nx workspace for Angular; ESLint/Prettier.
- Backend tooling: .NET 8 SDK, testing framework (xUnit/NUnit), logging.
