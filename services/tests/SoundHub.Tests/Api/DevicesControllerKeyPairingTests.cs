using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SoundHub.Api.Controllers;
using SoundHub.Application.Services;
using SoundHub.Domain.Entities;
using SoundHub.Domain.Interfaces;

namespace SoundHub.Tests.Api;

public class DevicesControllerKeyPairingTests
{
    private readonly IDeviceRepository _repository;
    private readonly DeviceAdapterRegistry _adapterRegistry;
    private readonly IDeviceAdapter _adapter;
    private readonly DevicesController _controller;

    public DevicesControllerKeyPairingTests()
    {
        _repository = Substitute.For<IDeviceRepository>();
        _adapterRegistry = new DeviceAdapterRegistry();
        _adapter = Substitute.For<IDeviceAdapter>();
        _adapter.VendorId.Returns("bose-soundtouch");
        _adapterRegistry.RegisterAdapter(_adapter);

        var serviceLogger = Substitute.For<ILogger<DeviceService>>();
        var deviceService = new DeviceService(_repository, _adapterRegistry, serviceLogger);

        var controllerLogger = Substitute.For<ILogger<DevicesController>>();
        _controller = new DevicesController(deviceService, controllerLogger);
    }

    [Fact]
    public async Task PressKey_SupportedKey_ReturnsOk()
    {
        // Arrange
        var device = CreateDevice("device-1", capabilities: new[] { "power", "volume", "bluetoothPairing" });
        _repository.GetDeviceAsync(device.Id, Arg.Any<CancellationToken>()).Returns(device);

        // Act
        var result = await _controller.PressKey(device.Id, new PressKeyRequest("PLAY_PAUSE"), CancellationToken.None);

        // Assert
        Assert.IsType<OkResult>(result);
        await _adapter.Received(1).PressKeyAsync(device.Id, "PLAY_PAUSE", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PressKey_UnsupportedKey_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.PressKey("device-1", new PressKeyRequest("INVALID_KEY"), CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        await _adapter.DidNotReceive().PressKeyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PressKey_DeviceNotFound_ReturnsNotFound()
    {
        // Arrange
        _repository.GetDeviceAsync("missing", Arg.Any<CancellationToken>()).Returns((Device?)null);

        // Act
        var result = await _controller.PressKey("missing", new PressKeyRequest("PLAY_PAUSE"), CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task EnterBluetoothPairing_DeviceWithoutCapability_ReturnsBadRequest()
    {
        // Arrange
        var device = CreateDevice("device-1", capabilities: new[] { "power", "volume" });
        _repository.GetDeviceAsync(device.Id, Arg.Any<CancellationToken>()).Returns(device);

        // Act
        var result = await _controller.EnterBluetoothPairing(device.Id, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        await _adapter.DidNotReceive().EnterPairingModeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnterBluetoothPairing_Timeout_ReturnsGatewayTimeout()
    {
        // Arrange
        var device = CreateDevice("device-1", capabilities: new[] { "power", "volume", "bluetoothPairing" });
        _repository.GetDeviceAsync(device.Id, Arg.Any<CancellationToken>()).Returns(device);
        _adapter.EnterPairingModeAsync(device.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new TimeoutException("Timed out")));

        // Act
        var result = await _controller.EnterBluetoothPairing(device.Id, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status504GatewayTimeout, objectResult.StatusCode);
    }

    private static Device CreateDevice(string id, IEnumerable<string>? capabilities = null)
    {
        return new Device
        {
            Id = id,
            Vendor = "bose-soundtouch",
            Name = "Test Device",
            IpAddress = "192.168.1.10",
            Capabilities = new HashSet<string>(capabilities ?? Array.Empty<string>()),
            DateTimeAdded = DateTime.UtcNow
        };
    }
}