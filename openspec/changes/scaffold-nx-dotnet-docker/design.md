## Context
SoundHub is a local-network control application for Bose SoundTouch speakers (extensible to other vendors). The architecture mandates an Nx monorepo for the Angular frontend and a .NET 8 Web API for the backend, both containerized. This change lays the technical foundation—structure, patterns, and tooling—to support API-first design and third-party consumers.

## Goals
- Establish a reproducible monorepo and API scaffold that can be cloned and run locally in minutes (docker-compose up).
- Encode architectural patterns (device adapters, library boundaries, API contract) as code so future features follow convention.
- Define OpenAPI early to guide backend and frontend development in parallel.
- Enable rapid testing and CI feedback with xUnit and Jest out of the box.
- Support future mobile and MCP clients by publishing a stable, discoverable REST API.

## Non-Goals
- Full device control logic (SoundTouch integration is a stub; real implementation follows in a separate change).
- Authentication / authorization (scaffolded as middleware hooks; security design is deferred).
- Performance optimization or load testing.
- Migrate existing code (greenfield).

## Decisions

### 1. Nx Monorepo Layout
**Decision**: Use Nx for the frontend only:
- `apps/web` – Angular application
- `libs/frontend/{feature,data-access,ui,shared}` – Angular libraries by domain
- `libs/shared` – Language-agnostic utilities (e.g., OpenAPI specs, shared DTOs as markdown)

The .NET Web API is maintained in a **separate repository** or folder outside the Nx monorepo.

**Rationale**: 
- Nx is specialized for TypeScript/JavaScript monorepos; keeping .NET separate avoids tooling conflicts and allows independent versioning/deployment.
- Enforces clear separation of concerns: frontend and backend have independent build pipelines, dependencies, and release cycles.
- Simplifies CI/CD: each codebase uses its native toolchain (npm/Nx for frontend, dotnet CLI for backend).
- Reduces monorepo complexity; each team works with familiar tooling.

**Alternatives Considered**:
- Unified monorepo (Nx + .NET) – Adds complexity and requires polyglot build orchestration; Nx is optimized for TypeScript.
- Separate repos entirely – Works but requires separate cloning and coordination; a shared folder or git submodule bridges the gap.

### 2. Device Adapter Pattern with Per-Device Capability Discovery
**Decision**: Backend implements `IDeviceAdapter` interface with per-device capability discovery:
```csharp
public interface IDeviceAdapter
{
    string VendorId { get; }
    Task<IReadOnlySet<string>> GetCapabilitiesAsync(string deviceId, CancellationToken ct);  // Query capabilities per device
    Task<DeviceStatus> GetStatusAsync(string deviceId, CancellationToken ct);
    Task SetPowerAsync(string deviceId, bool on, CancellationToken ct);
    Task SetVolumeAsync(string deviceId, int level, CancellationToken ct);
    Task EnterPairingModeAsync(string deviceId, CancellationToken ct);
    // ... other operations (may throw NotSupportedException if capability absent)
}
```
Implementations (e.g., `SoundTouchAdapter`) query each device to discover its actual capabilities (e.g., via device metadata or introspection). The API caches capabilities per device and uses them to:
- Expose only supported controls to the frontend.
- Return 501 Not Implemented or a descriptive error if an unsupported operation is requested.

