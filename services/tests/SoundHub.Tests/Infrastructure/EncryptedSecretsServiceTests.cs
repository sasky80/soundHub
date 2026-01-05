using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using SoundHub.Infrastructure.Services;

namespace SoundHub.Tests.Infrastructure;

public class EncryptedSecretsServiceTests
{
    [Fact]
    public async Task SetSecretAsync_StoresV1Envelope_AndGetSecretAsyncRoundTrips()
    {
        var (secretsPath, keyDbPath) = CreateTempPaths();

        var secretsOptions = Options.Create(new SecretsServiceOptions { SecretsFilePath = secretsPath });
        var keyOptions = Options.Create(new EncryptionKeyStoreOptions
        {
            KeyDbPath = keyDbPath,
            MasterPassword = "test-master-password"
        });

        var logger = Substitute.For<ILogger<EncryptedSecretsService>>();
        var keyLogger = Substitute.For<ILogger<EncryptionKeyStore>>();

        var keyStore = new EncryptionKeyStore(keyOptions, keyLogger);
        var sut = new EncryptedSecretsService(secretsOptions, keyStore, logger);

        await sut.SetSecretAsync("wifi", "super-secret");

        var json = await File.ReadAllTextAsync(secretsPath);
        Assert.Contains("\"SecretName\": \"wifi\"", json);
        Assert.Contains("\"SecretValue\": \"v1:", json);

        var roundTrip = await sut.GetSecretAsync("wifi");
        Assert.Equal("super-secret", roundTrip);
    }

    [Fact]
    public async Task GetSecretAsync_WhenCiphertextIsTampered_ThrowsCryptographicException()
    {
        var (secretsPath, keyDbPath) = CreateTempPaths();

        var secretsOptions = Options.Create(new SecretsServiceOptions { SecretsFilePath = secretsPath });
        var keyOptions = Options.Create(new EncryptionKeyStoreOptions
        {
            KeyDbPath = keyDbPath,
            MasterPassword = "test-master-password"
        });

        var logger = Substitute.For<ILogger<EncryptedSecretsService>>();
        var keyLogger = Substitute.For<ILogger<EncryptionKeyStore>>();

        var keyStore = new EncryptionKeyStore(keyOptions, keyLogger);
        var sut = new EncryptedSecretsService(secretsOptions, keyStore, logger);

        await sut.SetSecretAsync("wifi", "super-secret");

        var jsonText = await File.ReadAllTextAsync(secretsPath);
        var doc = JsonDocument.Parse(jsonText);
        var entry = doc.RootElement.EnumerateArray().Single(e => e.GetProperty("SecretName").GetString() == "wifi");
        var secretValue = entry.GetProperty("SecretValue").GetString();
        Assert.NotNull(secretValue);
        Assert.StartsWith("v1:", secretValue!, StringComparison.Ordinal);

        var payload = Convert.FromBase64String(secretValue![3..]);
        payload[^1] ^= 0xFF; // flip a bit
        var tampered = "v1:" + Convert.ToBase64String(payload);

        var tamperedJson = jsonText.Replace(secretValue!, tampered, StringComparison.Ordinal);
        await File.WriteAllTextAsync(secretsPath, tamperedJson);

        await Assert.ThrowsAnyAsync<CryptographicException>(() => sut.GetSecretAsync("wifi"));
    }

    [Fact]
    public async Task GetSecretAsync_WhenLegacyCiphertext_DecryptsAndMigratesToV1()
    {
        var (secretsPath, keyDbPath) = CreateTempPaths();

        var secretsOptions = Options.Create(new SecretsServiceOptions { SecretsFilePath = secretsPath });
        var keyOptions = Options.Create(new EncryptionKeyStoreOptions
        {
            KeyDbPath = keyDbPath,
            MasterPassword = "test-master-password"
        });

        var logger = Substitute.For<ILogger<EncryptedSecretsService>>();
        var keyLogger = Substitute.For<ILogger<EncryptionKeyStore>>();

        var keyStore = new EncryptionKeyStore(keyOptions, keyLogger);
        var key = await keyStore.GetEncryptionKeyAsync();

        var legacyCipher = EncryptLegacyAesCbc(key, "super-secret");
        var legacyJson = "[" +
            "{\"SecretName\":\"wifi\",\"SecretValue\":\"" + legacyCipher + "\"}" +
            "]";
        await File.WriteAllTextAsync(secretsPath, legacyJson);

        var sut = new EncryptedSecretsService(secretsOptions, keyStore, logger);

        var plainText = await sut.GetSecretAsync("wifi");
        Assert.Equal("super-secret", plainText);

        var migratedJson = await File.ReadAllTextAsync(secretsPath);
        Assert.Contains("\"SecretValue\": \"v1:", migratedJson);
    }

    private static (string SecretsPath, string KeyDbPath) CreateTempPaths()
    {
        var root = Path.Combine(Path.GetTempPath(), "SoundHub.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var secretsPath = Path.Combine(root, "secrets.json");
        var keyDbPath = Path.Combine(root, "key4.db");
        return (secretsPath, keyDbPath);
    }

    private static string EncryptLegacyAesCbc(byte[] key, string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        ms.Write(aes.IV, 0, aes.IV.Length);

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var writer = new StreamWriter(cs))
        {
            writer.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }
}
