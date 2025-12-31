# SoundHub Architecture

This document provides an overview of the SoundHub architecture, including the monorepo structure, API layers, and device adapter pattern.

## High-Level Architecture

```mermaid
flowchart TB
    subgraph Clients["Clients"]
        Web["Angular Web App<br/>(frontend/)"]
        Mobile["Mobile App<br/>(future)"]
        MCP["MCP Server<br/>(future)"]
    end

    subgraph API["SoundHub API (.NET 8)"]
        Controllers["Controllers<br/>(Presentation)"]
        Application["Application Layer<br/>(Services)"]
        Domain["Domain Layer<br/>(Entities, Interfaces)"]
        Infrastructure["Infrastructure Layer<br/>(Adapters, Persistence)"]
    end

    subgraph Devices["Physical Devices"]
        SoundTouch["Bose SoundTouch"]
        Sonos["Sonos<br/>(future)"]
        Other["Other Vendors<br/>(future)"]
    end

    subgraph Storage["Data Storage"]
        DevicesJson["devices.json<br/>(configuration)"]
        SecretsJson["secrets.json<br/>(encrypted)"]
        Key4DB["key4.db<br/>(encryption keys)"]
    end

    Web --> Controllers
    Mobile --> Controllers
    MCP --> Controllers
    Controllers --> Application
    Application --> Domain
    Application --> Infrastructure
    Infrastructure --> Domain
    Infrastructure --> Devices
    Infrastructure --> Storage
```

## Monorepo Structure

```mermaid
flowchart TB
    subgraph Root["soundHub/"]
        subgraph Frontend["frontend/ (Nx Workspace)"]
            AppsWeb["src/<br/>Angular App"]
            LibsFeature["libs/frontend/feature/<br/>Smart Components"]
            LibsDataAccess["libs/frontend/data-access/<br/>API Services"]
            LibsUI["libs/frontend/ui/<br/>Dumb Components"]
            LibsShared["libs/frontend/shared/<br/>Utilities"]
            E2E["e2e/<br/>Playwright Tests"]
        end

        subgraph Services["services/ (.NET Solution)"]
            SoundHubApi["SoundHub.Api<br/>Controllers, Startup"]
            SoundHubApp["SoundHub.Application<br/>Business Logic"]
            SoundHubDomain["SoundHub.Domain<br/>Entities, Interfaces"]
            SoundHubInfra["SoundHub.Infrastructure<br/>Adapters, Persistence"]
            Tests["tests/<br/>xUnit Tests"]
        end

        Data["data/<br/>devices.json"]
        Docs["docs/<br/>Documentation"]
        OpenSpec["openspec/<br/>Change Proposals"]
    end

    AppsWeb --> LibsFeature
    AppsWeb --> LibsDataAccess
    LibsFeature --> LibsUI
    LibsFeature --> LibsDataAccess
    LibsDataAccess --> LibsShared
    LibsUI --> LibsShared

    SoundHubApi --> SoundHubApp
    SoundHubApp --> SoundHubDomain
    SoundHubApp --> SoundHubInfra
    SoundHubInfra --> SoundHubDomain
```

## .NET API Layers

