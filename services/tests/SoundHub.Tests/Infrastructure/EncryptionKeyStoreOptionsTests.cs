using SoundHub.Infrastructure.Services;

namespace SoundHub.Tests.Infrastructure;

/// <summary>
/// Tests for EncryptionKeyStoreOptions configuration.
/// </summary>
public class EncryptionKeyStoreOptionsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new EncryptionKeyStoreOptions();

        // Assert
        Assert.Equal("/data/key4.db", options.KeyDbPath);
        Assert.Equal("default-dev-password", options.MasterPassword);
        Assert.Null(options.MasterPasswordFile);
    }

    [Fact]
    public void GetEffectiveMasterPassword_WhenNoFile_ReturnsMasterPassword()
    {
        // Arrange
        var options = new EncryptionKeyStoreOptions
        {
            MasterPassword = "test-password"
        };

        // Act
        var result = options.GetEffectiveMasterPassword();

        // Assert
        Assert.Equal("test-password", result);
    }

    [Fact]
    public void GetEffectiveMasterPassword_WhenFileDoesNotExist_ReturnsMasterPassword()
    {
        // Arrange
        var options = new EncryptionKeyStoreOptions
        {
            MasterPassword = "fallback-password",
            MasterPasswordFile = "/nonexistent/path/secret.txt"
        };

        // Act
        var result = options.GetEffectiveMasterPassword();

        // Assert
        Assert.Equal("fallback-password", result);
    }

    [Fact]
    public void GetEffectiveMasterPassword_WhenFileExists_ReadsFromFile()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "file-based-password\n");
            var options = new EncryptionKeyStoreOptions
            {
                MasterPassword = "fallback-password",
                MasterPasswordFile = tempFile
            };

            // Act
            var result = options.GetEffectiveMasterPassword();

            // Assert
            Assert.Equal("file-based-password", result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void GetEffectiveMasterPassword_TrimsWhitespace()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "  password-with-spaces  \r\n");
            var options = new EncryptionKeyStoreOptions
            {
                MasterPasswordFile = tempFile
            };

            // Act
            var result = options.GetEffectiveMasterPassword();

            // Assert
            Assert.Equal("password-with-spaces", result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
