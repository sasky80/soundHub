# Change: Scaffold Nx Workspace and .NET API with OpenAPI and Docker

## Why
SoundHub requires a foundational development environment to begin implementation. Establishing a monorepo structure (Nx for Angular), a .NET 8 Web API, containerized deployment, and initial OpenAPI specs will enable rapid feature development, ensure API-first design, and support future consumers (mobile app, MCP server) from day one.

## What Changes
- **Project Structure**: Initialize Nx monorepo with Angular workspace, libraries (feature, data-access, ui), and shared utilities.
- **Backend Foundation**: Scaffold .NET 8 Web API with device adapter pattern, device repository, health checks, and structured logging.
- **API Contract**: First-pass OpenAPI 3.1 specification covering device management (add, remove, list, discover), controls (power, volume, presets, pairing), and status endpoints.
- **Device Adapter Pattern**: Core abstraction for vendor-specific device implementations; initial SoundTouch adapter stub.
- **Docker Deployment**: Multi-stage Dockerfiles for API and web app, `docker-compose.yml` for local dev, and CI-friendly build scripts.
- **Testing Scaffolds**: xUnit test project for backend, Jest setup for frontend, integration test harness for mock SoundTouch adapter.
- **CI/CD Foundation**: GitHub Actions workflow for build, lint, test, and Docker image publish (non-blocking initially).

## Impact
- **Affected Specs (New)**:
  - `project-structure` – Nx workspace layout and conventions
  - `api-foundation` – .NET API architecture and endpoints
  - `device-adapter-pattern` – Device abstraction and vendor implementation
  - `docker-deployment` – Containerization and local dev environment
- **Affected Code**: All future frontend and backend development will follow the established patterns.
- **Breaking Changes**: None (greenfield project).
- **Dependencies Added**: Nx CLI, @nrwl packages, .NET 8 SDK, Docker, OpenAPI tooling.
- **Timeline**: 1–2 sprints for full implementation; deliverable is a working dev environment with hot-reload, automated tests, and docker-compose start-up.
