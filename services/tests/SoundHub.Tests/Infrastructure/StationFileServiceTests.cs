using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SoundHub.Infrastructure.Persistence;

namespace SoundHub.Tests.Infrastructure;

public class StationFileServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly StationFileService _service;

    public StationFileServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"soundhub-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        var options = Options.Create(new StationFileServiceOptions
        {
            PresetsDirectory = _tempDir,
            PublicHostUrl = "http://mini.local/soundhub",
        });

        _service = new StationFileService(options, NullLogger<StationFileService>.Instance);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    // --- Slugify tests ---

    [Theory]
    [InlineData("Jazz FM", "jazz-fm")]
    [InlineData("Jazz FM 91.1", "jazz-fm-91-1")]
    [InlineData("Rock & Roll!", "rock-roll")]
    [InlineData("  spaces  ", "spaces")]
    [InlineData("UPPER CASE", "upper-case")]
    [InlineData("already-slug", "already-slug")]
    [InlineData("dots.and.more", "dots-and-more")]
    [InlineData("special!@#$chars", "special-chars")]
    [InlineData("múltiple àccéntèd", "m-ltiple-cc-nt-d")]
    [InlineData("123 numbers", "123-numbers")]
    public void Slugify_GeneratesExpectedSlug(string name, string expectedSlug)
    {
        var slug = _service.Slugify(name);
        Assert.Equal(expectedSlug, slug);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Slugify_ThrowsForEmptyName(string? name)
    {
        Assert.Throws<ArgumentException>(() => _service.Slugify(name!));
    }

    [Fact]
    public void Slugify_ThrowsForNonAlphanumericOnlyName()
    {
        Assert.Throws<ArgumentException>(() => _service.Slugify("!!!"));
    }

    // --- CreateAsync tests ---

    [Fact]
    public async Task CreateAsync_WritesFileAndReturnsSlug()
    {
        var slug = await _service.CreateAsync("My Station", "http://stream.example.com/radio");

        Assert.Equal("my-station", slug);
        Assert.True(File.Exists(Path.Combine(_tempDir, "my-station.json")));

        var content = await File.ReadAllTextAsync(Path.Combine(_tempDir, "my-station.json"));
        Assert.Contains("\"streamUrl\": \"http://stream.example.com/radio\"", content);
        Assert.Contains("\"name\": \"My Station\"", content);
        Assert.Contains("\"streamType\": \"liveRadio\"", content);
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenFileAlreadyExists()
    {
        await _service.CreateAsync("Duplicate Station", "http://stream1.example.com");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAsync("Duplicate Station", "http://stream2.example.com"));

        Assert.Contains("already exists", ex.Message);
    }

    // --- UpdateAsync tests ---

    [Fact]
    public async Task UpdateAsync_OverwritesExistingFile()
    {
        var slug = await _service.CreateAsync("Update Me", "http://old.example.com");

        await _service.UpdateAsync(slug, "Update Me", "http://new.example.com");

        var content = await File.ReadAllTextAsync(Path.Combine(_tempDir, $"{slug}.json"));
        Assert.Contains("\"streamUrl\": \"http://new.example.com\"", content);
        Assert.DoesNotContain("http://old.example.com", content);
    }

    // --- ReadAsync tests ---

    [Fact]
    public async Task ReadAsync_ReturnsContentForExistingFile()
    {
        await _service.CreateAsync("Readable", "http://stream.example.com");

        var content = await _service.ReadAsync("readable.json");

        Assert.NotNull(content);
        Assert.Contains("\"streamUrl\": \"http://stream.example.com\"", content);
    }

    [Fact]
    public async Task ReadAsync_ReturnsNullForMissingFile()
    {
        var content = await _service.ReadAsync("nonexistent.json");
        Assert.Null(content);
    }

    // --- Exists tests ---

    [Fact]
    public async Task Exists_ReturnsTrueForExistingFile()
    {
        await _service.CreateAsync("Exists Test", "http://stream.example.com");
        Assert.True(_service.Exists("exists-test"));
    }

    [Fact]
    public void Exists_ReturnsFalseForMissingFile()
    {
        Assert.False(_service.Exists("nope"));
    }

    // --- GetPublicUrl tests ---

    [Fact]
    public void GetPublicUrl_ReturnsCorrectUrl()
    {
        var url = _service.GetPublicUrl("jazz-fm");
        Assert.Equal("http://mini.local/soundhub/presets/jazz-fm.json", url);
    }

    [Fact]
    public void GetPublicUrl_TrimsTrailingSlashFromHost()
    {
        var options = Options.Create(new StationFileServiceOptions
        {
            PresetsDirectory = _tempDir,
            PublicHostUrl = "http://host.local/soundhub/",
        });
        var service = new StationFileService(options, NullLogger<StationFileService>.Instance);

        var url = service.GetPublicUrl("test");
        Assert.Equal("http://host.local/soundhub/presets/test.json", url);
    }
}
