using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using SoundHub.Api.Controllers;
using SoundHub.Application.Services;
using SoundHub.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace SoundHub.Tests.Api;

/// <summary>
/// Unit tests for ConfigController network mask endpoints.
/// </summary>
public class ConfigControllerTests
{
    private readonly DeviceService _deviceService;
    private readonly IDeviceRepository _repository;
    private readonly ConfigController _controller;

    public ConfigControllerTests()
    {
        _repository = Substitute.For<IDeviceRepository>();
        var adapterRegistry = new DeviceAdapterRegistry();
        var serviceLogger = Substitute.For<ILogger<DeviceService>>();
        _deviceService = new DeviceService(_repository, adapterRegistry, serviceLogger);
        _controller = new ConfigController(_deviceService);
    }

    #region GET /api/config/network-mask Tests

    [Fact]
    public async Task GetNetworkMask_WhenConfigured_ReturnsNetworkMask()
    {
        // Arrange
        _repository.GetNetworkMaskAsync(Arg.Any<CancellationToken>())
            .Returns("192.168.1.0/24");

        // Act
        var result = await _controller.GetNetworkMask(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<NetworkMaskResponse>(okResult.Value);
        Assert.Equal("192.168.1.0/24", response.NetworkMask);
    }

    [Fact]
    public async Task GetNetworkMask_WhenNotConfigured_ReturnsNull()
    {
        // Arrange
        _repository.GetNetworkMaskAsync(Arg.Any<CancellationToken>())
            .Returns((string?)null);

        // Act
        var result = await _controller.GetNetworkMask(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<NetworkMaskResponse>(okResult.Value);
        Assert.Null(response.NetworkMask);
    }

    #endregion

    #region PUT /api/config/network-mask Tests

    [Fact]
    public async Task SetNetworkMask_ValidMask_ReturnsNoContent()
    {
        // Arrange
        var request = new SetNetworkMaskRequest("192.168.1.0/24");
        _repository.SetNetworkMaskAsync("192.168.1.0/24", Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.SetNetworkMask(request, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        await _repository.Received(1).SetNetworkMaskAsync("192.168.1.0/24", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetNetworkMask_InvalidMask_ReturnsBadRequest()
    {
        // Arrange
        var request = new SetNetworkMaskRequest("not-a-valid-mask");

        // Act
        var result = await _controller.SetNetworkMask(request, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task SetNetworkMask_EmptyMask_ReturnsBadRequest()
    {
        // Arrange
        var request = new SetNetworkMaskRequest("");

        // Act
        var result = await _controller.SetNetworkMask(request, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SetNetworkMask_NullMask_ReturnsBadRequest()
    {
        // Arrange
        var request = new SetNetworkMaskRequest(null!);

        // Act
        var result = await _controller.SetNetworkMask(request, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    #endregion
}
