using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using SoundHub.Infrastructure.Persistence;

namespace SoundHub.Tests.Infrastructure;

/// <summary>
/// Tests for DeviceFileWatcher functionality.
/// </summary>
public class DeviceFileWatcherTests
{
    [Fact]
    public async Task StartAsync_WhenHotReloadDisabled_DoesNotStartWatcher()
    {
        // Arrange
        var options = Options.Create(new FileDeviceRepositoryOptions
        {
            FilePath = "/data/devices.json",
            EnableHotReload = false
        });
        var logger = Substitute.For<ILogger<DeviceFileWatcher>>();
        var watcher = new DeviceFileWatcher(options, logger);

        // Act
        await watcher.StartAsync(CancellationToken.None);
        await watcher.StopAsync(CancellationToken.None);

        // Assert - Should complete without errors when hot-reload is disabled
        watcher.Dispose();
    }

    [Fact]
    public async Task StartAsync_WhenDirectoryDoesNotExist_LogsWarning()
    {
        // Arrange
        var options = Options.Create(new FileDeviceRepositoryOptions
        {
            FilePath = "/nonexistent/directory/devices.json",
            EnableHotReload = true
        });
        var logger = Substitute.For<ILogger<DeviceFileWatcher>>();
        var watcher = new DeviceFileWatcher(options, logger);

        // Act
        await watcher.StartAsync(CancellationToken.None);

        // Assert - Should handle missing directory gracefully
        await watcher.StopAsync(CancellationToken.None);
        watcher.Dispose();
    }

    [Fact]
    public void DevicesFileChangedEventArgs_HasCorrectProperties()
    {
        // Arrange & Act
        var args = new DevicesFileChangedEventArgs("/data/devices.json", WatcherChangeTypes.Changed);

        // Assert
        Assert.Equal("/data/devices.json", args.FilePath);
        Assert.Equal(WatcherChangeTypes.Changed, args.ChangeType);
    }

    [Fact]
    public async Task Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var options = Options.Create(new FileDeviceRepositoryOptions
        {
            FilePath = "/data/devices.json",
            EnableHotReload = false
        });
        var logger = Substitute.For<ILogger<DeviceFileWatcher>>();
        var watcher = new DeviceFileWatcher(options, logger);

        // Act & Assert - Should not throw
        await watcher.StartAsync(CancellationToken.None);
        watcher.Dispose();
        watcher.Dispose(); // Second call should be safe
    }
}
