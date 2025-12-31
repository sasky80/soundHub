using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using SoundHub.Domain.Entities;
using SoundHub.Infrastructure.Persistence;

namespace SoundHub.Tests.Infrastructure;

/// <summary>
/// Unit tests for FileDeviceRepository CRUD operations.
/// </summary>
public class FileDeviceRepositoryTests : IDisposable
{
    private readonly string _tempFilePath;
    private readonly FileDeviceRepository _repository;

    public FileDeviceRepositoryTests()
    {
        _tempFilePath = Path.Combine(Path.GetTempPath(), $"test-devices-{Guid.NewGuid()}.json");
        var options = Options.Create(new FileDeviceRepositoryOptions
        {
            FilePath = _tempFilePath,
            EnableHotReload = false
        });
        var logger = Substitute.For<ILogger<FileDeviceRepository>>();
        _repository = new FileDeviceRepository(options, logger);
    }

    public void Dispose()
    {
        if (File.Exists(_tempFilePath))
        {
            File.Delete(_tempFilePath);
        }
    }

    #region Add Device Tests

    [Fact]
    public async Task AddDeviceAsync_NewDevice_PersistsToFile()
    {
        // Arrange
        var device = CreateTestDevice("test-1", "Speaker 1", "192.168.1.10");

        // Act
        var result = await _repository.AddDeviceAsync(device);

        // Assert
        Assert.Equal(device.Id, result.Id);
        Assert.Equal(device.Name, result.Name);
        Assert.Equal(device.IpAddress, result.IpAddress);

        // Verify file content
        var json = await File.ReadAllTextAsync(_tempFilePath);
        Assert.Contains("Speaker 1", json);
        Assert.Contains("192.168.1.10", json);
    }

    [Fact]
    public async Task AddDeviceAsync_MultipleDevices_AllPersisted()
    {
        // Arrange & Act
        var device1 = CreateTestDevice("test-1", "Speaker 1", "192.168.1.10");
        var device2 = CreateTestDevice("test-2", "Speaker 2", "192.168.1.11");
        var device3 = CreateTestDevice("test-3", "Speaker 3", "192.168.1.12");

        await _repository.AddDeviceAsync(device1);
        await _repository.AddDeviceAsync(device2);
        await _repository.AddDeviceAsync(device3);

        // Assert
        var devices = await _repository.GetAllDevicesAsync();
        Assert.Equal(3, devices.Count);
    }

    #endregion

    #region Get Device Tests

    [Fact]
    public async Task GetDeviceAsync_ExistingDevice_ReturnsDevice()
    {
        // Arrange
        var device = CreateTestDevice("test-1", "Speaker 1", "192.168.1.10");
        await _repository.AddDeviceAsync(device);

        // Act
        var result = await _repository.GetDeviceAsync("test-1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-1", result.Id);
        Assert.Equal("Speaker 1", result.Name);
    }

