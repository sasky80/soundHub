using SoundHub.Application.Services;
using SoundHub.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace SoundHub.Tests.Application;

/// <summary>
/// Unit tests for network mask parsing and validation in DeviceService.
/// </summary>
public class NetworkMaskValidationTests
{
    private readonly IDeviceRepository _repository;
    private readonly DeviceAdapterRegistry _registry;
    private readonly ILogger<DeviceService> _logger;
    private readonly DeviceService _service;

    public NetworkMaskValidationTests()
    {
        _repository = Substitute.For<IDeviceRepository>();
        _registry = new DeviceAdapterRegistry();
        _logger = Substitute.For<ILogger<DeviceService>>();
        _service = new DeviceService(_repository, _registry, _logger);
    }

    [Theory]
    [InlineData("192.168.1.0/24")]
    [InlineData("10.0.0.0/8")]
    [InlineData("172.16.0.0/12")]
    [InlineData("192.168.0.0/16")]
    [InlineData("0.0.0.0/0")]
    [InlineData("255.255.255.255/32")]
    public async Task SetNetworkMaskAsync_ValidCidr_Succeeds(string networkMask)
    {
        // Arrange
        _repository.SetNetworkMaskAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act & Assert - should not throw
        await _service.SetNetworkMaskAsync(networkMask);
        await _repository.Received(1).SetNetworkMaskAsync(networkMask, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("192.168.1.0", "Missing CIDR prefix")]
    [InlineData("192.168.1.0/", "Empty CIDR prefix")]
    [InlineData("192.168.1.0/33", "CIDR prefix too large")]
    [InlineData("192.168.1.0/-1", "Negative CIDR prefix")]
    [InlineData("192.168.1.0/abc", "Non-numeric CIDR prefix")]
    [InlineData("/24", "Missing IP address")]
    [InlineData("192.168.1/24", "Incomplete IP address")]
    [InlineData("192.168.1.256/24", "Invalid octet value")]
    [InlineData("192.168.1.abc/24", "Non-numeric octet")]
    [InlineData("192.168.1.0.0/24", "Too many octets")]
    [InlineData("", "Empty string")]
    [InlineData("   ", "Whitespace only")]
    [InlineData("not-a-cidr", "Invalid format")]
    public async Task SetNetworkMaskAsync_InvalidCidr_ThrowsArgumentException(string networkMask, string testCase)
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.SetNetworkMaskAsync(networkMask));

        Assert.Contains("Invalid network mask format", ex.Message);
        await _repository.DidNotReceive().SetNetworkMaskAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetNetworkMaskAsync_ReturnsStoredValue()
    {
        // Arrange
        var expectedMask = "192.168.1.0/24";
        _repository.GetNetworkMaskAsync(Arg.Any<CancellationToken>())
            .Returns(expectedMask);

        // Act
        var result = await _service.GetNetworkMaskAsync();

        // Assert
        Assert.Equal(expectedMask, result);
    }

    [Fact]
    public async Task GetNetworkMaskAsync_WhenNotSet_ReturnsNull()
    {
        // Arrange
        _repository.GetNetworkMaskAsync(Arg.Any<CancellationToken>())
            .Returns((string?)null);

        // Act
        var result = await _service.GetNetworkMaskAsync();

        // Assert
        Assert.Null(result);
    }
}
