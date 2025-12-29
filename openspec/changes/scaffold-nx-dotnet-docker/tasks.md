## 1. Frontend Scaffolding

- [ ] 1.1 Initialize Nx workspace (npx create-nx-workspace@latest)
- [ ] 1.2 Generate Angular application (apps/web)
- [ ] 1.3 Generate library structure (libs/frontend/feature, libs/frontend/data-access, libs/frontend/ui, libs/frontend/shared)
- [ ] 1.4 Configure ESLint and Prettier for TypeScript
- [ ] 1.5 Set up Jest for unit tests
- [ ] 1.6 Create root .editorconfig and .gitignore
- [ ] 1.7 Configure Nx cache and CI environment variables

## 2. Backend Scaffolding

- [ ] 2.1 Create .NET 8 Web API project (dotnet new webapi)
- [ ] 2.2 Set up project structure (Domain, Application, Infrastructure, Presentation layers)
- [ ] 2.3 Add device adapter interface (IDeviceAdapter) and registry
- [ ] 2.4 Create SoundTouch adapter stub (implementation deferred)
- [ ] 2.5 Implement configuration service for devices.json (read/write with vendor grouping)
- [ ] 2.6 Implement secrets service with AES-256-CBC encryption (read/write secrets.json)
- [ ] 2.7 Create encryption key store (key4.db SQLite) with master password from Docker secret
- [ ] 2.8 Add file watcher for hot-reload when devices.json changes
- [ ] 2.9 Configure xUnit test project and sample tests
- [ ] 2.10 Add structured logging (Serilog or built-in ILogger)
- [ ] 2.11 Create health check endpoint (/health)
- [ ] 2.12 Add CORS configuration (allow frontend + future mobile/MCP)
- [ ] 2.13 Enable nullable reference types and analyzers in .csproj

## 3. OpenAPI Specification

- [ ] 3.1 Author OpenAPI 3.1 YAML spec (openapi.yaml or docs/api.yaml)
- [ ] 3.2 Define device management endpoints (GET /devices, POST /devices, DELETE /devices/{id}, GET /devices/discover)
- [ ] 3.3 Define control endpoints (POST /devices/{id}/power, POST /devices/{id}/volume, etc.)
- [ ] 3.4 Define status endpoints (GET /devices/{id}/status)
- [ ] 3.5 Define preset endpoints (GET /devices/{id}/presets, POST /devices/{id}/presets, POST /devices/{id}/presets/{id}/play)
- [ ] 3.6 Add request/response schemas (Device, Preset, Error, etc.)
- [ ] 3.7 Generate OpenAPI documentation and validate against code

## 4. Docker Containerization

- [ ] 4.1 Create Dockerfile for .NET API (multi-stage, .NET 8)
- [ ] 4.2 Create Dockerfile for Angular web app (multi-stage, Node 20 + nginx)
- [ ] 4.3 Create docker-compose.yml for local development (API + web, hot-reload, volumes)
- [ ] 4.4 Configure /data volume mount for devices.json, secrets.json, and key4.db
- [ ] 4.5 Add Docker secret for master password (development and production)
- [ ] 4.6 Add build optimization (layer caching, .dockerignore)
- [ ] 4.7 Test docker-compose up locally; confirm health checks pass
- [ ] 4.8 Verify configuration files persist across container restarts
- [ ] 4.9 Document Docker commands in README

## 5. Testing & CI/CD

- [ ] 5.1 Create GitHub Actions workflow for build, lint, test on push/PR
- [ ] 5.2 Add Nx workspace lint and affected tests
- [ ] 5.3 Set up xUnit test run in CI
- [ ] 5.4 Set up Jest test run in CI
- [ ] 5.5 Add optional Docker image build and publish (non-blocking)
- [ ] 5.6 Configure branch protection rules (require CI green)

## 6. Documentation & Onboarding

- [ ] 6.1 Create root README.md with tech stack, quick start, and architecture overview
- [ ] 6.2 Document Nx workspace conventions (naming, imports, testing)
- [ ] 6.3 Document .NET API conventions (namespaces, controller routes, logging)
- [ ] 6.4 Create CONTRIBUTING.md with commit conventions (Conventional Commits)
- [ ] 6.5 Add inline code comments for device adapter pattern
- [ ] 6.6 Create architecture diagram (monorepo, API layers, device adapters)

## 7. Validation & Sign-Off

- [ ] 7.1 Verify `docker-compose up` starts without errors
- [ ] 7.2 Verify `ng serve` and `dotnet run` work locally
- [ ] 7.3 Verify tests pass locally (nx test, dotnet test)
- [ ] 7.4 Verify CI pipeline runs all checks
- [ ] 7.5 Verify OpenAPI spec is accessible via /swagger or /api-docs
- [ ] 7.6 Run openspec validate --strict on the proposal
- [ ] 7.7 Obtain sign-off from tech lead
