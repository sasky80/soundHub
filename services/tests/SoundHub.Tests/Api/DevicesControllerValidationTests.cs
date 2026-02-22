using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SoundHub.Api.Controllers;
using SoundHub.Application.Services;
using SoundHub.Domain.Interfaces;

namespace SoundHub.Tests.Api;

public class DevicesControllerValidationTests
{
    private readonly DevicesController _controller;

    public DevicesControllerValidationTests()
    {
        var repository = Substitute.For<IDeviceRepository>();
        var adapterRegistry = new DeviceAdapterRegistry();
        var stationFileService = Substitute.For<IStationFileService>();
        var serviceLogger = Substitute.For<ILogger<DeviceService>>();
        var service = new DeviceService(repository, adapterRegistry, stationFileService, serviceLogger);
        var controllerLogger = Substitute.For<ILogger<DevicesController>>();
        _controller = new DevicesController(service, controllerLogger);
    }

    [Fact]
    public async Task AddDevice_OversizedName_ReturnsBadRequest()
    {
        var request = new AddDeviceRequest(new string('a', 101), "192.168.1.10", "bose-soundtouch");

        var result = await _controller.AddDevice(request, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AddDevice_OversizedIpAddress_ReturnsBadRequest()
    {
        var request = new AddDeviceRequest("Living Room", new string('1', 46), "bose-soundtouch");

        var result = await _controller.AddDevice(request, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateDevice_OversizedName_ReturnsBadRequest()
    {
        var request = new UpdateDeviceRequest(new string('a', 101), "192.168.1.10", null);

        var result = await _controller.UpdateDevice("device-1", request, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateDevice_OversizedIpAddress_ReturnsBadRequest()
    {
        var request = new UpdateDeviceRequest("Living Room", new string('1', 46), null);

        var result = await _controller.UpdateDevice("device-1", request, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }
}