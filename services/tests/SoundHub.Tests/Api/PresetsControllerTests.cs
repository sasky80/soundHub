using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using SoundHub.Api.Controllers;
using SoundHub.Domain.Interfaces;

namespace SoundHub.Tests.Api;

/// <summary>
/// Unit tests for <see cref="PresetsController"/> (station file serving endpoint).
/// </summary>
public class PresetsControllerTests
{
    private readonly IStationFileService _stationFileService;
    private readonly PresetsController _controller;

    public PresetsControllerTests()
    {
        _stationFileService = Substitute.For<IStationFileService>();
        _controller = new PresetsController(_stationFileService);
    }

    [Fact]
    public async Task GetStationFile_FileExists_ReturnsJsonContent()
    {
        // Arrange
        var json = """{"name":"Jazz FM","streamType":"radio","audio":{"hasPlaylist":false,"isRealtime":true,"streamUrl":"http://jazz.stream/live"}}""";
        _stationFileService.ReadAsync("jazz-fm.json", Arg.Any<CancellationToken>()).Returns(json);

        // Act
        var result = await _controller.GetStationFile("jazz-fm.json", CancellationToken.None);

        // Assert
        var contentResult = Assert.IsType<ContentResult>(result);
        Assert.Equal("application/json", contentResult.ContentType);
        Assert.Equal(json, contentResult.Content);
    }

    [Fact]
    public async Task GetStationFile_FileNotFound_Returns404()
    {
        // Arrange
        _stationFileService.ReadAsync("missing.json", Arg.Any<CancellationToken>()).Returns((string?)null);

        // Act
        var result = await _controller.GetStationFile("missing.json", CancellationToken.None);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFound.Value);
    }

    [Theory]
    [InlineData("../secret.json")]
    [InlineData("..\\secret.json")]
    [InlineData("folder/secret.json")]
    [InlineData("folder\\secret.json")]
    public async Task GetStationFile_PathTraversalAttempt_ReturnsBadRequest(string filename)
    {
        // Act
        var result = await _controller.GetStationFile(filename, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        await _stationFileService.DidNotReceive().ReadAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
