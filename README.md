# SoundHub

SoundHub is a local-network control application for smart audio devices, starting with Bose SoundTouch speakers. Built with a modern tech stack featuring Angular frontend, .NET 8 backend, and Docker deployment.

## 🎯 Features

- **Device Management**: Add, remove, and discover smart audio devices on your local network
- **Device Control**: Power, volume, presets, and Bluetooth pairing
- **Web Interface**: Modern landing page with device list, settings, and device control
- **Internationalization**: Runtime language switching (English and Polish)
- **Vendor Abstraction**: Extensible device adapter pattern for supporting multiple vendors
- **Secure Secrets**: AES-256-CBC encrypted secrets storage
- **File-Based Configuration**: Simple devices.json for device metadata
- **REST API**: Well-documented OpenAPI/Swagger endpoints
- **Containerized**: Docker-ready for easy deployment

## 🌐 Web UI Routes

| Route | Description |
|-------|-------------|
| `/` | Landing page – displays list of configured devices |
| `/settings` | Settings page – language selection, navigation to device config |
| `/settings/devices` | Device configuration page – manage configured devices |
| `/devices/:id` | Device details page – view and control a specific device (power toggle) |

## 🏗️ Architecture

For a detailed architecture overview including layered diagrams, monorepo structure, device adapter pattern, and data flow, see [docs/architecture.md](docs/architecture.md).

### Tech Stack

**Frontend:**
- Angular (standalone components)
- Nx monorepo for code organization
- TypeScript, SCSS
- Jest for testing

**Backend:**
- .NET 8 Web API
- Clean Architecture (Domain, Application, Infrastructure, Presentation)
- Structured logging (JSON format)
- Health checks for Docker
- Swashbuckle/OpenAPI documentation

**Deployment:**
- Docker & Docker Compose
- Multi-stage builds for optimized images
- Volume mounts for persistent data
- Health checks and auto-restart

### Project Structure

```
soundHub/
├── frontend/                          # Nx Angular workspace
│   ├── src/                           # Main Angular application
│   ├── e2e/                           # Playwright E2E tests
│   └── libs/frontend/                 # Shared libraries
│       ├── feature/                   # Feature modules
│       ├── data-access/               # Services & state management
│       ├── ui/                        # UI components
│       └── shared/                    # Utilities & types
├── services/                          # .NET backend solution
│   ├── SoundHub.Api/                  # Web API controllers & startup
│   ├── SoundHub.Application/          # Business logic & services
│   ├── SoundHub.Domain/               # Entities & interfaces
│   ├── SoundHub.Infrastructure/       # Adapters & persistence
│   └── tests/SoundHub.Tests/          # xUnit test project
├── data/                              # Volume mount for config & secrets
├── docs/                              # Documentation & diagrams
│   └── architecture.md                # Architecture overview with Mermaid diagrams
├── openspec/                          # OpenSpec change proposals
├── docker-compose.yml                 # Local development environment
├── Dockerfile.api                     # API container
└── Dockerfile.web                     # Web container
```

### Device Adapter Pattern

The core abstraction for vendor-specific device control:

```csharp
public interface IDeviceAdapter
{
    string VendorId { get; }
    Task<IReadOnlySet<string>> GetCapabilitiesAsync(string deviceId);
    Task<DeviceStatus> GetStatusAsync(string deviceId);
    Task SetPowerAsync(string deviceId, bool on);
    // ... other control methods
}
```

Each vendor (e.g., Bose SoundTouch) implements this interface. The adapter registry resolves the correct implementation at runtime.

## 🚀 Quick Start

### Prerequisites

- Docker & Docker Compose
- (Optional) .NET 8 SDK for local API development
- (Optional) Node.js 20+ for local frontend development

### Run with Docker Compose

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/soundHub.git
   cd soundHub
   ```

2. **Start all services**
   ```bash
   docker-compose up --build
   ```

3. **Access the application**
   - Web UI: http://localhost:4200
   - API: http://localhost:5000
   - Swagger UI: http://localhost:5000/swagger

4. **Stop services**
   ```bash
   docker-compose down
   ```

### Local Development (Without Docker)

**Backend (.NET API):**
```bash
cd soundHub
dotnet build
dotnet run --project src/SoundHub.Api
```

**Frontend (Angular):**
```bash
cd frontend
npm install
npx nx serve web
```

## 📝 Configuration

### Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | ASP.NET Core environment |
| `DevicesFilePath` | `/data/devices.json` | Path to device configuration |
| `SecretsFilePath` | `/data/secrets.json` | Path to encrypted secrets |
| `MasterPasswordFile` | `/run/secrets/master_password` | Path to Docker secret file containing master password |
| `MasterPassword` | `default-dev-password` | Fallback master password (used when file not available) |

### Docker Secrets (Recommended for Production)

The master password for encrypting secrets is managed via Docker secrets:

**Development Setup:**
1. Create a secrets directory and password file:
   ```bash
   mkdir -p secrets
   echo "your-secure-password" > secrets/master_password.txt
   ```

2. The `docker-compose.yml` is pre-configured to use this file as a Docker secret.

**Production Setup (Docker Swarm):**
```bash
# Create a Docker secret
echo "your-production-password" | docker secret create master_password -

