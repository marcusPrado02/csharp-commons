using System.Security.Cryptography;
using System.Text;

namespace MarcusPrado.Platform.Configuration;

/// <summary>
/// Provides AES-256 CBC encryption and decryption for configuration values.
/// Encrypted values are wrapped in the <c>ENC(...)</c> envelope.
/// </summary>
public static class ConfigCipherTool
{
    private const int IvSize = 16;   // 128-bit block size

    /// <summary>
    /// Encrypts the given plain text using AES-256 CBC and returns it wrapped as <c>ENC(&lt;base64&gt;)</c>.
    /// </summary>
    /// <param name="plainText">The plain text value to encrypt.</param>
    /// <param name="key">The encryption key (any length; will be normalized to 32 bytes via SHA-256).</param>
    /// <returns>The encrypted value in the form <c>ENC(base64EncodedData)</c>.</returns>
    public static string Encrypt(string plainText, string key)
    {
        ArgumentNullException.ThrowIfNull(plainText);
        ArgumentNullException.ThrowIfNull(key);

        var keyBytes = DeriveKey(key);

        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = keyBytes;
        aes.GenerateIV();

        var iv = aes.IV;
        var plainBytes = Encoding.UTF8.GetBytes(plainText);

        using var encryptor = aes.CreateEncryptor();
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Prepend IV to cipher bytes for storage
        var combined = new byte[IvSize + cipherBytes.Length];
        Buffer.BlockCopy(iv, 0, combined, 0, IvSize);
        Buffer.BlockCopy(cipherBytes, 0, combined, IvSize, cipherBytes.Length);

        return $"ENC({Convert.ToBase64String(combined)})";
    }

    /// <summary>
    /// Decrypts a value previously encrypted by <see cref="Encrypt"/>.
    /// The input must be in the form <c>ENC(base64EncodedData)</c>.
    /// </summary>
    /// <param name="cipherText">The cipher text in the form <c>ENC(base64EncodedData)</c>.</param>
    /// <param name="key">The decryption key (must match the key used for encryption).</param>
    /// <returns>The decrypted plain text.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="cipherText"/> is not in the expected format.</exception>
    public static string Decrypt(string cipherText, string key)
    {
        ArgumentNullException.ThrowIfNull(cipherText);
        ArgumentNullException.ThrowIfNull(key);

        if (!cipherText.StartsWith("ENC(", StringComparison.Ordinal) || !cipherText.EndsWith(')'))
            throw new ArgumentException($"Value is not in ENC(...) format: {cipherText}", nameof(cipherText));

        var base64 = cipherText[4..^1];
        var combined = Convert.FromBase64String(base64);

        if (combined.Length < IvSize)
            throw new ArgumentException("Cipher data is too short to contain an IV.", nameof(cipherText));

        var iv = combined[..IvSize];
        var cipherBytes = combined[IvSize..];
        var keyBytes = DeriveKey(key);

        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = keyBytes;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }

    /// <summary>
    /// Derives a 32-byte key from the provided key string using SHA-256.
    /// </summary>
    private static byte[] DeriveKey(string key)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(key));
    }
}
