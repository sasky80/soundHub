## ADDED Requirements

### Requirement: API Docker Image
The system SHALL build and containerize the .NET Web API as a multi-stage Docker image optimized for production.

#### Scenario: Build API image
- **WHEN** `docker build -f Dockerfile.api -t soundhub-api .` is executed
- **THEN** a Docker image is created with the compiled .NET 8 API
- **AND** the image size is < 200 MB (runtime-only, no build tools)

#### Scenario: API container starts successfully
- **WHEN** the API container is started with `docker run -p 5000:5000 soundhub-api`
- **THEN** the API listens on port 5000 and responds to /health
- **AND** logs are written to stdout in JSON format

### Requirement: Web App Docker Image
The system SHALL build and containerize the Angular web application as a multi-stage image with nginx.

#### Scenario: Build web app image
- **WHEN** `docker build -f Dockerfile.web -t soundhub-web .` is executed
- **THEN** a Docker image is created with the compiled Angular app served by nginx
- **AND** the image size is < 100 MB

#### Scenario: Web app container serves assets
- **WHEN** the web container is started with `docker run -p 80:80 soundhub-web`
- **THEN** the Angular application is accessible at http://localhost/
- **AND** static assets load without 404 errors

### Requirement: Docker Compose Development Environment
The system SHALL provide a docker-compose.yml file that orchestrates API and web services for local development.

#### Scenario: Start development environment
- **WHEN** a developer runs `docker-compose up` in the project root
- **THEN** both API and web containers start
- **AND** the web app is accessible at http://localhost:4200 (with hot-reload)
- **AND** the API is accessible at http://localhost:5000

#### Scenario: Hot-reload in Docker Compose
- **WHEN** a developer modifies Angular source code or .NET controller code
- **THEN** the running containers detect changes and recompile/reload
- **AND** the browser or API automatically reflects the changes within 2 seconds

#### Scenario: Volume mounts for development
- **WHEN** docker-compose starts, source directories are mounted as volumes
- **THEN** changes to `apps/web` and `src/SoundHub.Api` are reflected immediately
- **AND** no container rebuild is needed for code changes

#### Scenario: Database and cache services (future)
- **WHEN** docker-compose is configured with database and cache services
- **THEN** the API can connect to these services via hostname (e.g., db, redis)
- **AND** data persists across container restarts (via volumes)

### Requirement: Docker Build Optimization
The system SHALL use best practices for Docker image building (layer caching, .dockerignore).

#### Scenario: Efficient layer caching
- **WHEN** a Dockerfile copies source files after a dependency layer
- **THEN** changing only source code invalidates only the source layer
- **AND** dependency layers are cached and reused across builds

#### Scenario: .dockerignore excludes unnecessary files
- **WHEN** Docker builds an image
- **THEN** node_modules, bin, obj, .git, and other large folders are excluded
- **AND** the build context size is minimized

### Requirement: Local Development Documentation
The system SHALL document how to build, run, and troubleshoot the Docker environment.

#### Scenario: Developer follows quick start guide
- **WHEN** a developer reads the README or CONTRIBUTING guide
- **THEN** it includes steps to clone, build, and run docker-compose up
- **AND** expected output and troubleshooting tips are provided

#### Scenario: Ports and networking documented
- **WHEN** a developer needs to customize ports or network configuration
- **THEN** the docker-compose.yml includes comments explaining each service and port
- **AND** a section in README lists ports and service URLs

### Requirement: Health Checks in Docker
The system SHALL configure health checks in docker-compose to verify service availability.

#### Scenario: API health check
- **WHEN** the API container is running
- **THEN** docker-compose periodically calls GET /health
- **AND** the container is marked as healthy only if /health returns 200 OK
- **AND** docker-compose waits for the health check before starting dependent services

#### Scenario: Container unhealthy state
- **WHEN** the API health check fails repeatedly
- **THEN** docker-compose logs the failure and the container may be restarted
- **AND** a developer can inspect logs to diagnose the issue

### Requirement: Multi-Environment Docker Configuration
The system SHALL support different docker-compose configurations for development, testing, and production.

#### Scenario: Development vs production compose files
- **WHEN** a developer uses `docker-compose.yml` for dev with hot-reload
- **THEN** a separate `docker-compose.prod.yml` exists for production without volumes
- **AND** environment variables and logging are optimized per environment

### Requirement: CI/CD Docker Integration
The system SHALL enable automated Docker image builds and publishing in CI pipelines.

#### Scenario: Build and test image in CI
- **WHEN** a PR is submitted
- **THEN** the CI pipeline builds both API and web images
- **AND** runs basic smoke tests (container starts, health check passes)
- **AND** optionally pushes images to a registry (e.g., Docker Hub, GitHub Container Registry)