    [Fact]
    public async Task GetDeviceAsync_NonExistentDevice_ReturnsNull()
    {
        // Act
        var result = await _repository.GetDeviceAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllDevicesAsync_EmptyFile_ReturnsEmptyList()
    {
        // Act
        var result = await _repository.GetAllDevicesAsync();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region Update Device Tests

    [Fact]
    public async Task UpdateDeviceAsync_ExistingDevice_UpdatesFields()
    {
        // Arrange
        var device = CreateTestDevice("test-1", "Speaker 1", "192.168.1.10");
        await _repository.AddDeviceAsync(device);

        // Act
        device.Name = "Updated Speaker";
        device.IpAddress = "192.168.1.100";
        device.Capabilities = new HashSet<string> { "power", "volume", "presets" };
        var result = await _repository.UpdateDeviceAsync(device);

        // Assert
        Assert.Equal("Updated Speaker", result.Name);
        Assert.Equal("192.168.1.100", result.IpAddress);
        Assert.Contains("presets", result.Capabilities);

        // Verify persistence
        var retrieved = await _repository.GetDeviceAsync("test-1");
        Assert.NotNull(retrieved);
        Assert.Equal("Updated Speaker", retrieved.Name);
    }

    [Fact]
    public async Task UpdateDeviceAsync_NonExistentDevice_ThrowsKeyNotFoundException()
    {
        // Arrange
        var device = CreateTestDevice("nonexistent", "Speaker", "192.168.1.10");

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _repository.UpdateDeviceAsync(device));
    }

    #endregion

    #region Remove Device Tests

    [Fact]
    public async Task RemoveDeviceAsync_ExistingDevice_ReturnsTrue()
    {
        // Arrange
        var device = CreateTestDevice("test-1", "Speaker 1", "192.168.1.10");
        await _repository.AddDeviceAsync(device);

        // Act
        var result = await _repository.RemoveDeviceAsync("test-1");

        // Assert
        Assert.True(result);

        // Verify device no longer exists
        var retrieved = await _repository.GetDeviceAsync("test-1");
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task RemoveDeviceAsync_NonExistentDevice_ReturnsFalse()
    {
        // Act
        var result = await _repository.RemoveDeviceAsync("nonexistent");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Network Mask Tests

    [Fact]
    public async Task GetNetworkMaskAsync_WhenNotSet_ReturnsNull()
    {
        // Act
        var result = await _repository.GetNetworkMaskAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetNetworkMaskAsync_StoresValue()
    {
        // Act
        await _repository.SetNetworkMaskAsync("192.168.1.0/24");

        // Assert
        var result = await _repository.GetNetworkMaskAsync();
        Assert.Equal("192.168.1.0/24", result);
    }

    [Fact]
    public async Task SetNetworkMaskAsync_OverwritesPreviousValue()
    {
        // Arrange
        await _repository.SetNetworkMaskAsync("10.0.0.0/8");

        // Act
        await _repository.SetNetworkMaskAsync("192.168.1.0/24");

        // Assert
        var result = await _repository.GetNetworkMaskAsync();
        Assert.Equal("192.168.1.0/24", result);
    }

    [Fact]
    public async Task SetNetworkMaskAsync_PersistsWithDevices()
    {
        // Arrange
        var device = CreateTestDevice("test-1", "Speaker 1", "192.168.1.10");
        await _repository.AddDeviceAsync(device);

        // Act
        await _repository.SetNetworkMaskAsync("192.168.1.0/24");

        // Assert - both device and network mask should be persisted
        var json = await File.ReadAllTextAsync(_tempFilePath);
        Assert.Contains("NetworkMask", json);
        Assert.Contains("192.168.1.0/24", json);
        Assert.Contains("Speaker 1", json);
    }

    #endregion

    #region GetDevicesByVendorAsync Tests

    [Fact]
    public async Task GetDevicesByVendorAsync_ReturnsMatchingDevices()
    {
        // Arrange
        var device1 = CreateTestDevice("test-1", "Speaker 1", "192.168.1.10");
        var device2 = CreateTestDevice("test-2", "Speaker 2", "192.168.1.11");
        await _repository.AddDeviceAsync(device1);
        await _repository.AddDeviceAsync(device2);

        // Act
        var result = await _repository.GetDevicesByVendorAsync("bose-soundtouch");

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetDevicesByVendorAsync_NoMatch_ReturnsEmpty()
    {
        // Act
        var result = await _repository.GetDevicesByVendorAsync("unknown-vendor");

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region DateTimeAdded Tests

    [Fact]
    public async Task AddDeviceAsync_PreservesDateTimeAdded()
    {
        // Arrange
        var addedTime = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var device = new Device
        {
            Id = "test-1",
            Vendor = "bose-soundtouch",
            Name = "Speaker 1",
            IpAddress = "192.168.1.10",
            DateTimeAdded = addedTime
        };

        // Act
        await _repository.AddDeviceAsync(device);
        var result = await _repository.GetDeviceAsync("test-1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(addedTime, result.DateTimeAdded);
    }

    #endregion

    private static Device CreateTestDevice(string id, string name, string ipAddress)
    {
        return new Device
        {
            Id = id,
            Vendor = "bose-soundtouch",
            Name = name,
            IpAddress = ipAddress,
            Capabilities = new HashSet<string> { "power", "volume" },
            DateTimeAdded = DateTime.UtcNow
        };
    }
}