```mermaid
flowchart TB
    subgraph Presentation["Presentation Layer (SoundHub.Api)"]
        DevicesController["DevicesController"]
        HealthController["HealthController"]
        Middleware["CORS, Error Handling"]
    end

    subgraph AppLayer["Application Layer (SoundHub.Application)"]
        DeviceService["DeviceService"]
        AdapterRegistry["DeviceAdapterRegistry"]
    end

    subgraph DomainLayer["Domain Layer (SoundHub.Domain)"]
        subgraph Entities["Entities"]
            Device["Device"]
            DeviceStatus["DeviceStatus"]
            Preset["Preset"]
        end
        subgraph Interfaces["Interfaces"]
            IDeviceAdapter["IDeviceAdapter"]
            IDeviceRepository["IDeviceRepository"]
            ISecretsService["ISecretsService"]
        end
    end

    subgraph InfraLayer["Infrastructure Layer (SoundHub.Infrastructure)"]
        subgraph Adapters["Adapters"]
            SoundTouchAdapter["SoundTouchAdapter"]
            FutureAdapter["Future Adapters..."]
        end
        subgraph Persistence["Persistence"]
            JsonDeviceRepo["JsonDeviceRepository"]
        end
        subgraph ServicesInfra["Services"]
            FileSecretsService["FileSecretsService"]
            EncryptionKeyStore["EncryptionKeyStore"]
        end
    end

    DevicesController --> DeviceService
    HealthController --> DeviceService
    DeviceService --> AdapterRegistry
    DeviceService --> IDeviceRepository
    AdapterRegistry --> IDeviceAdapter
    SoundTouchAdapter -.implements.-> IDeviceAdapter
    JsonDeviceRepo -.implements.-> IDeviceRepository
    FileSecretsService -.implements.-> ISecretsService
```

## Device Adapter Pattern

```mermaid
classDiagram
    class IDeviceAdapter {
        <<interface>>
        +VendorId: string
        +GetCapabilitiesAsync(deviceId, ct): Task~IReadOnlySet~string~~
        +GetStatusAsync(deviceId, ct): Task~DeviceStatus~
        +SetPowerAsync(deviceId, on, ct): Task
        +SetVolumeAsync(deviceId, level, ct): Task
        +EnterPairingModeAsync(deviceId, ct): Task
        +GetPresetsAsync(deviceId, ct): Task~IReadOnlyList~Preset~~
        +PlayPresetAsync(deviceId, presetId, ct): Task
        +DiscoverDevicesAsync(ct): Task~IReadOnlyList~Device~~
    }

    class SoundTouchAdapter {
        +VendorId: "bose-soundtouch"
        +GetCapabilitiesAsync()
        +GetStatusAsync()
        +SetPowerAsync()
        +SetVolumeAsync()
        +EnterPairingModeAsync()
        +GetPresetsAsync()
        +PlayPresetAsync()
        +DiscoverDevicesAsync()
    }

    class FutureVendorAdapter {
        +VendorId: "vendor-name"
        ...
    }

    class DeviceAdapterRegistry {
        -_adapters: Dictionary~string, IDeviceAdapter~
        +RegisterAdapter(adapter): void
        +GetAdapter(vendorId): IDeviceAdapter?
        +GetAllAdapters(): IEnumerable~IDeviceAdapter~
    }

    IDeviceAdapter <|.. SoundTouchAdapter : implements
    IDeviceAdapter <|.. FutureVendorAdapter : implements
    DeviceAdapterRegistry o-- IDeviceAdapter : manages
```

## Docker Deployment

```mermaid
flowchart TB
    subgraph DockerCompose["docker-compose.yml"]
        subgraph APIContainer["soundhub-api"]
            DotNetAPI[".NET 8 Web API<br/>Port 5000"]
        end

        subgraph WebContainer["soundhub-web"]
            NginxWeb["Nginx + Angular<br/>Port 80"]
        end

        subgraph Volumes["Volumes"]
            DataVolume["/data<br/>devices.json<br/>secrets.json<br/>key4.db"]
            SecretsVolume["/run/secrets<br/>master_password"]
        end
    end

    Browser["Browser"] --> NginxWeb
    NginxWeb --> DotNetAPI
    DotNetAPI --> DataVolume
    DotNetAPI --> SecretsVolume
```

## Data Flow: Device Control

