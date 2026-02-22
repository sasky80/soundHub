using Microsoft.AspNetCore.Mvc;
using SoundHub.Domain.Interfaces;

namespace SoundHub.Api.Controllers;

/// <summary>
/// Serves locally-stored station JSON files for LOCAL_INTERNET_RADIO presets.
/// </summary>
[ApiController]
[Route("api/presets")]
public class PresetsController : ControllerBase
{
    private readonly IStationFileService _stationFileService;

    public PresetsController(IStationFileService stationFileService)
    {
        _stationFileService = stationFileService;
    }

    /// <summary>
    /// Returns a station JSON file by filename.
    /// </summary>
    [HttpGet("{filename}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStationFile(string filename, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(filename)
            || filename.Contains("..", StringComparison.Ordinal)
            || filename.Contains('/')
            || filename.Contains('\\')
            || filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            return BadRequest(new { code = "INVALID_INPUT", message = "Invalid station filename" });
        }

        var content = await _stationFileService.ReadAsync(filename, ct);
        if (content == null)
        {
            return NotFound(new { code = "STATION_NOT_FOUND", message = $"Station file '{filename}' not found" });
        }

        return Content(content, "application/json");
    }
}