# Reference in docker-compose.yml:
secrets:
  master_password:
    external: true
```

**How it works:**
- Docker mounts the secret file at `/run/secrets/master_password` inside the container
- The API reads the password from this file (via `MasterPasswordFile` configuration)
- If the file doesn't exist, it falls back to the `MasterPassword` environment variable

### Configuration Files

**`data/devices.json`** - Device metadata (vendor-grouped):
```json
{
  "bose-soundtouch": {
    "Devices": [
      {
        "Id": "...",
        "Name": "Living Room Speaker",
        "IpAddress": "192.168.1.131",
        "Port": 8090
      }
    ]
  }
}
```

**`data/secrets.json`** - Encrypted secrets (AES-256-CBC):
```json
[
  {
    "SecretName": "SpotifyAccountPassword",
    "SecretValue": "<encrypted-base64>"
  }
]
```

## 🧪 Testing

**Backend:**
```bash
cd services
dotnet test
```

**Frontend:**
```bash
cd frontend
npx nx test
```

### Testing with Real SoundTouch Devices

For integration testing against real Bose SoundTouch devices:

1. **Ensure device is on the network** - The device should be reachable via its IP address on port 8090.

2. **Add the device** via API:
   ```bash
   curl -X POST http://localhost:5000/api/devices \
     -H "Content-Type: application/json" \
     -d '{"name": "Living Room", "ipAddress": "192.168.1.100", "vendor": "bose-soundtouch", "port": 8090}'
   ```

3. **Test device endpoints**:
   ```bash
   # Get device info
   curl http://localhost:5000/api/devices/{id}/info

   # Get now playing
   curl http://localhost:5000/api/devices/{id}/nowPlaying

   # Get volume
   curl http://localhost:5000/api/devices/{id}/volume

   # Set volume to 30%
   curl -X POST http://localhost:5000/api/devices/{id}/volume \
     -H "Content-Type: application/json" \
     -d '{"level": 30}'

   # Play preset 1
   curl -X POST http://localhost:5000/api/devices/{id}/presets/1/play
   ```

4. **Device discovery** - Scan your local network for SoundTouch devices:
   ```bash
   curl http://localhost:5000/api/devices/discover?vendor=bose-soundtouch
   ```

**Note:** Integration tests require a real SoundTouch device on the network. Unit tests use mocked HTTP responses and don't require hardware.

**Run all tests in CI:**
```bash
# See .github/workflows/ci.yml
```

## 📖 API Documentation

Once the API is running, access interactive documentation:
- Swagger UI: http://localhost:5000/swagger
- OpenAPI JSON: http://localhost:5000/swagger/v1/swagger.json

### Key Endpoints

#### Device Management
- `GET /api/devices` - List all devices
- `POST /api/devices` - Add a device
- `GET /api/devices/{id}` - Get device by ID
- `DELETE /api/devices/{id}` - Remove a device
- `GET /api/devices/discover` - Discover devices on LAN

#### Device Status & Info
- `GET /api/devices/{id}/status` - Get device status (power, volume, source)
- `GET /api/devices/{id}/info` - Get detailed device info (name, type, MAC, software version)
- `GET /api/devices/{id}/nowPlaying` - Get current playback info (track, artist, album, source)

#### Device Control
- `POST /api/devices/{id}/power` - Set power state (`{ "on": true|false }`)
- `GET /api/devices/{id}/volume` - Get volume info (target, actual, mute state)
- `POST /api/devices/{id}/volume` - Set volume (`{ "level": 0-100 }`)
- `POST /api/devices/{id}/bluetooth/pairing` - Enter Bluetooth pairing mode

#### Presets
- `GET /api/devices/{id}/presets` - List device presets (1-6)
- `POST /api/devices/{id}/presets/{presetNumber}/play` - Play a preset (1-6)

#### Health
- `GET /health` - Health check

## 🔒 Security

- **Secrets Encryption**: AES-256-CBC with PBKDF2 key derivation
- **Master Password**: Retrieved from Docker secret file (`/run/secrets/master_password`) with fallback to environment variable
- **Key Storage**: SQLite-based NSS-style key4.db for encrypted key storage
- **CORS**: Configured for frontend origin only
- **HTTPS**: Enabled in production (configure certificates in appsettings)

## 🤝 Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for development guidelines, commit conventions, and testing practices.

## 📄 License

[Your License Here]

## 🙏 Acknowledgments

- Bose SoundTouch API documentation
- Nx for monorepo tooling
- .NET community for clean architecture patterns

---

**Questions or issues?** Open an issue on GitHub!
