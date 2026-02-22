using Microsoft.AspNetCore.Mvc;
using SoundHub.Application.Services;
using SoundHub.Domain.Entities;

namespace SoundHub.Api.Controllers;

/// <summary>
/// Configuration endpoints for device management settings.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private const int MaxNetworkMaskLength = 18;

    private readonly DeviceService _deviceService;

    public ConfigController(DeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    /// <summary>
    /// Gets the configured network mask for device discovery.
    /// </summary>
    [HttpGet("network-mask")]
    [ProducesResponseType(typeof(NetworkMaskResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNetworkMask(CancellationToken ct)
    {
        var networkMask = await _deviceService.GetNetworkMaskAsync(ct);
        return Ok(new NetworkMaskResponse(networkMask));
    }

    /// <summary>
    /// Sets the network mask for device discovery.
    /// </summary>
    [HttpPut("network-mask")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetNetworkMask([FromBody] SetNetworkMaskRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.NetworkMask))
        {
            return BadRequest(new { code = "INVALID_INPUT", message = "NetworkMask is required" });
        }

        if (request.NetworkMask.Length > MaxNetworkMaskLength)
        {
            return BadRequest(new { code = "INVALID_INPUT", message = $"NetworkMask must be {MaxNetworkMaskLength} characters or fewer" });
        }

        try
        {
            await _deviceService.SetNetworkMaskAsync(request.NetworkMask, ct);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { code = "INVALID_INPUT", message = ex.Message });
        }
    }
}

/// <summary>
/// Vendor endpoints for listing supported device vendors.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class VendorsController : ControllerBase
{
    private readonly DeviceService _deviceService;

    public VendorsController(DeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    /// <summary>
    /// Gets the list of supported device vendors.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VendorInfo>), StatusCodes.Status200OK)]
    public IActionResult GetVendors()
    {
        var vendors = _deviceService.GetVendors();
        return Ok(vendors);
    }
}

public record NetworkMaskResponse(string? NetworkMask);
public record SetNetworkMaskRequest(string NetworkMask);
