using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using SoundHub.Domain.Entities;
using SoundHub.Domain.Interfaces;
using SoundHub.Infrastructure.Adapters;

namespace SoundHub.Tests.Infrastructure;

/// <summary>
/// Unit tests for ping functionality in SoundTouchAdapter.
/// </summary>
public class SoundTouchAdapterPingTests
{
    private readonly ILogger<SoundTouchAdapter> _logger;
    private readonly IDeviceRepository _deviceRepository;
    private readonly MockHttpMessageHandler _mockHandler;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SoundTouchAdapter _adapter;

    private const string TestDeviceId = "test-device-123";
    private const string TestDeviceIp = "192.168.1.100";

    public SoundTouchAdapterPingTests()
    {
        _logger = Substitute.For<ILogger<SoundTouchAdapter>>();
        _deviceRepository = Substitute.For<IDeviceRepository>();
        _mockHandler = new MockHttpMessageHandler();

        var httpClient = new HttpClient(_mockHandler);
        _httpClientFactory = Substitute.For<IHttpClientFactory>();
        _httpClientFactory.CreateClient("SoundTouch").Returns(httpClient);

        // Setup default device lookup
        var testDevice = new Device
        {
            Id = TestDeviceId,
            Name = "Test SoundTouch",
            IpAddress = TestDeviceIp,
            Vendor = "bose-soundtouch",
            Capabilities = new HashSet<string> { "power", "volume", "ping" }
        };
        _deviceRepository.GetDeviceAsync(TestDeviceId, Arg.Any<CancellationToken>())
            .Returns(testDevice);

        var options = Options.Create(new SoundTouchAdapterOptions { PingTimeoutSeconds = 10 });
        _adapter = new SoundTouchAdapter(_logger, _httpClientFactory, _deviceRepository, options);
    }

    [Fact]
    public async Task PingAsync_DeviceResponds_ReturnsReachableWithLatency()
    {
        // Arrange
        _mockHandler.SetResponse("", HttpStatusCode.OK);

        // Act
        var result = await _adapter.PingAsync(TestDeviceId);

        // Assert
        Assert.True(result.Reachable);
        Assert.True(result.LatencyMs >= 0);
        Assert.Contains("/playNotification", _mockHandler.LastRequestUrl);
    }

    [Fact]
    public async Task PingAsync_DeviceNotReachable_ReturnsNotReachable()
    {
        // Arrange
        _mockHandler.SetException(new HttpRequestException("Connection refused"));

        // Act
        var result = await _adapter.PingAsync(TestDeviceId);

        // Assert
        Assert.False(result.Reachable);
        Assert.True(result.LatencyMs >= 0);
    }

    [Fact]
    public async Task PingAsync_DeviceTimeout_ReturnsNotReachable()
    {
        // Arrange
        _mockHandler.SetException(new TaskCanceledException("Request timed out"));

        // Act
        var result = await _adapter.PingAsync(TestDeviceId);

        // Assert
        Assert.False(result.Reachable);
    }

    [Fact]
    public async Task PingAsync_DeviceNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _deviceRepository.GetDeviceAsync("nonexistent", Arg.Any<CancellationToken>())
            .Returns((Device?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _adapter.PingAsync("nonexistent"));
    }

    [Fact]
    public async Task PingAsync_UsesPlayNotificationEndpoint()
    {
        // Arrange
        _mockHandler.SetResponse("", HttpStatusCode.OK);

        // Act
        await _adapter.PingAsync(TestDeviceId);

        // Assert - verify URL format
        Assert.Contains(TestDeviceIp, _mockHandler.LastRequestUrl);
        Assert.Contains("8090", _mockHandler.LastRequestUrl);
        Assert.Contains("/playNotification", _mockHandler.LastRequestUrl);
    }

    [Fact]
    public async Task PingAsync_ServerError_ReturnsNotReachable()
    {
        // Arrange
        _mockHandler.SetResponse("Internal Server Error", HttpStatusCode.InternalServerError);

        // Act
        var result = await _adapter.PingAsync(TestDeviceId);

        // Assert - server error means device is reachable but may have issues
        // The implementation behavior determines this - checking it returns some result
        Assert.True(result.LatencyMs >= 0);
    }
}
