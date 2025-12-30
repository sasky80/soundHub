namespace SoundHub.Domain.Interfaces;

/// <summary>
/// Service for managing encrypted secrets (e.g., account passwords).
/// </summary>
public interface ISecretsService
{
    Task<string?> GetSecretAsync(string secretName, CancellationToken ct = default);
    Task SetSecretAsync(string secretName, string value, CancellationToken ct = default);
    Task<bool> DeleteSecretAsync(string secretName, CancellationToken ct = default);
}
