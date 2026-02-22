using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using SoundHub.Infrastructure.Services;

namespace SoundHub.Tests.Infrastructure;

public class EncryptedSecretsServiceTests : IDisposable
{
    private readonly string _secretsPath;
    private readonly string _keyDbPath;
    private readonly string _tempDir;
    private readonly EncryptionKeyStore _keyStore;
    private readonly EncryptedSecretsService _sut;

    public EncryptedSecretsServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "SoundHub.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);

        _secretsPath = Path.Combine(_tempDir, "secrets.json");
        _keyDbPath = Path.Combine(_tempDir, "key4.db");

        var secretsOptions = Options.Create(new SecretsServiceOptions { SecretsFilePath = _secretsPath });
        var keyOptions = Options.Create(new EncryptionKeyStoreOptions
        {
            KeyDbPath = _keyDbPath,
            MasterPassword = "test-master-password"
        });

        _keyStore = new EncryptionKeyStore(keyOptions, Substitute.For<ILogger<EncryptionKeyStore>>());
        _sut = new EncryptedSecretsService(secretsOptions, _keyStore, Substitute.For<ILogger<EncryptedSecretsService>>());
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public async Task SetSecretAsync_StoresV1Envelope_AndGetSecretAsyncRoundTrips()
    {
        await _sut.SetSecretAsync("wifi", "super-secret");

        var json = await File.ReadAllTextAsync(_secretsPath);
        Assert.Contains("\"SecretName\": \"wifi\"", json);
        Assert.Contains("\"SecretValue\": \"v1:", json);

        var roundTrip = await _sut.GetSecretAsync("wifi");
        Assert.Equal("super-secret", roundTrip);
    }

    [Fact]
    public async Task GetSecretAsync_WhenCiphertextIsTampered_ThrowsCryptographicException()
    {
        await _sut.SetSecretAsync("wifi", "super-secret");

        var jsonText = await File.ReadAllTextAsync(_secretsPath);
        var doc = JsonDocument.Parse(jsonText);
        var entry = doc.RootElement.EnumerateArray().Single(e => e.GetProperty("SecretName").GetString() == "wifi");
        var secretValue = entry.GetProperty("SecretValue").GetString();
        Assert.NotNull(secretValue);
        Assert.StartsWith("v1:", secretValue!, StringComparison.Ordinal);

        var payload = Convert.FromBase64String(secretValue![3..]);
        payload[^1] ^= 0xFF; // flip a bit
        var tampered = "v1:" + Convert.ToBase64String(payload);

        var secrets = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(jsonText)
            ?? throw new InvalidOperationException("Expected secrets JSON array.");
        var wifiSecret = secrets.Single(s => s["SecretName"] == "wifi");
        wifiSecret["SecretValue"] = tampered;

        var tamperedJson = JsonSerializer.Serialize(secrets, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        await File.WriteAllTextAsync(_secretsPath, tamperedJson);

        await Assert.ThrowsAnyAsync<CryptographicException>(() => _sut.GetSecretAsync("wifi"));
    }

    [Fact]
    public async Task GetSecretAsync_WhenLegacyCiphertext_DecryptsAndMigratesToV1()
    {
        var key = await _keyStore.GetEncryptionKeyAsync();

        var legacyCipher = EncryptLegacyAesCbc(key, "super-secret");
        var legacyJson = "[" +
            "{\"SecretName\":\"wifi\",\"SecretValue\":\"" + legacyCipher + "\"}" +
            "]";
        await File.WriteAllTextAsync(_secretsPath, legacyJson);

        var plainText = await _sut.GetSecretAsync("wifi");
        Assert.Equal("super-secret", plainText);

        var migratedJson = await File.ReadAllTextAsync(_secretsPath);
        Assert.Contains("\"SecretValue\": \"v1:", migratedJson);
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