```mermaid
sequenceDiagram
    participant User
    participant WebApp as Angular Web App
    participant API as SoundHub API
    participant Registry as DeviceAdapterRegistry
    participant Adapter as SoundTouchAdapter
    participant Device as Bose SoundTouch

    User->>WebApp: Click "Volume Up"
    WebApp->>API: POST /devices/{id}/volume {level: 50}
    API->>Registry: GetAdapter("bose-soundtouch")
    Registry-->>API: SoundTouchAdapter
    API->>Adapter: SetVolumeAsync(deviceId, 50)
    Adapter->>Device: HTTP/SOAP Request
    Device-->>Adapter: OK
    Adapter-->>API: Success
    API-->>WebApp: 200 OK
    WebApp-->>User: Volume updated
```

## Data Flow: Power Toggle

```mermaid
sequenceDiagram
    participant User
    participant WebApp as Angular Web App
    participant API as SoundHub API
    participant Service as DeviceService
    participant Registry as DeviceAdapterRegistry
    participant Adapter as IDeviceAdapter
    participant Device as Physical Device

    User->>WebApp: Toggle Power On
    WebApp->>API: POST /api/devices/{id}/power {"on": true}
    API->>Service: SetPowerAsync(id, true)
    Service->>Registry: GetAdapter(device.Vendor)
    Registry-->>Service: VendorAdapter
    Service->>Adapter: SetPowerAsync(deviceId, true)
    Adapter->>Device: Vendor-specific command
    Device-->>Adapter: OK
    Adapter-->>Service: Success
    Service-->>API: Success
    API-->>WebApp: 204 No Content
    WebApp-->>User: Power state updated
```

## Frontend Library Architecture

```mermaid
flowchart TB
    subgraph App["Angular Application (src/)"]
        AppComponent["AppComponent"]
        Routes["app.routes.ts"]
    end

    subgraph Feature["libs/frontend/feature/"]
        Landing["LandingComponent"]
        Settings["SettingsComponent"]
        DeviceConfig["DeviceConfigComponent"]
        DeviceDetails["DeviceDetailsComponent"]
    end

    subgraph DataAccess["libs/frontend/data-access/"]
        DeviceService["DeviceService"]
    end

    subgraph Shared["libs/frontend/shared/"]
        LanguageService["LanguageService"]
        TranslatePipe["TranslatePipe"]
    end

    subgraph UI["libs/frontend/ui/"]
        VolumeSlider["Volume Slider"]
        DeviceCard["Device Card"]
        PresetButton["Preset Button"]
    end

    AppComponent --> Routes
    Routes --> Feature
    Landing --> DeviceService
    Landing --> TranslatePipe
    Settings --> LanguageService
    Settings --> TranslatePipe
    DeviceConfig --> DeviceService
    DeviceConfig --> TranslatePipe
    DeviceDetails --> DeviceService
    DeviceDetails --> TranslatePipe
    Feature --> Shared
    UI --> Shared
```

## Web UI Routes

| Route | Component | Description |
|-------|-----------|-------------|
| `/` | LandingComponent | Displays list of configured devices with navigation to settings and device details |
| `/settings` | SettingsComponent | Language selection (English/Polish) and navigation to device configuration |
| `/settings/devices` | DeviceConfigComponent | Lists configured devices with navigation to device details |
| `/devices/:id` | DeviceDetailsComponent | Device control page with power on/off toggle |

## Internationalization (i18n)

The frontend supports runtime language switching between English and Polish:

- **LanguageService**: Signal-based service that manages current language and translations
- **TranslatePipe**: Pipe for translating keys in templates
- **Persistence**: Selected language is stored in `localStorage` under `soundhub-language`

## Key Design Principles

1. **Separation of Concerns**: Clear boundaries between layers (Domain, Application, Infrastructure, Presentation)
2. **Dependency Inversion**: Core layers depend on abstractions, not implementations
3. **Device Adapter Pattern**: Vendor-specific logic is encapsulated in adapters implementing `IDeviceAdapter`
4. **Per-Device Capability Discovery**: Each device reports its capabilities, enabling heterogeneous device support
5. **API-First Design**: OpenAPI spec drives development; code implements the contract
6. **Container-First**: Docker Compose provides consistent dev/prod environments
