# Contributing to SoundHub

Thank you for your interest in contributing to SoundHub! This document provides guidelines and conventions for development.

## 📋 Table of Contents

- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Code Conventions](#code-conventions)
- [Commit Conventions](#commit-conventions)
- [Testing Guidelines](#testing-guidelines)
- [Pull Request Process](#pull-request-process)

## 🚀 Getting Started

### Prerequisites

- .NET 9 SDK
- Node.js 20+
- Docker & Docker Compose
- Git

### Setup

1. Fork and clone the repository
2. Install dependencies:
   ```bash
   # Backend
   dotnet restore
   
   # Frontend
   cd frontend
   npm install
   ```

3. Run locally:
   ```bash
   # Option 1: Docker Compose
   docker-compose up --build
   
   # Option 2: Manual
   # Terminal 1 (API):
   dotnet run --project src/SoundHub.Api
   
   # Terminal 2 (Web):
   cd frontend
   npx nx serve web
   ```

## 🔄 Development Workflow

1. Create a feature branch: `git checkout -b feature/your-feature-name`
2. Make your changes following code conventions
3. Write or update tests
4. Run tests locally: `dotnet test && npx nx test`
5. Commit with conventional commit messages
6. Push and create a pull request

## 💻 Code Conventions

### .NET Backend

**Architecture Layers:**
- **Domain**: Entities, interfaces, value objects (no external dependencies)
- **Application**: Business logic, service orchestration
- **Infrastructure**: Implementations (repositories, adapters, external APIs)
- **Presentation (Api)**: Controllers, DTOs, middleware

**Naming:**
- Interfaces: `IDeviceAdapter`, `IDeviceRepository`
- Implementations: `SoundTouchAdapter`, `FileDeviceRepository`
- Controllers: `DevicesController` (plural)
- DTOs: `AddDeviceRequest`, `DeviceResponse`

**Code Style:**
- Enable nullable reference types (`<Nullable>enable</Nullable>`)
- Use `required` for required properties
- Prefer `readonly` and immutability
- Use `async`/`await` for I/O operations
- Always pass `CancellationToken` for async methods

**Example:**
```csharp
public class DeviceService
{
    private readonly IDeviceRepository _repository;
    private readonly ILogger<DeviceService> _logger;

    public DeviceService(IDeviceRepository repository, ILogger<DeviceService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Device?> GetDeviceAsync(string id, CancellationToken ct = default)
    {
        return await _repository.GetDeviceAsync(id, ct);
    }
}
```

### Angular Frontend

**Structure:**
- **Feature Libraries**: Domain-specific features (e.g., device-management)
- **Data Access**: Services, state management, HTTP clients
- **UI Libraries**: Shared components (buttons, cards, etc.)
- **Shared**: Utilities, types, constants

**Naming:**
- Components: `device-list.component.ts` (kebab-case)
- Services: `device.service.ts`
- Imports: Use barrel exports (`@soundhub/frontend/feature`)

**Code Style:**
- Use standalone components
- Prefer signals over RxJS where appropriate
- Follow Angular style guide
- Use strict TypeScript

**Example:**
```typescript
import { Component, inject } from '@angular/core';
import { DeviceService } from '@soundhub/frontend/data-access';

@Component({
  selector: 'app-device-list',
  standalone: true,
  templateUrl: './device-list.component.html',
})
export class DeviceListComponent {
  private deviceService = inject(DeviceService);
  devices = this.deviceService.getDevices();
}
```

### Nx Workspace

**Library Boundaries:**
- `type:feature` can import from `type:data-access`, `type:ui`, `type:util`
- `type:data-access` can import from `type:util`
- `type:ui` can import from `type:util`
- `type:util` cannot import from other libraries

**Enforce with ESLint:**
```json
{
  "rules": {
    "@nx/enforce-module-boundaries": "error"
  }
}
```

## 📝 Commit Conventions

We use [Conventional Commits](https://www.conventionalcommits.org/) for clear commit messages and automated changelog generation.

**Format:**
```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation only
- `style`: Code style (formatting, no logic change)
- `refactor`: Code change that neither fixes a bug nor adds a feature
- `perf`: Performance improvement
- `test`: Adding or updating tests
- `chore`: Build process, dependencies, tooling

**Scopes:**
- `api`: Backend API changes
- `frontend`: Angular/web changes
- `domain`: Domain layer
- `infra`: Infrastructure layer
- `docker`: Docker/deployment
- `ci`: CI/CD pipeline

**Examples:**
```bash
feat(api): add device discovery endpoint

Implements GET /api/devices/discover to scan the local network for SoundTouch devices.
Uses HTTP probing on port 8090 with /info endpoint.

Closes #42
```

```bash
fix(frontend): resolve device list not updating after delete

The device list component was not reloading data after a device was removed.
Added a refresh call in the delete handler.
```

## 🧪 Testing Guidelines

### Backend Tests

**Unit Tests (xUnit):**
- Test business logic in isolation
- Mock dependencies with `Moq` or `NSubstitute`
- Use `FluentAssertions` for readable assertions

**Example:**
```csharp
public class DeviceServiceTests
{
    [Fact]
    public async Task GetDeviceAsync_WhenDeviceExists_ReturnsDevice()
    {
        // Arrange
        var mockRepo = new Mock<IDeviceRepository>();
        var device = new Device { Id = "123", Name = "Test" };
        mockRepo.Setup(r => r.GetDeviceAsync("123", default)).ReturnsAsync(device);
        var service = new DeviceService(mockRepo.Object, Mock.Of<ILogger<DeviceService>>());

        // Act
        var result = await service.GetDeviceAsync("123");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
    }
}
```

### Frontend Tests

**Unit Tests (Jest):**
- Test components in isolation
- Mock services with `jest.fn()`
- Use `@testing-library/angular` for component testing

**Example:**
```typescript
describe('DeviceListComponent', () => {
  it('should load devices on init', async () => {
    const mockService = { getDevices: jest.fn().mockReturnValue([]) };
    
    await render(DeviceListComponent, {
      providers: [{ provide: DeviceService, useValue: mockService }]
    });

    expect(mockService.getDevices).toHaveBeenCalled();
  });
});
```

### Running Tests

```bash
# Backend: all tests
dotnet test

# Backend: specific project
dotnet test tests/SoundHub.Tests

# Frontend: all tests
cd frontend
npx nx test

# Frontend: specific library
npx nx test feature

# Frontend: watch mode
npx nx test feature --watch
```

## 🔀 Pull Request Process

1. **Ensure CI passes**: All tests, linting, and build checks must succeed
2. **Update documentation**: Update README or inline docs if needed
3. **Add tests**: New features require tests
4. **Describe changes**: Provide a clear description and link related issues
5. **Request review**: Tag relevant maintainers for review
6. **Address feedback**: Make requested changes promptly
7. **Squash commits** (optional): Maintainers may squash before merging

### PR Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
How was this tested?

## Checklist
- [ ] Tests pass locally
- [ ] Code follows project conventions
- [ ] Documentation updated
- [ ] Commit messages follow conventions

## Related Issues
Closes #123
```

## 📚 Additional Resources

- [Angular Style Guide](https://angular.io/guide/styleguide)
- [.NET Clean Architecture](https://github.com/jasontaylordev/CleanArchitecture)
- [Nx Documentation](https://nx.dev)
- [Conventional Commits](https://www.conventionalcommits.org/)

## ❓ Questions?

Open an issue or ask in discussions!

---

Thank you for contributing to SoundHub! 🎉
