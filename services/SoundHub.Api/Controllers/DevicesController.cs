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

        try
        {
            var device = await _deviceService.AddDeviceAsync(request.Name, request.IpAddress, request.Vendor, ct);
            return CreatedAtAction(nameof(GetDevice), new { id = device.Id }, device);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { code = "INVALID_INPUT", message = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing device.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Device), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateDevice(string id, [FromBody] UpdateDeviceRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.IpAddress))
        {
            return BadRequest(new { code = "INVALID_INPUT", message = "Name and IpAddress are required" });
        }

        try
        {
            var device = await _deviceService.UpdateDeviceAsync(id, request.Name, request.IpAddress, request.Capabilities, ct);
            return Ok(device);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { code = "DEVICE_NOT_FOUND", message = $"Device with ID {id} not found" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { code = "INVALID_INPUT", message = ex.Message });
        }
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
    /// Pings a device for audible connectivity verification.
    /// </summary>
    [HttpGet("{id}/ping")]
    [ProducesResponseType(typeof(PingResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> PingDevice(string id, CancellationToken ct)
    {
        try
        {
            var result = await _deviceService.PingDeviceAsync(id, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { code = "DEVICE_NOT_FOUND", message = $"Device with ID {id} not found" });
        }
        catch (NotSupportedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new { code = "NOT_SUPPORTED", message = ex.Message });
        }
    }

    /// <summary>
    /// Discovers devices on the local network and auto-saves new devices.
    /// </summary>
    [HttpPost("discover")]
    [ProducesResponseType(typeof(DiscoveryResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> DiscoverDevices(CancellationToken ct)
    {
        _logger.LogInformation("Starting device discovery");
        var result = await _deviceService.DiscoverAndSaveDevicesAsync(ct);
        return Ok(result);
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

    /// <summary>
    /// Gets detailed info for a device.
    /// </summary>
    [HttpGet("{id}/info")]
    [ProducesResponseType(typeof(DeviceInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> GetDeviceInfo(string id, CancellationToken ct)
    {
        try
        {
            var info = await _deviceService.GetDeviceInfoAsync(id, ct);
            return Ok(info);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { code = "DEVICE_NOT_FOUND", message = $"Device with ID {id} not found" });
        }
        catch (NotSupportedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new { code = "NOT_SUPPORTED", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { code = "DEVICE_UNREACHABLE", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting device info for {DeviceId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { code = "INTERNAL_ERROR", message = "Failed to get device info" });
        }
    }

    /// <summary>
    /// Gets the now playing information for a device.
    /// </summary>
    [HttpGet("{id}/nowPlaying")]
    [ProducesResponseType(typeof(NowPlayingInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> GetNowPlaying(string id, CancellationToken ct)
    {
        try
        {
            var nowPlaying = await _deviceService.GetNowPlayingAsync(id, ct);
            return Ok(nowPlaying);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { code = "DEVICE_NOT_FOUND", message = $"Device with ID {id} not found" });
        }
        catch (NotSupportedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new { code = "NOT_SUPPORTED", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { code = "DEVICE_UNREACHABLE", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting now playing for {DeviceId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { code = "INTERNAL_ERROR", message = "Failed to get now playing" });
        }
    }

    /// <summary>
    /// Gets the volume information for a device.
    /// </summary>
    [HttpGet("{id}/volume")]
    [ProducesResponseType(typeof(VolumeInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> GetVolume(string id, CancellationToken ct)
    {
        try
        {
            var volume = await _deviceService.GetVolumeAsync(id, ct);
            return Ok(volume);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { code = "DEVICE_NOT_FOUND", message = $"Device with ID {id} not found" });
        }
        catch (NotSupportedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new { code = "NOT_SUPPORTED", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { code = "DEVICE_UNREACHABLE", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting volume for {DeviceId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { code = "INTERNAL_ERROR", message = "Failed to get volume" });
        }
    }

    /// <summary>
    /// Sets the volume for a device.
    /// </summary>
    [HttpPost("{id}/volume")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> SetVolume(string id, [FromBody] SetVolumeRequest request, CancellationToken ct)
    {
        if (request.Level < 0 || request.Level > 100)
        {
            return BadRequest(new { code = "INVALID_INPUT", message = "Volume level must be between 0 and 100" });
        }

        try
        {
            await _deviceService.SetVolumeAsync(id, request.Level, ct);
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
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { code = "DEVICE_UNREACHABLE", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting volume for {DeviceId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { code = "INTERNAL_ERROR", message = "Failed to set volume" });
        }
    }

    /// <summary>
    /// Enters Bluetooth pairing mode for a device.
    /// </summary>
    [HttpPost("{id}/bluetooth/pairing")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> EnterBluetoothPairing(string id, CancellationToken ct)
    {
        try
        {
            await _deviceService.EnterPairingModeAsync(id, ct);
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
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { code = "DEVICE_UNREACHABLE", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error entering Bluetooth pairing for {DeviceId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { code = "INTERNAL_ERROR", message = "Failed to enter Bluetooth pairing mode" });
        }
    }

    /// <summary>
    /// Gets the presets for a device.
    /// </summary>
    [HttpGet("{id}/presets")]
    [ProducesResponseType(typeof(IEnumerable<Preset>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> GetPresets(string id, CancellationToken ct)
    {
        try
        {
            var presets = await _deviceService.ListPresetsAsync(id, ct);
            return Ok(presets);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { code = "DEVICE_NOT_FOUND", message = $"Device with ID {id} not found" });
        }
        catch (NotSupportedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new { code = "NOT_SUPPORTED", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { code = "DEVICE_UNREACHABLE", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting presets for {DeviceId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { code = "INTERNAL_ERROR", message = "Failed to get presets" });
        }
    }

    /// <summary>
    /// Plays a preset on a device.
    /// </summary>
    [HttpPost("{id}/presets/{presetId}/play")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> PlayPreset(string id, string presetId, CancellationToken ct)
    {
        try
        {
            await _deviceService.PlayPresetAsync(id, presetId, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { code = "DEVICE_NOT_FOUND", message = $"Device with ID {id} not found" });
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { code = "INVALID_INPUT", message = ex.Message });
        }
        catch (NotSupportedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new { code = "NOT_SUPPORTED", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { code = "DEVICE_UNREACHABLE", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error playing preset for {DeviceId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { code = "INTERNAL_ERROR", message = "Failed to play preset" });
        }
    }
}

public record AddDeviceRequest(string Name, string IpAddress, string Vendor);
public record UpdateDeviceRequest(string Name, string IpAddress, IEnumerable<string>? Capabilities);
public record SetPowerRequest(bool On);
public record SetVolumeRequest(int Level);
