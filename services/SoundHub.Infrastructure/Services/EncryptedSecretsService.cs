using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoundHub.Domain.Interfaces;

namespace SoundHub.Infrastructure.Services;

/// <summary>
/// Secrets service with AES-256-CBC encryption.
/// Stores encrypted secrets in secrets.json and uses EncryptionKeyStore for key management.
/// </summary>
public class EncryptedSecretsService : ISecretsService, IDisposable
{
    private const string AeadEnvelopePrefix = "v1:";
    private const int AesGcmNonceSizeBytes = 12;
    private const int AesGcmTagSizeBytes = 16;

    private readonly string _secretsFilePath;
    private readonly ILogger<EncryptedSecretsService> _logger;
    private readonly EncryptionKeyStore _keyStore;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private byte[]? _encryptionKey;
    private bool _disposed;

    public EncryptedSecretsService(
        IOptions<SecretsServiceOptions> options,
        EncryptionKeyStore keyStore,
        ILogger<EncryptedSecretsService> logger)
    {
        _secretsFilePath = options.Value.SecretsFilePath;
        _keyStore = keyStore;
        _logger = logger;
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

    private async Task<byte[]> GetEncryptionKeyAsync(CancellationToken ct)
    {
        if (_encryptionKey != null)
        {
            return _encryptionKey;
        }

        _encryptionKey = await _keyStore.GetEncryptionKeyAsync(ct);
        return _encryptionKey;
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

            var (plainText, wasLegacy) = await DecryptValueAsync(secret.SecretValue, ct).ConfigureAwait(false);
            if (wasLegacy)
            {
                var legacyIndex = secrets.FindIndex(s => s.SecretName == secretName);
                if (legacyIndex >= 0)
                {
                    secrets[legacyIndex] = new SecretEntry
                    {
                        SecretName = secretName,
                        SecretValue = await EncryptValueAsync(plainText, ct).ConfigureAwait(false)
                    };

                    await SaveSecretsAsync(secrets, ct).ConfigureAwait(false);
                }
            }

            return plainText;
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

            var encryptedValue = await EncryptValueAsync(value, ct);
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

    private async Task<string> EncryptValueAsync(string plainText, CancellationToken ct)
    {
        var key = await GetEncryptionKeyAsync(ct);

        var nonce = RandomNumberGenerator.GetBytes(AesGcmNonceSizeBytes);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[AesGcmTagSizeBytes];

        using (var aesGcm = new AesGcm(key, AesGcmTagSizeBytes))
        {
            aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);
        }

        var payload = new byte[nonce.Length + tag.Length + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, payload, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, payload, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipherBytes, 0, payload, nonce.Length + tag.Length, cipherBytes.Length);

        return AeadEnvelopePrefix + Convert.ToBase64String(payload);
    }

    private async Task<(string PlainText, bool WasLegacy)> DecryptValueAsync(string cipherText, CancellationToken ct)
    {
        var key = await GetEncryptionKeyAsync(ct);

        if (cipherText.StartsWith(AeadEnvelopePrefix, StringComparison.Ordinal))
        {
            var payloadBase64 = cipherText[AeadEnvelopePrefix.Length..];
            byte[] payload;
            try
            {
                payload = Convert.FromBase64String(payloadBase64);
            }
            catch (FormatException ex)
            {
                throw new CryptographicException("Invalid encrypted secret format (base64).", ex);
            }

            var minLength = AesGcmNonceSizeBytes + AesGcmTagSizeBytes;
            if (payload.Length < minLength)
            {
                throw new CryptographicException("Invalid encrypted secret format (payload too short).");
            }

            var nonce = payload.AsSpan(0, AesGcmNonceSizeBytes).ToArray();
            var tag = payload.AsSpan(AesGcmNonceSizeBytes, AesGcmTagSizeBytes).ToArray();
            var cipherBytes = payload.AsSpan(minLength).ToArray();
            var plainBytes = new byte[cipherBytes.Length];

            using (var aesGcm = new AesGcm(key, AesGcmTagSizeBytes))
            {
                aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);
            }

            return (Encoding.UTF8.GetString(plainBytes), WasLegacy: false);
        }

        // Legacy format (AES-CBC): base64( IV | ciphertext )
        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = key;

        var iv = new byte[aes.BlockSize / 8];
        if (fullCipher.Length < iv.Length)
        {
            throw new CryptographicException("Invalid legacy encrypted secret format.");
        }

        Array.Copy(fullCipher, 0, iv, 0, iv.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cs, Encoding.UTF8);
        var legacyPlainText = await reader.ReadToEndAsync(ct).ConfigureAwait(false);
        return (legacyPlainText, WasLegacy: true);
    }

    private class SecretEntry
    {
        public required string SecretName { get; init; }
        public required string SecretValue { get; init; }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_encryptionKey != null)
        {
            CryptographicOperations.ZeroMemory(_encryptionKey);
            _encryptionKey = null;
        }

        _lock.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

public class SecretsServiceOptions
{
    public string SecretsFilePath { get; set; } = "/data/secrets.json";
}
