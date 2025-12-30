using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoundHub.Domain.Interfaces;

namespace SoundHub.Infrastructure.Services;

/// <summary>
/// Secrets service with AES-256-CBC encryption.
/// Stores encrypted secrets in secrets.json and encryption key in key4.db (SQLite).
/// </summary>
public class EncryptedSecretsService : ISecretsService
{
    private readonly string _secretsFilePath;
    private readonly ILogger<EncryptedSecretsService> _logger;
    private readonly byte[] _encryptionKey;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public EncryptedSecretsService(
        IOptions<SecretsServiceOptions> options,
        ILogger<EncryptedSecretsService> logger)
    {
        _secretsFilePath = options.Value.SecretsFilePath;
        _logger = logger;
        _encryptionKey = DeriveKeyFromMasterPassword(options.Value.MasterPassword);
        EnsureFileExists();
    }

    private void EnsureFileExists()
    {
        var directory = Path.GetDirectoryName(_secretsFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (!File.Exists(_secretsFilePath))
        {
            File.WriteAllText(_secretsFilePath, "[]");
        }
    }

    private static byte[] DeriveKeyFromMasterPassword(string masterPassword)
    {
        // Use PBKDF2 to derive a 256-bit key from the master password
        using var pbkdf2 = new Rfc2898DeriveBytes(
            masterPassword,
            Encoding.UTF8.GetBytes("SoundHub-Salt-V1"), // Fixed salt (in production, store per-installation)
            100000, // Iterations
            HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(32); // 256 bits
    }

    public async Task<string?> GetSecretAsync(string secretName, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var secrets = await LoadSecretsAsync(ct);
            var secret = secrets.FirstOrDefault(s => s.SecretName == secretName);
            if (secret == null)
            {
                return null;
            }

            return DecryptValue(secret.SecretValue);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SetSecretAsync(string secretName, string value, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var secrets = await LoadSecretsAsync(ct);
            var existingIndex = secrets.FindIndex(s => s.SecretName == secretName);

            var encryptedValue = EncryptValue(value);
            var secretEntry = new SecretEntry { SecretName = secretName, SecretValue = encryptedValue };

            if (existingIndex >= 0)
            {
                secrets[existingIndex] = secretEntry;
            }
            else
            {
                secrets.Add(secretEntry);
            }

            await SaveSecretsAsync(secrets, ct);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> DeleteSecretAsync(string secretName, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var secrets = await LoadSecretsAsync(ct);
            var removed = secrets.RemoveAll(s => s.SecretName == secretName) > 0;
            if (removed)
            {
                await SaveSecretsAsync(secrets, ct);
            }
            return removed;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<List<SecretEntry>> LoadSecretsAsync(CancellationToken ct)
    {
        try
        {
            var json = await File.ReadAllTextAsync(_secretsFilePath, ct);
            return JsonSerializer.Deserialize<List<SecretEntry>>(json) ?? new List<SecretEntry>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load secrets from {FilePath}", _secretsFilePath);
            return new List<SecretEntry>();
        }
    }

    private async Task SaveSecretsAsync(List<SecretEntry> secrets, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(secrets, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_secretsFilePath, json, ct);
    }

    private string EncryptValue(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        ms.Write(aes.IV, 0, aes.IV.Length); // Prepend IV to ciphertext

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var writer = new StreamWriter(cs))
        {
            writer.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    private string DecryptValue(string cipherText)
    {
        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;

        // Extract IV from the beginning of the ciphertext
        var iv = new byte[aes.BlockSize / 8];
        Array.Copy(fullCipher, 0, iv, 0, iv.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cs);
        return reader.ReadToEnd();
    }

    private class SecretEntry
    {
        public required string SecretName { get; init; }
        public required string SecretValue { get; init; }
    }
}

public class SecretsServiceOptions
{
    public string SecretsFilePath { get; set; } = "/data/secrets.json";
    public string MasterPassword { get; set; } = "default-dev-password"; // Override with Docker secret in production
}
