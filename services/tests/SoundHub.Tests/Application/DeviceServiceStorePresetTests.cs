using Microsoft.Extensions.Logging;
using NSubstitute;
using SoundHub.Application.Services;
using SoundHub.Domain.Entities;
using SoundHub.Domain.Interfaces;

namespace SoundHub.Tests.Application;

/// <summary>
/// Tests for DeviceService.StorePresetAsync, especially the LOCAL_INTERNET_RADIO
/// station-file orchestration flow introduced by add-local-preset-storage.
/// </summary>
public class DeviceServiceStorePresetTests
{
    private readonly IDeviceRepository _repository;
    private readonly DeviceAdapterRegistry _registry;
    private readonly IStationFileService _stationFileService;
    private readonly IDeviceAdapter _adapter;
    private readonly ILogger<DeviceService> _logger;
    private readonly DeviceService _service;

    private readonly Device _device;

    public DeviceServiceStorePresetTests()
    {
        _repository = Substitute.For<IDeviceRepository>();
        _registry = new DeviceAdapterRegistry();
        _stationFileService = Substitute.For<IStationFileService>();
        _adapter = Substitute.For<IDeviceAdapter>();
        _adapter.VendorId.Returns("bose-soundtouch");
        _adapter.VendorName.Returns("Bose SoundTouch");
        _registry.RegisterAdapter(_adapter);
        _logger = Substitute.For<ILogger<DeviceService>>();
        _service = new DeviceService(_repository, _registry, _stationFileService, _logger);

        _device = new Device
        {
            Id = "device-1",
            Vendor = "bose-soundtouch",
            Name = "Living Room",
            IpAddress = "192.168.1.20",
            Capabilities = new HashSet<string> { "power", "volume", "presets" },
            DateTimeAdded = DateTime.UtcNow
        };

        _repository.GetDeviceAsync(_device.Id, Arg.Any<CancellationToken>()).Returns(_device);
    }

    // ── Create station file (new preset) ────────────────────────────────

    [Fact]
    public async Task StorePreset_LocalRadioWithStreamUrl_CreatesStationFile()
    {
        // Arrange
        var preset = MakePreset("Jazz FM", source: "LOCAL_INTERNET_RADIO");
        var request = new StorePresetRequest
        {
            Id = 1,
            Name = "Jazz FM",
            StreamUrl = "http://streams.example.com/jazz"
        };

        _stationFileService.Slugify("Jazz FM").Returns("jazz-fm");
        _stationFileService.CreateAsync("Jazz FM", "http://streams.example.com/jazz", Arg.Any<CancellationToken>())
            .Returns("jazz-fm");
        _stationFileService.GetPublicUrl("jazz-fm").Returns("http://host/presets/jazz-fm.json");

        _adapter.StorePresetAsync(_device.Id, Arg.Any<Preset>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Preset>());

        // Act
        var result = await _service.StorePresetAsync(_device.Id, preset, request);

        // Assert – station file created
        await _stationFileService.Received(1)
            .CreateAsync("Jazz FM", "http://streams.example.com/jazz", Arg.Any<CancellationToken>());

        // Assert – location overridden with public URL
        Assert.Equal("http://host/presets/jazz-fm.json", result.Location);
    }

    // ── Update station file (edit mode) ─────────────────────────────────

    [Fact]
    public async Task StorePreset_LocalRadioIsUpdate_UpdatesStationFile()
    {
        // Arrange
        var preset = MakePreset("Jazz FM", source: "LOCAL_INTERNET_RADIO");
        var request = new StorePresetRequest
        {
            Id = 1,
            Name = "Jazz FM",
            StreamUrl = "http://streams.example.com/jazz-v2",
            IsUpdate = true
        };

        _stationFileService.Slugify("Jazz FM").Returns("jazz-fm");
        _stationFileService.GetPublicUrl("jazz-fm").Returns("http://host/presets/jazz-fm.json");

        _adapter.StorePresetAsync(_device.Id, Arg.Any<Preset>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Preset>());

        // Act
        var result = await _service.StorePresetAsync(_device.Id, preset, request);

        // Assert – update called, not create
        await _stationFileService.Received(1)
            .UpdateAsync("jazz-fm", "Jazz FM", "http://streams.example.com/jazz-v2", Arg.Any<CancellationToken>());
        await _stationFileService.DidNotReceive()
            .CreateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());

        Assert.Equal("http://host/presets/jazz-fm.json", result.Location);
    }

    // ── Non-LOCAL_INTERNET_RADIO is unchanged ───────────────────────────

    [Fact]
    public async Task StorePreset_NonLocalRadio_DoesNotTouchStationFile()
    {
        // Arrange
        var preset = MakePreset("Spotify Playlist", location: "spotify:playlist:abc", source: "SPOTIFY");
        _adapter.StorePresetAsync(_device.Id, Arg.Any<Preset>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Preset>());

        // Act
        var result = await _service.StorePresetAsync(_device.Id, preset);

        // Assert – station file service never called
        await _stationFileService.DidNotReceive()
            .CreateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _stationFileService.DidNotReceive()
            .UpdateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        _stationFileService.DidNotReceive().Slugify(Arg.Any<string>());

        Assert.Equal("spotify:playlist:abc", result.Location);
    }

    // ── LOCAL_INTERNET_RADIO without StreamUrl passes through ───────────

    [Fact]
    public async Task StorePreset_LocalRadioWithoutStreamUrl_DoesNotTouchStationFile()
    {
        // Arrange – no request object at all
        var preset = MakePreset("Classic FM", location: "http://direct.stream/classic", source: "LOCAL_INTERNET_RADIO");
        _adapter.StorePresetAsync(_device.Id, Arg.Any<Preset>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Preset>());

        // Act
        var result = await _service.StorePresetAsync(_device.Id, preset);

        // Assert
        await _stationFileService.DidNotReceive()
            .CreateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        Assert.Equal("http://direct.stream/classic", result.Location);
    }

    // ── Create fails when file already exists ───────────────────────────

    [Fact]
    public async Task StorePreset_DuplicateStationFile_PropagatesException()
    {
        // Arrange
        var preset = MakePreset("Jazz FM", source: "LOCAL_INTERNET_RADIO");
        var request = new StorePresetRequest
        {
            Id = 1,
            Name = "Jazz FM",
            StreamUrl = "http://streams.example.com/jazz"
        };

        _stationFileService.Slugify("Jazz FM").Returns("jazz-fm");
        _stationFileService.CreateAsync("Jazz FM", "http://streams.example.com/jazz", Arg.Any<CancellationToken>())
            .Returns<string>(_ => throw new InvalidOperationException("Station file 'jazz-fm' already exists."));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.StorePresetAsync(_device.Id, preset, request));
        Assert.Contains("already exists", ex.Message);
    }

    // ── Device not found ────────────────────────────────────────────────

    [Fact]
    public async Task StorePreset_DeviceNotFound_ThrowsKeyNotFound()
    {
        _repository.GetDeviceAsync("missing", Arg.Any<CancellationToken>()).Returns((Device?)null);

        var preset = MakePreset("Station");

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.StorePresetAsync("missing", preset));
    }

    // ── Helpers ─────────────────────────────────────────────────────────

    private Preset MakePreset(string name, string? location = null, string source = "LOCAL_INTERNET_RADIO")
    {
        return new Preset
        {
            Id = 1,
            DeviceId = _device.Id,
            Name = name,
            Location = location ?? string.Empty,
            Type = "stationurl",
            Source = source,
            IsPresetable = true
        };
    }
}
