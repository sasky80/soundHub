using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SoundHub.Api.Controllers;
using SoundHub.Application.Services;
using SoundHub.Domain.Entities;
using SoundHub.Domain.Interfaces;

namespace SoundHub.Tests.Api;

/// <summary>
/// Unit tests for DevicesController ping and discovery endpoints.
/// </summary>
public class DevicesControllerPingDiscoveryTests
{
    private readonly DeviceService _deviceService;
    private readonly IDeviceRepository _repository;
    private readonly DeviceAdapterRegistry _adapterRegistry;
    private readonly IDeviceAdapter _mockAdapter;
    private readonly DevicesController _controller;

    public DevicesControllerPingDiscoveryTests()
    {
        _repository = Substitute.For<IDeviceRepository>();
        _adapterRegistry = new DeviceAdapterRegistry();
        _mockAdapter = Substitute.For<IDeviceAdapter>();
        _mockAdapter.VendorId.Returns("bose-soundtouch");
        _mockAdapter.VendorName.Returns("Bose SoundTouch");
        _adapterRegistry.RegisterAdapter(_mockAdapter);

        var serviceLogger = Substitute.For<ILogger<DeviceService>>();
        _deviceService = new DeviceService(_repository, _adapterRegistry, serviceLogger);

        var controllerLogger = Substitute.For<ILogger<DevicesController>>();
        _controller = new DevicesController(_deviceService, controllerLogger);
    }

    #region Ping Endpoint Tests

    [Fact]
    public async Task PingDevice_DeviceExists_ReturnsOkWithPingResult()
    {
        // Arrange
        var device = CreateTestDevice("test-1");
        _repository.GetDeviceAsync("test-1", Arg.Any<CancellationToken>()).Returns(device);
        _mockAdapter.PingAsync("test-1", Arg.Any<CancellationToken>())
            .Returns(new PingResult(true, 42));

        // Act
        var result = await _controller.PingDevice("test-1", CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var pingResult = Assert.IsType<PingResult>(okResult.Value);
        Assert.True(pingResult.Reachable);
        Assert.Equal(42, pingResult.LatencyMs);
    }

    [Fact]
    public async Task PingDevice_DeviceNotFound_ReturnsNotFound()
    {
        // Arrange
        _repository.GetDeviceAsync("nonexistent", Arg.Any<CancellationToken>())
            .Returns((Device?)null);

        // Act
        var result = await _controller.PingDevice("nonexistent", CancellationToken.None);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundResult.Value);
    }

    [Fact]
    public async Task PingDevice_NoAdapterForVendor_ReturnsNotImplemented()
    {
        // Arrange
        var device = CreateTestDevice("test-1");
        device = new Device
        {
            Id = "test-1",
            Vendor = "unknown-vendor",
            Name = "Unknown Device",
            IpAddress = "192.168.1.10"
        };
        _repository.GetDeviceAsync("test-1", Arg.Any<CancellationToken>()).Returns(device);

        // Act
        var result = await _controller.PingDevice("test-1", CancellationToken.None);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(501, statusResult.StatusCode);
    }

    [Fact]
    public async Task PingDevice_DeviceUnreachable_ReturnsOkWithUnreachable()
    {
        // Arrange
        var device = CreateTestDevice("test-1");
        _repository.GetDeviceAsync("test-1", Arg.Any<CancellationToken>()).Returns(device);
        _mockAdapter.PingAsync("test-1", Arg.Any<CancellationToken>())
            .Returns(new PingResult(false, 5001));

        // Act
        var result = await _controller.PingDevice("test-1", CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var pingResult = Assert.IsType<PingResult>(okResult.Value);
        Assert.False(pingResult.Reachable);
    }

    #endregion

    #region Discovery Endpoint Tests

    [Fact]
    public async Task DiscoverDevices_NoDevicesFound_ReturnsEmptyResult()
    {
        // Arrange
        _repository.GetNetworkMaskAsync(Arg.Any<CancellationToken>())
            .Returns("192.168.1.0/24");
        _repository.GetAllDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Device>());
        _mockAdapter.DiscoverDevicesAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new List<Device>());

        // Act
        var result = await _controller.DiscoverDevices(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var discoveryResult = Assert.IsType<DiscoveryResult>(okResult.Value);
        Assert.Equal(0, discoveryResult.Discovered);
        Assert.Equal(0, discoveryResult.New);
    }

    [Fact]
    public async Task DiscoverDevices_NewDeviceFound_SavesAndReturnsDevice()
    {
        // Arrange
        var discoveredDevice = CreateTestDevice("discovered-1", "Discovered Speaker", "192.168.1.50");

        _repository.GetNetworkMaskAsync(Arg.Any<CancellationToken>())
            .Returns("192.168.1.0/24");
        _repository.GetAllDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Device>());
        _mockAdapter.DiscoverDevicesAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new List<Device> { discoveredDevice });
        _repository.AddDeviceAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Device>());

        // Act
        var result = await _controller.DiscoverDevices(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var discoveryResult = Assert.IsType<DiscoveryResult>(okResult.Value);
        Assert.Equal(1, discoveryResult.Discovered);
        Assert.Equal(1, discoveryResult.New);
        Assert.Single(discoveryResult.Devices);
    }

    [Fact]
    public async Task DiscoverDevices_DeviceAlreadyExists_SkipsExisting()
    {
        // Arrange
        var existingDevice = CreateTestDevice("existing-1", "Existing Speaker", "192.168.1.10");
        var discoveredDevice = CreateTestDevice("discovered-1", "Same Device", "192.168.1.10"); // Same IP

        _repository.GetNetworkMaskAsync(Arg.Any<CancellationToken>())
            .Returns("192.168.1.0/24");
        _repository.GetAllDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Device> { existingDevice });
        _mockAdapter.DiscoverDevicesAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new List<Device> { discoveredDevice });

        // Act
        var result = await _controller.DiscoverDevices(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var discoveryResult = Assert.IsType<DiscoveryResult>(okResult.Value);
        Assert.Equal(1, discoveryResult.Discovered);
        Assert.Equal(0, discoveryResult.New); // Not saved because already exists

        // Verify AddDeviceAsync was not called
        await _repository.DidNotReceive().AddDeviceAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
    }

    #endregion

    private static Device CreateTestDevice(string id, string name = "Test Device", string ipAddress = "192.168.1.10")
    {
        return new Device
        {
            Id = id,
            Vendor = "bose-soundtouch",
            Name = name,
            IpAddress = ipAddress,
            Capabilities = new HashSet<string> { "power", "volume", "ping" },
            DateTimeAdded = DateTime.UtcNow
        };
    }
}
