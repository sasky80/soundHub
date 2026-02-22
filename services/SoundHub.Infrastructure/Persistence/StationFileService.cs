using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoundHub.Domain.Entities;
using SoundHub.Domain.Interfaces;

namespace SoundHub.Infrastructure.Persistence;

public class StationFileServiceOptions
{
    /// <summary>
    /// Directory where station JSON files are stored.
    /// </summary>
    public string PresetsDirectory { get; set; } = "/data/presets";

    /// <summary>
    /// Public base URL used to construct the location URL for presets.
    /// Example: "http://mini.local/soundhub"
    /// </summary>
    public string PublicHostUrl { get; set; } = "http://localhost:5001";
}

public partial class StationFileService : IStationFileService
{
    private readonly StationFileServiceOptions _options;
    private readonly ILogger<StationFileService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public StationFileService(IOptions<StationFileServiceOptions> options, ILogger<StationFileService> logger)
    {
        _options = options.Value;
        _logger = logger;
        EnsureDirectoryExists();
    }

    public async Task<string> CreateAsync(string name, string streamUrl, CancellationToken ct = default)
    {
        var slug = Slugify(name);

        if (Exists(slug))
        {
            throw new InvalidOperationException(
                $"A station file with the name '{name}' (slug: {slug}) already exists. Choose a different station name.");
        }

        var stationFile = BuildStationFile(name, streamUrl);
        var filePath = GetFilePath(slug);

        var json = JsonSerializer.Serialize(stationFile, JsonOptions);
        await File.WriteAllTextAsync(filePath, json, ct);

        _logger.LogInformation("Created station file {Slug}.json for '{Name}'", slug, name);
        return slug;
    }

    public async Task UpdateAsync(string slug, string name, string streamUrl, CancellationToken ct = default)
    {
        var stationFile = BuildStationFile(name, streamUrl);
        var filePath = GetFilePath(slug);

        var json = JsonSerializer.Serialize(stationFile, JsonOptions);
        await File.WriteAllTextAsync(filePath, json, ct);

        _logger.LogInformation("Updated station file {Slug}.json for '{Name}'", slug, name);
    }

    public async Task<string?> ReadAsync(string filename, CancellationToken ct = default)
    {
        // Strip .json extension if present to get the slug, then re-add it
        var slug = filename.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
            ? filename[..^5]
            : filename;

        var filePath = GetFilePath(slug);

        if (!File.Exists(filePath))
        {
            return null;
        }

        return await File.ReadAllTextAsync(filePath, ct);
    }

    public bool Exists(string slug)
    {
        return File.Exists(GetFilePath(slug));
    }

    public string GetPublicUrl(string slug)
    {
        var baseUrl = _options.PublicHostUrl.TrimEnd('/');
        return $"{baseUrl}/presets/{slug}.json";
    }

    public string Slugify(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Station name cannot be empty", nameof(name));
        }

        // Lowercase
        var slug = name.ToLowerInvariant();

        // Replace non-alphanumeric characters with hyphens
        slug = NonAlphanumericRegex().Replace(slug, "-");

        // Collapse consecutive hyphens
        slug = ConsecutiveHyphensRegex().Replace(slug, "-");

        // Trim hyphens from start/end
        slug = slug.Trim('-');

        if (string.IsNullOrEmpty(slug))
        {
            throw new ArgumentException("Station name must contain at least one alphanumeric character", nameof(name));
        }

        return slug;
    }

    private static StationFile BuildStationFile(string name, string streamUrl)
    {
        return new StationFile
        {
            Name = name,
            StreamType = "liveRadio",
            Audio = new StationAudio
            {
                HasPlaylist = false,
                IsRealtime = true,
                StreamUrl = streamUrl,
            },
        };
    }

    private string GetFilePath(string slug) => Path.Combine(_options.PresetsDirectory, $"{slug}.json");

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_options.PresetsDirectory))
        {
            Directory.CreateDirectory(_options.PresetsDirectory);
            _logger.LogInformation("Created presets directory at {Path}", _options.PresetsDirectory);
        }
    }

    [GeneratedRegex("[^a-z0-9]")]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex("-{2,}")]
    private static partial Regex ConsecutiveHyphensRegex();
}
