## 1. Frontend Scaffolding

- [x] 1.1 Initialize Nx workspace (npx create-nx-workspace@latest)
- [x] 1.2 Generate Angular application (apps/web)
- [x] 1.3 Generate library structure (libs/frontend/feature, libs/frontend/data-access, libs/frontend/ui, libs/frontend/shared)
- [x] 1.4 Configure ESLint and Prettier for TypeScript
- [x] 1.5 Set up Jest for unit tests
- [x] 1.6 Create root .editorconfig and .gitignore
- [x] 1.7 Configure Nx cache and CI environment variables

## 2. Backend Scaffolding

- [x] 2.1 Create .NET 8 Web API project (dotnet new webapi)
- [x] 2.2 Set up project structure (Domain, Application, Infrastructure, Presentation layers)
- [x] 2.3 Add device adapter interface (IDeviceAdapter) and registry
- [x] 2.4 Create SoundTouch adapter stub (implementation deferred)
- [x] 2.5 Implement configuration service for devices.json (read/write with vendor grouping)
- [x] 2.6 Implement secrets service with AES-256-CBC encryption (read/write secrets.json)
- [x] 2.7 Create encryption key store (key4.db SQLite NSS key database) with master password from Docker secret
- [x] 2.8 Add file watcher for hot-reload when devices.json changes
- [x] 2.9 Configure xUnit test project and sample tests
- [x] 2.10 Add structured logging (Serilog or built-in ILogger)
- [x] 2.11 Create health check endpoint (/health)
- [x] 2.12 Add CORS configuration (allow frontend + future mobile/MCP)
- [x] 2.13 Enable nullable reference types and analyzers in .csproj
- [x] 2.14 Update unit tests

## 3. OpenAPI Specification

- [x] 3.1 Author OpenAPI 3.1 YAML spec (openapi.yaml or docs/api.yaml)
- [x] 3.2 Define device management endpoints (GET /devices, POST /devices, DELETE /devices/{id}, GET /devices/discover)
- [x] 3.3 Define control endpoints (POST /devices/{id}/power, POST /devices/{id}/volume, etc.)
- [x] 3.4 Define status endpoints (GET /devices/{id}/status)
- [x] 3.5 Define preset endpoints (GET /devices/{id}/presets, POST /devices/{id}/presets, POST /devices/{id}/presets/{id}/play)
- [x] 3.6 Add request/response schemas (Device, Preset, Error, etc.)
- [x] 3.7 Generate OpenAPI documentation and validate against code

## 4. Docker Containerization

- [x] 4.1 Create Dockerfile for .NET API (multi-stage, .NET 8)
- [x] 4.2 Create Dockerfile for Angular web app (multi-stage, Node 20 + nginx)
- [x] 4.3 Create docker-compose.yml for local development (API + web, hot-reload, volumes)
- [x] 4.4 Configure /data volume mount for devices.json, secrets.json, and key4.db
- [x] 4.5 Add Docker secret for master password (development and production)
- [x] 4.6 Add build optimization (layer caching, .dockerignore)
- [x] 4.7 Test docker-compose up locally; confirm health checks pass
- [x] 4.8 Verify configuration files persist across container restarts
- [x] 4.9 Document Docker commands in README

## 5. Testing & CI/CD

- [x] 5.1 Create GitHub Actions workflow for build, lint, test on push/PR
- [x] 5.2 Add Nx workspace lint and affected tests
- [x] 5.3 Set up xUnit test run in CI
- [x] 5.4 Set up Jest test run in CI
- [ ] 5.5 Add optional Docker image build and publish (non-blocking)
- [ ] 5.6 Configure branch protection rules (require CI green)

## 6. Documentation & Onboarding

- [x] 6.1 Create root README.md with tech stack, quick start, and architecture overview
- [x] 6.2 Document Nx workspace conventions (naming, imports, testing)
- [x] 6.3 Document .NET API conventions (namespaces, controller routes, logging)
- [x] 6.4 Create CONTRIBUTING.md with commit conventions (Conventional Commits)
- [x] 6.5 Add inline code comments for device adapter pattern
- [x] 6.6 Create architecture diagram (monorepo, API layers, device adapters)

## 7. Validation & Sign-Off

- [x] 7.1 Verify `docker-compose up` starts without errors
- [x] 7.2 Verify `ng serve` and `dotnet run` work locally
- [x] 7.3 Verify tests pass locally (nx test, dotnet test)
- [x] 7.4 Verify CI pipeline runs all checks
- [x] 7.5 Verify OpenAPI spec is accessible via /swagger or /api-docs
- [x] 7.6 Run openspec validate --strict on the proposal
- [ ] 7.7 Obtain sign-off from tech lead
