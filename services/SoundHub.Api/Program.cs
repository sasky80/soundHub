using SoundHub.Application.Services;
using SoundHub.Domain.Interfaces;
using SoundHub.Infrastructure.Adapters;
using SoundHub.Infrastructure.Persistence;
using SoundHub.Infrastructure.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configure logging (JSON format for structured logs)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddJsonConsole(options =>
{
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configure CORS for frontend and future clients
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200", // Angular dev server
                "http://localhost:80",   // Docker web container
                "http://localhost"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

// Configure options from environment variables or appsettings.json
builder.Services.Configure<FileDeviceRepositoryOptions>(options =>
{
    options.FilePath = builder.Configuration.GetValue<string>("DevicesFilePath") ?? "/data/devices.json";
    options.EnableHotReload = builder.Configuration.GetValue<bool?>("EnableDeviceHotReload") ?? true;
});

builder.Services.Configure<SecretsServiceOptions>(options =>
{
    options.SecretsFilePath = builder.Configuration.GetValue<string>("SecretsFilePath") ?? "/data/secrets.json";
});

builder.Services.Configure<EncryptionKeyStoreOptions>(options =>
{
    options.KeyDbPath = builder.Configuration.GetValue<string>("KeyDbPath") ?? "/data/key4.db";
    options.MasterPasswordFile = builder.Configuration.GetValue<string>("MasterPasswordFile");
    options.MasterPassword = builder.Configuration.GetValue<string>("MasterPassword") ?? "default-dev-password";
});

// Register application services
builder.Services.AddSingleton<EncryptionKeyStore>();
builder.Services.AddSingleton<IDeviceRepository, FileDeviceRepository>();
builder.Services.AddSingleton<ISecretsService, EncryptedSecretsService>();
builder.Services.AddScoped<DeviceService>();

// Register file watcher for hot-reload of devices.json
builder.Services.AddSingleton<DeviceFileWatcher>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DeviceFileWatcher>());

// Register device adapters
builder.Services.AddHttpClient("SoundTouch", client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
});
builder.Services.AddSingleton<IDeviceAdapter, SoundTouchAdapter>();

// Register the adapter registry and populate it with all adapters
builder.Services.AddSingleton<DeviceAdapterRegistry>(sp =>
{
    var registry = new DeviceAdapterRegistry();
    var adapters = sp.GetServices<IDeviceAdapter>();
    foreach (var adapter in adapters)
    {
        registry.RegisterAdapter(adapter);
    }
    return registry;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "SoundHub API";
        options.Theme = ScalarTheme.BluePlanet;
    });
}

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Map health check endpoint
app.MapHealthChecks("/health");

app.Logger.LogInformation("SoundHub API starting on {Environment}", app.Environment.EnvironmentName);

app.Run();
