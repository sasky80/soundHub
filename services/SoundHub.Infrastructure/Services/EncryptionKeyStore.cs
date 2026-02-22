using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SoundHub.Infrastructure.Services;

/// <summary>
/// SQLite-based encryption key store (key4.db NSS-style database).
/// Derives and persists the encryption key using master password from Docker secret.
/// </summary>
public class EncryptionKeyStore : IDisposable
{
    private const int KeySizeBytes = 32; // 256 bits for AES-256
    private const int SaltSizeBytes = 16;
    private const int Iterations = 100000;
    private const string KeyName = "master-encryption-key";

    private readonly string _keyDbPath;
    private readonly string _masterPassword;
    private readonly ILogger<EncryptionKeyStore> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private byte[]? _cachedKey;
    private bool _disposed;

    public EncryptionKeyStore(
        IOptions<EncryptionKeyStoreOptions> options,
        ILogger<EncryptionKeyStore> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        _keyDbPath = options.Value.KeyDbPath;
        _masterPassword = options.Value.GetEffectiveMasterPassword();
        _logger = logger;
        EnsureDatabaseExists();
    }

    /// <summary>
    /// Gets the encryption key, generating and storing it if not present.
    /// </summary>
    public async Task<byte[]> GetEncryptionKeyAsync(CancellationToken ct = default)
    {
        if (_cachedKey != null)
        {
            return _cachedKey;
        }

        await _lock.WaitAsync(ct);
        try
        {
            // Double-check after acquiring lock
            if (_cachedKey != null)
            {
                return _cachedKey;
            }

            var storedKey = await LoadKeyFromDatabaseAsync(ct);
            if (storedKey != null)
            {
                _cachedKey = storedKey;
                _logger.LogDebug("Encryption key loaded from key store");
                return _cachedKey;
            }

            // Generate new key and store it
            _cachedKey = GenerateNewKey();
            await StoreKeyInDatabaseAsync(_cachedKey, ct);
            _logger.LogInformation("New encryption key generated and stored");
            return _cachedKey;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Rotates the encryption key. The caller is responsible for re-encrypting data.
    /// </summary>
    public async Task<byte[]> RotateKeyAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            if (_cachedKey != null)
            {
                CryptographicOperations.ZeroMemory(_cachedKey);
            }

            var newKey = GenerateNewKey();
            await StoreKeyInDatabaseAsync(newKey, ct);
            _cachedKey = newKey;
            _logger.LogInformation("Encryption key rotated");
            return newKey;
        }
        finally
        {
            _lock.Release();
        }
    }

    private void EnsureDatabaseExists()
    {
        var directory = Path.GetDirectoryName(_keyDbPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var connection = CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS keys (
                name TEXT PRIMARY KEY,
                encrypted_key BLOB NOT NULL,
                salt BLOB NOT NULL,
                iterations INTEGER NOT NULL,
                created_at TEXT NOT NULL,
                updated_at TEXT NOT NULL
            )
            """;
        command.ExecuteNonQuery();
    }

    private SqliteConnection CreateConnection()
    {
        return new SqliteConnection($"Data Source={_keyDbPath}");
    }

    private async Task<byte[]?> LoadKeyFromDatabaseAsync(CancellationToken ct)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT encrypted_key, salt, iterations FROM keys WHERE name = @name";
        command.Parameters.AddWithValue("@name", KeyName);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        var encryptedKey = (byte[])reader["encrypted_key"];
        var salt = (byte[])reader["salt"];
        var iterations = reader.GetInt32(reader.GetOrdinal("iterations"));

        return DecryptKey(encryptedKey, salt, iterations);
    }

    private async Task StoreKeyInDatabaseAsync(byte[] key, CancellationToken ct)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var encryptedKey = EncryptKey(key, salt);
        var now = DateTime.UtcNow.ToString("O");

        await using var connection = CreateConnection();
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO keys (name, encrypted_key, salt, iterations, created_at, updated_at)
            VALUES (@name, @encrypted_key, @salt, @iterations, @created_at, @updated_at)
            ON CONFLICT(name) DO UPDATE SET
                encrypted_key = @encrypted_key,
                salt = @salt,
                iterations = @iterations,
                updated_at = @updated_at
            """;
        command.Parameters.AddWithValue("@name", KeyName);
        command.Parameters.AddWithValue("@encrypted_key", encryptedKey);
        command.Parameters.AddWithValue("@salt", salt);
        command.Parameters.AddWithValue("@iterations", Iterations);
        command.Parameters.AddWithValue("@created_at", now);
        command.Parameters.AddWithValue("@updated_at", now);

        await command.ExecuteNonQueryAsync(ct);
    }

    private static byte[] GenerateNewKey()
    {
        return RandomNumberGenerator.GetBytes(KeySizeBytes);
    }

    private byte[] EncryptKey(byte[] key, byte[] salt)
    {
        var derivedKey = DeriveKeyFromMasterPassword(salt);

        using var aes = Aes.Create();
        aes.Key = derivedKey;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();

        // Write IV first
        ms.Write(aes.IV, 0, aes.IV.Length);

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            cs.Write(key, 0, key.Length);
        }

        return ms.ToArray();
    }

    private byte[] DecryptKey(byte[] encryptedKey, byte[] salt, int iterations)
    {
        var derivedKey = DeriveKeyFromMasterPassword(salt, iterations);

        using var aes = Aes.Create();
        aes.Key = derivedKey;

        // Extract IV from the beginning
        var ivLength = aes.BlockSize / 8;
        var iv = new byte[ivLength];
        Array.Copy(encryptedKey, 0, iv, 0, ivLength);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(encryptedKey, ivLength, encryptedKey.Length - ivLength);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var resultMs = new MemoryStream();

        cs.CopyTo(resultMs);
        return resultMs.ToArray();
    }

    private byte[] DeriveKeyFromMasterPassword(byte[] salt, int? iterations = null)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            _masterPassword,
            salt,
            iterations ?? Iterations,
            HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(KeySizeBytes);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_cachedKey != null)
        {
            CryptographicOperations.ZeroMemory(_cachedKey);
            _cachedKey = null;
        }

        _lock.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Configuration options for the encryption key store.
/// </summary>
public class EncryptionKeyStoreOptions
{
    /// <summary>
    /// Path to the key4.db SQLite database file.
    /// </summary>
    public string KeyDbPath { get; set; } = "/data/key4.db";

    /// <summary>
    /// Master password used to encrypt/decrypt the stored encryption key.
    /// Can be provided directly or loaded from <see cref="MasterPasswordFile"/>.
    /// When both are set, the file takes precedence.
    /// </summary>
    public string MasterPassword { get; set; } = "default-dev-password";

    /// <summary>
    /// Path to a file containing the master password (e.g., Docker secret at /run/secrets/master_password).
    /// When set, the password is read from this file, trimming any trailing whitespace.
    /// </summary>
    public string? MasterPasswordFile { get; set; }

    /// <summary>
    /// Resolves the effective master password, preferring the file if specified and exists.
    /// </summary>
    public string GetEffectiveMasterPassword()
    {
        if (!string.IsNullOrEmpty(MasterPasswordFile) && File.Exists(MasterPasswordFile))
        {
            return File.ReadAllText(MasterPasswordFile).Trim();
        }

        return MasterPassword;
    }
}
