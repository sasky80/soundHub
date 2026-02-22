using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SoundHub.Api.Controllers;
using SoundHub.Application.Services;
using SoundHub.Domain.Entities;
using SoundHub.Domain.Interfaces;

namespace SoundHub.Tests.Api;

/// <summary>
/// Tests for the LOCAL_INTERNET_RADIO / StreamUrl store-preset integration
/// in <see cref="DevicesController"/>.
/// </summary>
public class DevicesControllerStorePresetStreamUrlTests
{
    private readonly IDeviceRepository _repository;
    private readonly IStationFileService _stationFileService;
    private readonly IDeviceAdapter _adapter;
    private readonly DevicesController _controller;

    private readonly Device _device;

    public DevicesControllerStorePresetStreamUrlTests()
    {
        _repository = Substitute.For<IDeviceRepository>();
        var adapterRegistry = new DeviceAdapterRegistry();
        _adapter = Substitute.For<IDeviceAdapter>();
        _adapter.VendorId.Returns("bose-soundtouch");
        _adapter.VendorName.Returns("Bose SoundTouch");
        adapterRegistry.RegisterAdapter(_adapter);

        _stationFileService = Substitute.For<IStationFileService>();

        var serviceLogger = Substitute.For<ILogger<DeviceService>>();
        var deviceService = new DeviceService(_repository, adapterRegistry, _stationFileService, serviceLogger);

        var controllerLogger = Substitute.For<ILogger<DevicesController>>();
        _controller = new DevicesController(deviceService, controllerLogger);

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

    [Fact]
    public async Task StorePreset_StreamUrlProvided_LocationNotRequired()
    {
        // Arrange – no Location but StreamUrl present
        var request = new StorePresetRequest
        {
            Id = 1,
            Name = "Jazz FM",
            StreamUrl = "http://jazz.stream/live"
            // Location is null
        };

        _stationFileService.Slugify("Jazz FM").Returns("jazz-fm");
        _stationFileService.CreateAsync("Jazz FM", "http://jazz.stream/live", Arg.Any<CancellationToken>())
            .Returns("jazz-fm");
        _stationFileService.GetPublicUrl("jazz-fm").Returns("http://host/presets/jazz-fm.json");

        _adapter.StorePresetAsync(_device.Id, Arg.Any<Preset>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Preset>());

        // Act
        var result = await _controller.StorePreset(_device.Id, request, CancellationToken.None);

        // Assert – should succeed (201), not return 400 for missing location
        var created = Assert.IsType<CreatedAtActionResult>(result);
        var preset = Assert.IsType<Preset>(created.Value);
        Assert.Equal("http://host/presets/jazz-fm.json", preset.Location);
    }

    [Fact]
    public async Task StorePreset_NoStreamUrlAndNoLocation_ReturnsBadRequest()
    {
        // Arrange – neither StreamUrl nor Location
        var request = new StorePresetRequest
        {
            Id = 1,
            Name = "Jazz FM"
            // No Location, no StreamUrl
        };

        // Act
        var result = await _controller.StorePreset(_device.Id, request, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task StorePreset_DuplicateStationFile_Returns409Conflict()
    {
        // Arrange
        var request = new StorePresetRequest
        {
            Id = 1,
            Name = "Jazz FM",
            StreamUrl = "http://jazz.stream/live"
        };

        _stationFileService.Slugify("Jazz FM").Returns("jazz-fm");
        _stationFileService.CreateAsync("Jazz FM", "http://jazz.stream/live", Arg.Any<CancellationToken>())
            .Returns<string>(_ => throw new InvalidOperationException("Station file 'jazz-fm' already exists."));

        // Act
        var result = await _controller.StorePreset(_device.Id, request, CancellationToken.None);

        // Assert – should be 409 Conflict
        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.NotNull(conflict.Value);
    }

    [Fact]
    public async Task StorePreset_ExplicitSourceNotLocalRadio_RequiresLocation()
    {
        // Arrange – source=SPOTIFY without Location
        var request = new StorePresetRequest
        {
            Id = 1,
            Name = "My Playlist",
            Source = "SPOTIFY",
            StreamUrl = "http://something" // StreamUrl ignored when source != LOCAL_INTERNET_RADIO
        };

        // Act
        var result = await _controller.StorePreset(_device.Id, request, CancellationToken.None);

        // Assert – should require Location for non-local-radio sources
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