**Rationale**: 
- Loose coupling, testability, and painless addition of new vendors.
- **Handles heterogeneous devices**: Different vendors, models, and firmware versions can support different feature sets. Each device reports what it actually supports.
- Per-device discovery accounts for device-level variation within the same vendor (e.g., Bose SoundTouch Gen 1 vs Gen 2 may have different capabilities).
- Frontend can query device capabilities and conditionally render UI controls (e.g., hide volume slider if device doesn't support it).
- Extensible: new capabilities can be added without breaking existing adapters.

**Alternatives Considered**:
- Vendor-level capability set only – Too coarse; doesn't account for per-device variation.
- Monolithic switch statement – Tightly coupled, unmaintainable at scale.
- One service per vendor – Redundant; adapter pattern is cleaner.
- Fixed interface with all methods required – Brittle; forces dummy implementations for unsupported features.

### 3. Docker Multi-Stage Builds
**Decision**: Use multi-stage Dockerfiles:
- API: `mcr.microsoft.com/dotnet/aspnet:8.0` base, 2-stage (build + runtime).
- Web: Node 20 build stage, nginx runtime stage.

**Rationale**: Minimal final images, security (no build tools in production), and fast layer caching.

### 4. OpenAPI Generation
**Decision**: Author OpenAPI manually in YAML (not auto-generated from code) to drive API-first design. Tooling will validate endpoint implementations against the spec.

**Rationale**: API is the contract; code should implement spec, not vice versa. Manual authoring ensures clarity and foresight.

**Alternatives Considered**:
- Swagger.AspNetCore auto-generation – Gets out of sync if not disciplined; puts implementation before contract.

### 5. Local Dev Environment
**Decision**: `docker-compose.yml` spins up API and web with hot-reload:
- API: volume-mounted source, dotnet watch for auto-reload.
- Web: Nx serve on port 4200, proxied via docker-compose.

**Rationale**: Zero-install for developers (just Docker); parity with CI/production.

**Alternatives Considered**:
- Local .NET/Node development – Works but diverges from container-based CI/prod.

### 6. Device Persistence and Secrets Storage
**Decision**: File-based configuration with encrypted secrets on mounted volumes:

**Configuration Structure:**
1. **devices.json** (plain, on mounted volume):
```json
{
  "SoundTouch": {
    "Devices": [
      {
        "Name": "Living Room Speaker",
        "IpAddress": "192.168.1.131"
      },
      {
        "Name": "Bedroom Soundbar",
        "IpAddress": "192.168.1.130"
      }
    ]
  }
}
```

2. **secrets.json** (encrypted values, on mounted volume):
```json
[
  {
    "SecretName": "SpotifyAccountPassword",
    "SecretValue": "AES-256-CBC-encrypted-value"
  }
]
```

**Encryption Architecture:**
- Algorithm: AES-256-CBC for secret values
- Encryption key: Stored in `key4.db` (SQLite database)
- Master password: Retrieved from Docker secret (secure, not in codebase)
- Key derivation: Master password → encryption key (using PBKDF2 or similar)

**Implementation:**
```csharp
public interface ISecretsService
{
    Task<string> GetSecretAsync(string secretName);
    Task SetSecretAsync(string secretName, string value);
}

// Encryption key loaded from key4.db, protected by master password
public class EncryptionKeyStore
{
    private readonly string _masterPassword; // from Docker secret
    public async Task<byte[]> GetEncryptionKeyAsync();
}
```

**File Layout:**
```
/data/
  ├── devices.json          # Device configurations (plain)
  ├── secrets.json          # Encrypted secrets
  └── key4.db              # SQLite: encryption key storage
```

**Rationale:**
- **Simplicity**: No external database required; files on mounted volumes persist across container restarts.
- **Portability**: Configuration can be backed up, versioned, or migrated by copying files.
- **Security**: Secrets are encrypted with AES-256-CBC; encryption key is protected by master password from Docker secret (not exposed in config).
- **Separation**: Device metadata (non-sensitive) is separate from secrets (encrypted).
- **Docker-native**: Leverages Docker secrets/volumes for production security.

**Master Password Delivery:**
- Development: Environment variable or file mount (e.g., `/run/secrets/master_password`)
- Production: Docker secret or Kubernetes secret mounted at runtime

**Future Enhancements**:
- Credential rotation: re-encrypt secrets with a new encryption key
- Audit logging for secret access
- Support for external secrets managers (Azure Key Vault) as an alternative backend

**Alternatives Considered**:
- SQL database – Adds complexity; file-based is sufficient for device counts < 10K.
- Plaintext secrets – Security risk; never acceptable.
- Separate secrets manager (Vault) – Over-engineered for initial scope; can be added later.
- In-memory only – Lost on restart; unsuitable.

## Risks / Trade-offs

| Risk | Mitigation |
|------|-----------|
| Monorepo size grows; Nx cache invalidation slows CI | Use Nx cloud or self-hosted cache; enforce strict module boundaries early |
| OpenAPI spec drift from implementation | Automated checks in CI (validate endpoints exist) + code review discipline |
| Docker layer caching misses during dev | Volume-mount source; dotnet watch / ng serve handle recompilation |
| Device adapter pattern adds indirection | Minimal overhead; clarity and extensibility outweigh performance cost |

## Migration Plan
- This is a greenfield change; no migration needed.
- Future device implementations will follow the adapter pattern.
- Future authentication/authorization will hook into middleware (already scaffolded as comments).

## Open Questions
- Which CI/CD platform (GitHub Actions, Azure Pipelines)? → Proposal uses GitHub Actions as default; easy to swap.
- Publish OpenAPI to a developer portal (e.g., Stoplight, SwaggerHub)? → Defer to future change; spec will be in repo.
- Which secrets manager for production (Azure Key Vault, HashiCorp Vault, AWS Secrets Manager)? → Architecture supports all; choice deferred to infrastructure planning.
