using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using SoundHub.Domain.Entities;
using SoundHub.Domain.Interfaces;
using SoundHub.Infrastructure.Adapters;

namespace SoundHub.Tests.Infrastructure;

/// <summary>
/// Unit tests for SoundTouchAdapter XML parsing and behavior.
/// Uses mocked HTTP responses to test parsing logic without real network calls.
/// </summary>
public class SoundTouchAdapterTests
{
    private readonly ILogger<SoundTouchAdapter> _logger;
    private readonly IDeviceRepository _deviceRepository;
    private readonly MockHttpMessageHandler _mockHandler;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SoundTouchAdapter _adapter;

    private const string TestDeviceId = "test-device-123";
    private const string TestDeviceIp = "192.168.1.100";

    public SoundTouchAdapterTests()
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
            Vendor = "bose-soundtouch"
        };
        _deviceRepository.GetDeviceAsync(TestDeviceId, Arg.Any<CancellationToken>())
            .Returns(testDevice);

        var options = Options.Create(new SoundTouchAdapterOptions { PingTimeoutSeconds = 10 });
        _adapter = new SoundTouchAdapter(_logger, _httpClientFactory, _deviceRepository, options);
    }

    #region GetDeviceInfoAsync Tests

    [Fact]
    public async Task GetDeviceInfoAsync_ParsesFullResponse_ReturnsDeviceInfo()
    {
        // Arrange
        const string xmlResponse = """
            <?xml version="1.0" encoding="UTF-8" ?>
            <info deviceID="C8DF84AE0B6E">
                <name>Living Room Speaker</name>
                <type>SoundTouch 10</type>
                <margeAccountUUID>1234567</margeAccountUUID>
                <components>
                    <component>
                        <componentCategory>SCM</componentCategory>
                        <softwareVersion>27.0.6.1</softwareVersion>
                        <serialNumber>123456789</serialNumber>
                    </component>
                </components>
                <networkInfo macAddress="C8:DF:84:AE:0B:6E">
                    <ipAddress>192.168.1.100</ipAddress>
                </networkInfo>
            </info>
            """;

        _mockHandler.SetResponse(xmlResponse, HttpStatusCode.OK);

        // Act
        var result = await _adapter.GetDeviceInfoAsync(TestDeviceId);

        // Assert
        Assert.Equal("C8DF84AE0B6E", result.DeviceId);
        Assert.Equal("Living Room Speaker", result.Name);
        Assert.Equal("SoundTouch 10", result.Type);
        Assert.Equal("C8:DF:84:AE:0B:6E", result.MacAddress);
        Assert.Equal("192.168.1.100", result.IpAddress);
        Assert.Equal("27.0.6.1", result.SoftwareVersion);
    }

    [Fact]
    public async Task GetDeviceInfoAsync_MinimalResponse_ReturnsDefaults()
    {
        // Arrange
        const string xmlResponse = """
            <?xml version="1.0" encoding="UTF-8" ?>
            <info deviceID="ABC123">
                <name>Speaker</name>
                <type>Unknown</type>
            </info>
            """;

        _mockHandler.SetResponse(xmlResponse, HttpStatusCode.OK);

        // Act
        var result = await _adapter.GetDeviceInfoAsync(TestDeviceId);

        // Assert
        Assert.Equal("ABC123", result.DeviceId);
        Assert.Equal("Speaker", result.Name);
        Assert.Equal("Unknown", result.Type);
        Assert.Null(result.MacAddress);
        Assert.Equal(TestDeviceIp, result.IpAddress); // Falls back to device IP
        Assert.Null(result.SoftwareVersion);
    }

    [Fact]
    public async Task GetDeviceInfoAsync_DeviceUnreachable_ThrowsInvalidOperation()
    {
        // Arrange
        _mockHandler.SetException(new HttpRequestException("Connection refused"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _adapter.GetDeviceInfoAsync(TestDeviceId));
        Assert.Contains("not reachable", ex.Message);
    }

    #endregion

    #region GetNowPlayingAsync Tests

    [Fact]
    public async Task GetNowPlayingAsync_MusicPlaying_ReturnsFullInfo()
    {
        // Arrange
        const string xmlResponse = """
            <?xml version="1.0" encoding="UTF-8" ?>
            <nowPlaying deviceID="C8DF84AE0B6E" source="SPOTIFY">
                <ContentItem source="SPOTIFY" type="uri" location="/v2/playback">
                    <itemName>My Playlist</itemName>
                </ContentItem>
                <track>Bohemian Rhapsody</track>
                <artist>Queen</artist>
                <album>A Night at the Opera</album>
                <stationName>My Playlist</stationName>
                <art artImageStatus="IMAGE_PRESENT">https://example.com/art.jpg</art>
                <playStatus>PLAY_STATE</playStatus>
                <streamType>TRACK_ONDEMAND</streamType>
            </nowPlaying>
            """;

        _mockHandler.SetResponse(xmlResponse, HttpStatusCode.OK);

        // Act
        var result = await _adapter.GetNowPlayingAsync(TestDeviceId);

        // Assert
        Assert.Equal("SPOTIFY", result.Source);
        Assert.Equal("Bohemian Rhapsody", result.Track);
        Assert.Equal("Queen", result.Artist);
        Assert.Equal("A Night at the Opera", result.Album);
        Assert.Equal("My Playlist", result.StationName);
        Assert.Equal("PLAY_STATE", result.PlayStatus);
        Assert.Equal("https://example.com/art.jpg", result.ArtUrl);
    }

    [Fact]
    public async Task GetNowPlayingAsync_Standby_ReturnsStandbySource()
    {
        // Arrange
        const string xmlResponse = """
            <?xml version="1.0" encoding="UTF-8" ?>
            <nowPlaying deviceID="C8DF84AE0B6E" source="STANDBY">
                <ContentItem source="STANDBY" isPresetable="false" />
            </nowPlaying>
            """;

        _mockHandler.SetResponse(xmlResponse, HttpStatusCode.OK);

        // Act
        var result = await _adapter.GetNowPlayingAsync(TestDeviceId);

        // Assert
        Assert.Equal("STANDBY", result.Source);
        Assert.Null(result.Track);
        Assert.Null(result.Artist);
        Assert.Null(result.Album);
    }

    [Fact]
    public async Task GetNowPlayingAsync_Bluetooth_ReturnsBluetoothInfo()
    {
        // Arrange
        const string xmlResponse = """
            <?xml version="1.0" encoding="UTF-8" ?>
            <nowPlaying deviceID="C8DF84AE0B6E" source="BLUETOOTH">
                <track>Unknown Track</track>
                <artist>Unknown Artist</artist>
                <playStatus>PLAY_STATE</playStatus>
            </nowPlaying>
            """;

        _mockHandler.SetResponse(xmlResponse, HttpStatusCode.OK);

        // Act
        var result = await _adapter.GetNowPlayingAsync(TestDeviceId);

        // Assert
        Assert.Equal("BLUETOOTH", result.Source);
        Assert.Equal("Unknown Track", result.Track);
        Assert.Equal("Unknown Artist", result.Artist);
    }

    #endregion

    #region GetVolumeAsync Tests

    [Fact]
    public async Task GetVolumeAsync_NormalVolume_ReturnsVolumeInfo()
    {
        // Arrange
        const string xmlResponse = """
            <?xml version="1.0" encoding="UTF-8" ?>
            <volume deviceID="C8DF84AE0B6E">
                <targetvolume>45</targetvolume>
                <actualvolume>45</actualvolume>
                <muteenabled>false</muteenabled>
            </volume>
            """;

        _mockHandler.SetResponse(xmlResponse, HttpStatusCode.OK);

        // Act
        var result = await _adapter.GetVolumeAsync(TestDeviceId);

        // Assert
        Assert.Equal(45, result.TargetVolume);
        Assert.Equal(45, result.ActualVolume);
        Assert.False(result.IsMuted);
    }

    [Fact]
    public async Task GetVolumeAsync_Muted_ReturnsMutedState()
    {
        // Arrange
        const string xmlResponse = """
            <?xml version="1.0" encoding="UTF-8" ?>
            <volume deviceID="C8DF84AE0B6E">
                <targetvolume>50</targetvolume>
                <actualvolume>0</actualvolume>
                <muteenabled>true</muteenabled>
            </volume>
            """;

        _mockHandler.SetResponse(xmlResponse, HttpStatusCode.OK);

        // Act
        var result = await _adapter.GetVolumeAsync(TestDeviceId);

        // Assert
        Assert.Equal(50, result.TargetVolume);
        Assert.Equal(0, result.ActualVolume);
        Assert.True(result.IsMuted);
    }

    [Fact]
    public async Task GetVolumeAsync_VolumeTransitioning_ShowsDifferentValues()
    {
        // Arrange
        const string xmlResponse = """
            <?xml version="1.0" encoding="UTF-8" ?>
            <volume deviceID="C8DF84AE0B6E">
                <targetvolume>60</targetvolume>
                <actualvolume>55</actualvolume>
                <muteenabled>false</muteenabled>
            </volume>
            """;

        _mockHandler.SetResponse(xmlResponse, HttpStatusCode.OK);

        // Act
        var result = await _adapter.GetVolumeAsync(TestDeviceId);

        // Assert
        Assert.Equal(60, result.TargetVolume);
        Assert.Equal(55, result.ActualVolume);
    }

    #endregion

    #region ListPresetsAsync Tests

    [Fact]
    public async Task ListPresetsAsync_MultiplePresets_ReturnsAllPresets()
    {
        // Arrange
        const string xmlResponse = """
            <?xml version="1.0" encoding="UTF-8" ?>
            <presets deviceID="C8DF84AE0B6E">
                <preset id="1" createdOn="1234567890" updatedOn="1234567890">
                    <ContentItem source="TUNEIN" type="stationurl" location="/v1/playback/station/s12345">
                        <itemName>BBC Radio 1</itemName>
                    </ContentItem>
                </preset>
                <preset id="2" createdOn="1234567890" updatedOn="1234567890">
                    <ContentItem source="SPOTIFY" type="uri" location="spotify:playlist:123">
                        <itemName>My Playlist</itemName>
                    </ContentItem>
                </preset>
                <preset id="3" createdOn="1234567890" updatedOn="1234567890">
                    <ContentItem source="AMAZON" type="uri" location="amazon:station:456">
                        <itemName>Chill Vibes</itemName>
                    </ContentItem>
                </preset>
            </presets>
            """;

        _mockHandler.SetResponse(xmlResponse, HttpStatusCode.OK);

        // Act
        var result = await _adapter.ListPresetsAsync(TestDeviceId);

        // Assert
        Assert.Equal(3, result.Count);

        Assert.Equal(1, result[0].Id);
        Assert.Equal("BBC Radio 1", result[0].Name);
        Assert.Equal("TUNEIN", result[0].Source);

        Assert.Equal(2, result[1].Id);
        Assert.Equal("My Playlist", result[1].Name);
        Assert.Equal("SPOTIFY", result[1].Source);

        Assert.Equal(3, result[2].Id);
        Assert.Equal("Chill Vibes", result[2].Name);
        Assert.Equal("AMAZON", result[2].Source);
    }

    [Fact]
    public async Task ListPresetsAsync_NoPresets_ReturnsEmptyList()
    {
        // Arrange
        const string xmlResponse = """
            <?xml version="1.0" encoding="UTF-8" ?>
            <presets deviceID="C8DF84AE0B6E">
            </presets>
            """;

        _mockHandler.SetResponse(xmlResponse, HttpStatusCode.OK);

        // Act
        var result = await _adapter.ListPresetsAsync(TestDeviceId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ListPresetsAsync_PartialPresets_SkipsEmptySlots()
    {
        // Arrange - Only presets 1 and 4 configured
        const string xmlResponse = """
            <?xml version="1.0" encoding="UTF-8" ?>
            <presets deviceID="C8DF84AE0B6E">
                <preset id="1">
                    <ContentItem source="TUNEIN" location="/v1/playback/station/s1">
                        <itemName>Station 1</itemName>
                    </ContentItem>
                </preset>
                <preset id="4">
                    <ContentItem source="SPOTIFY" location="spotify:playlist:abc">
                        <itemName>Playlist</itemName>
                    </ContentItem>
                </preset>
            </presets>
            """;

        _mockHandler.SetResponse(xmlResponse, HttpStatusCode.OK);

        // Act
        var result = await _adapter.ListPresetsAsync(TestDeviceId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal(4, result[1].Id);
    }

    #endregion

    #region PlayPresetAsync Tests

    [Fact]
    public async Task PlayPresetAsync_ValidPreset_SendsKeyPress()
    {
        // Arrange
        _mockHandler.SetResponse("", HttpStatusCode.OK);

        // Act
        await _adapter.PlayPresetAsync(TestDeviceId, "3");

        // Assert - Should have made 2 POST requests (press and release)
        Assert.Equal(2, _mockHandler.RequestCount);
        Assert.Contains("PRESET_3", _mockHandler.LastRequestContent);
    }

    [Fact]
    public async Task PlayPresetAsync_InvalidPresetZero_ThrowsArgumentOutOfRange()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _adapter.PlayPresetAsync(TestDeviceId, "0"));
        Assert.Contains("1 and 6", ex.Message);
    }

    [Fact]
    public async Task PlayPresetAsync_InvalidPresetSeven_ThrowsArgumentOutOfRange()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _adapter.PlayPresetAsync(TestDeviceId, "7"));
        Assert.Contains("1 and 6", ex.Message);
    }

    [Fact]
    public async Task PlayPresetAsync_NonNumericPreset_ThrowsArgumentOutOfRange()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _adapter.PlayPresetAsync(TestDeviceId, "abc"));
    }

    #endregion

    #region SetVolumeAsync Tests

    [Fact]
    public async Task SetVolumeAsync_ValidVolume_SendsRequest()
    {
        // Arrange
        _mockHandler.SetResponse("", HttpStatusCode.OK);

        // Act
        await _adapter.SetVolumeAsync(TestDeviceId, 50);

        // Assert
        Assert.Equal(1, _mockHandler.RequestCount);
        Assert.Contains("<volume>50</volume>", _mockHandler.LastRequestContent);
    }

    [Fact]
    public async Task SetVolumeAsync_MinVolume_SendsRequest()
    {
        // Arrange
        _mockHandler.SetResponse("", HttpStatusCode.OK);

        // Act
        await _adapter.SetVolumeAsync(TestDeviceId, 0);

        // Assert
        Assert.Contains("<volume>0</volume>", _mockHandler.LastRequestContent);
    }

    [Fact]
    public async Task SetVolumeAsync_MaxVolume_SendsRequest()
    {
        // Arrange
        _mockHandler.SetResponse("", HttpStatusCode.OK);

        // Act
        await _adapter.SetVolumeAsync(TestDeviceId, 100);

        // Assert
        Assert.Contains("<volume>100</volume>", _mockHandler.LastRequestContent);
    }

    [Fact]
    public async Task SetVolumeAsync_NegativeVolume_ThrowsArgumentOutOfRange()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _adapter.SetVolumeAsync(TestDeviceId, -1));
    }

    [Fact]
    public async Task SetVolumeAsync_VolumeOver100_ThrowsArgumentOutOfRange()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _adapter.SetVolumeAsync(TestDeviceId, 101));
    }

    #endregion

    #region GetStatusAsync Tests

    [Fact]
    public async Task GetStatusAsync_DevicePlaying_ReturnsOnlineStatus()
    {
        // Arrange - Need to handle multiple requests
        var responses = new Queue<string>();
        responses.Enqueue("""
            <volume deviceID="test">
                <targetvolume>45</targetvolume>
                <actualvolume>45</actualvolume>
                <muteenabled>false</muteenabled>
            </volume>
            """);
        responses.Enqueue("""
            <nowPlaying deviceID="test" source="SPOTIFY">
                <playStatus>PLAY_STATE</playStatus>
            </nowPlaying>
            """);

        _mockHandler.SetResponseQueue(responses);

        // Act
        var result = await _adapter.GetStatusAsync(TestDeviceId);

        // Assert
        Assert.True(result.IsOnline);
        Assert.True(result.PowerState);
        Assert.Equal(45, result.Volume);
        Assert.Equal("SPOTIFY", result.CurrentSource);
    }

    [Fact]
    public async Task GetStatusAsync_DeviceInStandby_ReturnsPowerOff()
    {
        // Arrange
        var responses = new Queue<string>();
        responses.Enqueue("""
            <volume deviceID="test">
                <targetvolume>0</targetvolume>
                <actualvolume>0</actualvolume>
                <muteenabled>false</muteenabled>
            </volume>
            """);
        responses.Enqueue("""
            <nowPlaying deviceID="test" source="STANDBY">
            </nowPlaying>
            """);

        _mockHandler.SetResponseQueue(responses);

        // Act
        var result = await _adapter.GetStatusAsync(TestDeviceId);

        // Assert
        Assert.True(result.IsOnline);
        Assert.False(result.PowerState); // STANDBY means power off
        Assert.Equal("STANDBY", result.CurrentSource);
    }

    [Fact]
    public async Task GetStatusAsync_DeviceUnreachable_ReturnsOfflineStatus()
    {
        // Arrange
        _mockHandler.SetException(new HttpRequestException("Network error"));

        // Act
        var result = await _adapter.GetStatusAsync(TestDeviceId);

        // Assert
        Assert.False(result.IsOnline);
        Assert.False(result.PowerState);
        Assert.Equal(0, result.Volume);
    }

    #endregion

    #region Device Lookup Tests

    [Fact]
    public async Task GetDeviceInfoAsync_DeviceNotFound_ThrowsKeyNotFound()
    {
        // Arrange
        _deviceRepository.GetDeviceAsync("unknown-device", Arg.Any<CancellationToken>())
            .Returns((Device?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _adapter.GetDeviceInfoAsync("unknown-device"));
    }

    #endregion

    #region Capabilities Tests

    [Fact]
    public async Task GetCapabilitiesAsync_WithSupportedUrls_ReturnsAllCapabilities()
    {
        // Arrange - mock /supportedURLs response with all capabilities
        const string supportedUrlsResponse = """
            <?xml version="1.0" encoding="UTF-8" ?>
            <supportedURLs deviceID="C8DF84AE0B6E">
                <supportedURL>/info</supportedURL>
                <supportedURL>/volume</supportedURL>
                <supportedURL>/presets</supportedURL>
                <supportedURL>/enterBluetoothPairing</supportedURL>
                <supportedURL>/playNotification</supportedURL>
            </supportedURLs>
            """;

        _mockHandler.SetResponse(supportedUrlsResponse, HttpStatusCode.OK);

        // Act
        var capabilities = await _adapter.GetCapabilitiesAsync(TestDeviceIp);

        // Assert - base capabilities plus dynamic ones
        Assert.Contains("power", capabilities);
        Assert.Contains("volume", capabilities);
        Assert.Contains("presets", capabilities);
        Assert.Contains("bluetoothPairing", capabilities);
        Assert.Contains("ping", capabilities);
    }

    [Fact]
    public async Task GetCapabilitiesAsync_WhenSupportedUrlsFails_ReturnsBaseCapabilities()
    {
        // Arrange - mock failure
        _mockHandler.SetResponse("error", HttpStatusCode.InternalServerError);

        // Act
        var capabilities = await _adapter.GetCapabilitiesAsync(TestDeviceIp);

        // Assert - only base capabilities
        Assert.Contains("power", capabilities);
        Assert.Contains("volume", capabilities);
        Assert.Equal(2, capabilities.Count);
    }

    #endregion

    #region VendorId Tests

    [Fact]
    public void VendorId_ReturnsBoseSoundtouch()
    {
        Assert.Equal("bose-soundtouch", _adapter.VendorId);
    }

    #endregion
}

/// <summary>
/// Mock HTTP message handler for testing HTTP-based adapters.
/// </summary>
public class MockHttpMessageHandler : HttpMessageHandler
{
    private string _responseContent = "";
    private HttpStatusCode _statusCode = HttpStatusCode.OK;
    private Exception? _exception;
    private Queue<string>? _responseQueue;

    public int RequestCount { get; private set; }
    public string LastRequestContent { get; private set; } = "";
    public string LastRequestUrl { get; private set; } = "";

    public void SetResponse(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _responseContent = content;
        _statusCode = statusCode;
        _exception = null;
        _responseQueue = null;
    }

    public void SetResponseQueue(Queue<string> responses)
    {
        _responseQueue = responses;
        _exception = null;
    }

    public void SetException(Exception exception)
    {
        _exception = exception;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        RequestCount++;
        LastRequestUrl = request.RequestUri?.ToString() ?? "";

        if (request.Content != null)
        {
            LastRequestContent = await request.Content.ReadAsStringAsync(cancellationToken);
        }

        if (_exception != null)
        {
            throw _exception;
        }

        var content = _responseQueue != null && _responseQueue.Count > 0
            ? _responseQueue.Dequeue()
            : _responseContent;

        return new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/xml")
        };
    }
}
