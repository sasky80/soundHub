using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SoundHub.Api.Controllers;
using SoundHub.Application.Services;
using SoundHub.Domain.Entities;
using SoundHub.Domain.Interfaces;

namespace SoundHub.Tests.Api;

/// <summary>
/// Unit tests for preset-related endpoints in <see cref="DevicesController"/>.
/// </summary>
public class DevicesControllerPresetTests
{
    private readonly DeviceService _deviceService;
    private readonly IDeviceRepository _repository;
    private readonly DeviceAdapterRegistry _adapterRegistry;
    private readonly IDeviceAdapter _adapter;
    private readonly DevicesController _controller;

    public DevicesControllerPresetTests()
    {
        _repository = Substitute.For<IDeviceRepository>();
        _adapterRegistry = new DeviceAdapterRegistry();
        _adapter = Substitute.For<IDeviceAdapter>();
        _adapter.VendorId.Returns("bose-soundtouch");
        _adapter.VendorName.Returns("Bose SoundTouch");
        _adapterRegistry.RegisterAdapter(_adapter);

        var serviceLogger = Substitute.For<ILogger<DeviceService>>();
        _deviceService = new DeviceService(_repository, _adapterRegistry, serviceLogger);

        var controllerLogger = Substitute.For<ILogger<DevicesController>>();
        _controller = new DevicesController(_deviceService, controllerLogger);
    }

    [Fact]
    public async Task GetPresets_DeviceExists_ReturnsPresetList()
    {
        var device = CreateDevice();
        _repository.GetDeviceAsync(device.Id, Arg.Any<CancellationToken>()).Returns(device);

        var presets = new List<Preset>
        {
            new()
            {
                Id = 1,
                DeviceId = device.Id,
                Name = "LoFi Station",
                Location = "http://example.com/stream",
                IconUrl = "http://example.com/icon.png",
                Type = "stationurl",
                Source = "LOCAL_INTERNET_RADIO",
                IsPresetable = true,
            },
        };

        _adapter.ListPresetsAsync(device.Id, Arg.Any<CancellationToken>()).Returns(presets);

        var result = await _controller.GetPresets(device.Id, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsAssignableFrom<IEnumerable<Preset>>(okResult.Value);
        Assert.Single(value);
    }

    [Fact]
    public async Task GetPresets_DeviceMissing_ReturnsNotFound()
    {
        _repository.GetDeviceAsync("device-1", Arg.Any<CancellationToken>()).Returns((Device?)null);

        var result = await _controller.GetPresets("device-1", CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task StorePreset_InvalidId_ReturnsBadRequest()
    {
        var request = new StorePresetRequest
        {
            Id = 0,
            Name = "Station",
            Location = "http://stream"
        };

        var result = await _controller.StorePreset("device-1", request, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task StorePreset_MissingName_ReturnsBadRequest()
    {
        var request = new StorePresetRequest
        {
            Id = 2,
            Name = string.Empty,
            Location = "http://stream"
        };

        var result = await _controller.StorePreset("device-1", request, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task StorePreset_MissingLocation_ReturnsBadRequest()
    {
        var request = new StorePresetRequest
        {
            Id = 2,
            Name = "Station",
            Location = string.Empty
        };

        var result = await _controller.StorePreset("device-1", request, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task StorePreset_ValidRequest_UsesDefaultTypeAndSource()
    {
        var device = CreateDevice();
        _repository.GetDeviceAsync(device.Id, Arg.Any<CancellationToken>()).Returns(device);

        Preset? capturedPreset = null;
        _adapter.StorePresetAsync(device.Id, Arg.Any<Preset>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                capturedPreset = call.Arg<Preset>();
                return capturedPreset!;
            });

        var request = new StorePresetRequest
        {
            Id = 3,
            Name = "Evening Mix",
            Location = "http://example.com/evening",
            IconUrl = null,
            Type = null,
            Source = null,
        };

        var result = await _controller.StorePreset(device.Id, request, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var preset = Assert.IsType<Preset>(created.Value);
        Assert.Equal("stationurl", preset.Type);
        Assert.Equal("LOCAL_INTERNET_RADIO", preset.Source);
        Assert.Equal(device.Id, preset.DeviceId);
        Assert.NotNull(capturedPreset);
        Assert.Equal("stationurl", capturedPreset!.Type);
        Assert.Equal("LOCAL_INTERNET_RADIO", capturedPreset.Source);
    }

    [Fact]
    public async Task StorePreset_DeviceMissing_ReturnsNotFound()
    {
        _repository.GetDeviceAsync("device-1", Arg.Any<CancellationToken>()).Returns((Device?)null);

        var request = new StorePresetRequest
        {
            Id = 2,
            Name = "Station",
            Location = "http://stream"
        };

        var result = await _controller.StorePreset("device-1", request, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeletePreset_InvalidId_ReturnsBadRequest()
    {
        var result = await _controller.DeletePreset("device-1", 0, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeletePreset_PresetMissing_ReturnsNotFound()
    {
        var device = CreateDevice();
        _repository.GetDeviceAsync(device.Id, Arg.Any<CancellationToken>()).Returns(device);
        _adapter.RemovePresetAsync(device.Id, 2, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _controller.DeletePreset(device.Id, 2, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFound.Value);
    }

    [Fact]
    public async Task DeletePreset_Success_ReturnsNoContent()
    {
        var device = CreateDevice();
        _repository.GetDeviceAsync(device.Id, Arg.Any<CancellationToken>()).Returns(device);
        _adapter.RemovePresetAsync(device.Id, 2, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _controller.DeletePreset(device.Id, 2, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task PlayPreset_AdapterThrowsOutOfRange_ReturnsBadRequest()
    {
        var device = CreateDevice();
        _repository.GetDeviceAsync(device.Id, Arg.Any<CancellationToken>()).Returns(device);
        _adapter.PlayPresetAsync(device.Id, "7", Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ArgumentOutOfRangeException("presetId", "Preset must be between 1 and 6"));

        var result = await _controller.PlayPreset(device.Id, "7", CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    private static Device CreateDevice()
    {
        return new Device
        {
            Id = "device-1",
            Vendor = "bose-soundtouch",
            Name = "Living Room",
            IpAddress = "192.168.1.20",
            Capabilities = new HashSet<string> { "power", "volume", "presets" },
            DateTimeAdded = DateTime.UtcNow
        };
    }
}
