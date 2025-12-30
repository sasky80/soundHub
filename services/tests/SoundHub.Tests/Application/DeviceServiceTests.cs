using SoundHub.Application.Services;
using SoundHub.Domain.Entities;
using SoundHub.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace SoundHub.Tests.Application;

/// <summary>
/// Sample tests for DeviceService demonstrating testing patterns.
/// </summary>
public class DeviceServiceTests
{
    private readonly Mock<IDeviceRepository> _mockRepository;
    private readonly Mock<DeviceAdapterRegistry> _mockRegistry;
    private readonly Mock<ILogger<DeviceService>> _mockLogger;
    private readonly DeviceService _service;

    public DeviceServiceTests()
    {
        _mockRepository = new Mock<IDeviceRepository>();
        _mockRegistry = new Mock<DeviceAdapterRegistry>();
        _mockLogger = new Mock<ILogger<DeviceService>>();
        _service = new DeviceService(_mockRepository.Object, _mockRegistry.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllDevicesAsync_ReturnsAllDevices()
    {
        // Arrange
        var expectedDevices = new List<Device>
        {
            new Device { Id = "1", Vendor = "bose-soundtouch", Name = "Speaker 1", IpAddress = "192.168.1.10" },
            new Device { Id = "2", Vendor = "bose-soundtouch", Name = "Speaker 2", IpAddress = "192.168.1.11" }
        };
        _mockRepository.Setup(r => r.GetAllDevicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDevices);

        // Act
        var result = await _service.GetAllDevicesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Speaker 1", result[0].Name);
    }

    [Fact]
    public async Task GetDeviceAsync_WhenDeviceExists_ReturnsDevice()
    {
        // Arrange
        var expectedDevice = new Device
        {
            Id = "123",
            Vendor = "bose-soundtouch",
            Name = "Test Speaker",
            IpAddress = "192.168.1.100"
        };
        _mockRepository.Setup(r => r.GetDeviceAsync("123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDevice);

        // Act
        var result = await _service.GetDeviceAsync("123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("123", result.Id);
        Assert.Equal("Test Speaker", result.Name);
    }

    [Fact]
    public async Task GetDeviceAsync_WhenDeviceDoesNotExist_ReturnsNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetDeviceAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Device?)null);

        // Act
        var result = await _service.GetDeviceAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddDeviceAsync_CreatesNewDevice()
    {
        // Arrange
        var newDevice = new Device
        {
            Id = "new-id",
            Vendor = "bose-soundtouch",
            Name = "New Speaker",
            IpAddress = "192.168.1.50",
            Port = 8090
        };
        _mockRepository.Setup(r => r.AddDeviceAsync(It.IsAny<Device>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Device d, CancellationToken ct) => d);

        // Act
        var result = await _service.AddDeviceAsync("New Speaker", "192.168.1.50", "bose-soundtouch", 8090);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Speaker", result.Name);
        Assert.Equal("192.168.1.50", result.IpAddress);
        _mockRepository.Verify(r => r.AddDeviceAsync(It.IsAny<Device>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveDeviceAsync_WhenDeviceExists_ReturnsTrue()
    {
        // Arrange
        _mockRepository.Setup(r => r.RemoveDeviceAsync("123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.RemoveDeviceAsync("123");

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.RemoveDeviceAsync("123", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveDeviceAsync_WhenDeviceDoesNotExist_ReturnsFalse()
    {
        // Arrange
        _mockRepository.Setup(r => r.RemoveDeviceAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.RemoveDeviceAsync("nonexistent");

        // Assert
        Assert.False(result);
    }
}
