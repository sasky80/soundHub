using SoundHub.Infrastructure.Persistence;

namespace SoundHub.Tests.Infrastructure;

/// <summary>
/// Tests for FileDeviceRepositoryOptions configuration.
/// </summary>
public class FileDeviceRepositoryOptionsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new FileDeviceRepositoryOptions();

        // Assert
        Assert.Equal("/data/devices.json", options.FilePath);
        Assert.True(options.EnableHotReload);
    }

    [Fact]
    public void FilePath_CanBeSet()
    {
        // Arrange
        var options = new FileDeviceRepositoryOptions();

        // Act
        options.FilePath = "/custom/path/devices.json";

        // Assert
        Assert.Equal("/custom/path/devices.json", options.FilePath);
    }

    [Fact]
    public void EnableHotReload_CanBeDisabled()
    {
        // Arrange
        var options = new FileDeviceRepositoryOptions();

        // Act
        options.EnableHotReload = false;

        // Assert
        Assert.False(options.EnableHotReload);
    }
}
