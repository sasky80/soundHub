using Microsoft.AspNetCore.Mvc;
using SoundHub.Application.Services;
using SoundHub.Domain.Entities;

namespace SoundHub.Api.Controllers;

/// <summary>
/// Device management endpoints: add, remove, list, discover devices.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly DeviceService _deviceService;
    private readonly ILogger<DevicesController> _logger;

    public DevicesController(DeviceService deviceService, ILogger<DevicesController> logger)
    {
        _deviceService = deviceService;
        _logger = logger;
    }

    /// <summary>
    /// Lists all registered devices.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Device>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllDevices(CancellationToken ct)
    {
        var devices = await _deviceService.GetAllDevicesAsync(ct);
        return Ok(devices);
    }

    /// <summary>
    /// Gets a specific device by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Device), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDevice(string id, CancellationToken ct)
    {
        var device = await _deviceService.GetDeviceAsync(id, ct);
        if (device == null)
        {
            return NotFound(new { code = "DEVICE_NOT_FOUND", message = $"Device with ID {id} not found" });
        }
        return Ok(device);
    }

    /// <summary>
    /// Adds a new device manually.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Device), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddDevice([FromBody] AddDeviceRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.IpAddress) || string.IsNullOrWhiteSpace(request.Vendor))
        {
            return BadRequest(new { code = "INVALID_INPUT", message = "Name, IpAddress, and Vendor are required" });
        }

        var device = await _deviceService.AddDeviceAsync(request.Name, request.IpAddress, request.Vendor, request.Port ?? 8090, ct);
        return CreatedAtAction(nameof(GetDevice), new { id = device.Id }, device);
    }

    /// <summary>
    /// Removes a device by ID.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveDevice(string id, CancellationToken ct)
    {
        var removed = await _deviceService.RemoveDeviceAsync(id, ct);
        if (!removed)
        {
            return NotFound(new { code = "DEVICE_NOT_FOUND", message = $"Device with ID {id} not found" });
        }
        return NoContent();
    }

    /// <summary>
    /// Discovers devices on the local network.
    /// </summary>
    [HttpGet("discover")]
    [ProducesResponseType(typeof(IEnumerable<Device>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DiscoverDevices([FromQuery] string? vendor = null, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting device discovery (vendor: {Vendor})", vendor ?? "all");
        var devices = await _deviceService.DiscoverDevicesAsync(vendor, ct);
        return Ok(devices);
    }

    /// <summary>
    /// Gets the current status of a device.
    /// </summary>
    [HttpGet("{id}/status")]
    [ProducesResponseType(typeof(DeviceStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDeviceStatus(string id, CancellationToken ct)
    {
        try
        {
            var status = await _deviceService.GetDeviceStatusAsync(id, ct);
            return Ok(status);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { code = "DEVICE_NOT_FOUND", message = $"Device with ID {id} not found" });
        }
        catch (NotSupportedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new { code = "NOT_SUPPORTED", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting device status for {DeviceId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { code = "INTERNAL_ERROR", message = "Failed to get device status" });
        }
    }

    /// <summary>
    /// Sets the power state of a device.
    /// </summary>
    [HttpPost("{id}/power")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> SetPower(string id, [FromBody] SetPowerRequest request, CancellationToken ct)
    {
        try
        {
            await _deviceService.SetPowerAsync(id, request.On, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { code = "DEVICE_NOT_FOUND", message = $"Device with ID {id} not found" });
        }
        catch (NotSupportedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new { code = "NOT_SUPPORTED", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting power state for {DeviceId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { code = "INTERNAL_ERROR", message = "Failed to set power state" });
        }
    }
}

public record AddDeviceRequest(string Name, string IpAddress, string Vendor, int? Port);
public record SetPowerRequest(bool On);
