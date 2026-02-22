namespace SoundHub.Domain.Interfaces;

/// <summary>
/// Service for managing local internet radio station JSON files.
/// Station files are stored under a configured directory and served at a public URL
/// so SoundTouch devices can fetch them during playback.
/// </summary>
public interface IStationFileService
{
    /// <summary>
    /// Creates a new station JSON file. Fails if a file with the same slug already exists.
    /// </summary>
    /// <param name="name">Station name (used to derive the filename slug).</param>
    /// <param name="streamUrl">HTTP stream URL for the station.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The slug (filename without extension) of the created file.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a file with the same slug already exists.</exception>
    Task<string> CreateAsync(string name, string streamUrl, CancellationToken ct = default);

    /// <summary>
    /// Updates (overwrites) an existing station JSON file.
    /// </summary>
    /// <param name="slug">The filename slug to update.</param>
    /// <param name="name">Station name.</param>
    /// <param name="streamUrl">HTTP stream URL for the station.</param>
    /// <param name="ct">Cancellation token.</param>
    Task UpdateAsync(string slug, string name, string streamUrl, CancellationToken ct = default);

    /// <summary>
    /// Reads a station JSON file and returns its raw content.
    /// </summary>
    /// <param name="filename">The filename (e.g., "jazz-fm.json").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The raw JSON content, or null if the file does not exist.</returns>
    Task<string?> ReadAsync(string filename, CancellationToken ct = default);

    /// <summary>
    /// Checks whether a station file with the given slug exists.
    /// </summary>
    /// <param name="slug">The filename slug (without .json extension).</param>
    /// <returns>True if the file exists.</returns>
    bool Exists(string slug);

    /// <summary>
    /// Returns the public URL for a station file.
    /// </summary>
    /// <param name="slug">The filename slug (without .json extension).</param>
    /// <returns>The full public URL.</returns>
    string GetPublicUrl(string slug);

    /// <summary>
    /// Generates a slug from a station name.
    /// </summary>
    /// <param name="name">The station name.</param>
    /// <returns>A URL-safe slug.</returns>
    string Slugify(string name);
}
