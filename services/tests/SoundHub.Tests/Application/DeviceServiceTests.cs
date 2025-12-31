using SoundHub.Application.Services;
using SoundHub.Domain.Entities;
using SoundHub.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace SoundHub.Tests.Application;

/// <summary>
/// Sample tests for DeviceService demonstrating testing patterns.
/// </summary>
public class DeviceServiceTests
{
    private readonly IDeviceRepository _repository;
    private readonly DeviceAdapterRegistry _registry;
    private readonly ILogger<DeviceService> _logger;
    private readonly DeviceService _service;

    public DeviceServiceTests()
    {
        _repository = Substitute.For<IDeviceRepository>();
        _registry = new DeviceAdapterRegistry();
        _logger = Substitute.For<ILogger<DeviceService>>();
        _service = new DeviceService(_repository, _registry, _logger);
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
        _repository.GetAllDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(expectedDevices);

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
        _repository.GetDeviceAsync("123", Arg.Any<CancellationToken>())
            .Returns(expectedDevice);

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
        _repository.GetDeviceAsync("nonexistent", Arg.Any<CancellationToken>())
            .Returns((Device?)null);

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
            IpAddress = "192.168.1.50"
        };
        _repository.AddDeviceAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Device>());

        // Act
        var result = await _service.AddDeviceAsync("New Speaker", "192.168.1.50", "bose-soundtouch");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Speaker", result.Name);
        Assert.Equal("192.168.1.50", result.IpAddress);
        await _repository.Received(1).AddDeviceAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveDeviceAsync_WhenDeviceExists_ReturnsTrue()
    {
        // Arrange
        _repository.RemoveDeviceAsync("123", Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _service.RemoveDeviceAsync("123");

        // Assert
        Assert.True(result);
        await _repository.Received(1).RemoveDeviceAsync("123", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveDeviceAsync_WhenDeviceDoesNotExist_ReturnsFalse()
    {
        // Arrange
        _repository.RemoveDeviceAsync("nonexistent", Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _service.RemoveDeviceAsync("nonexistent");

        // Assert
        Assert.False(result);
    }
}
