using System.Security.Cryptography;

namespace MarcusPrado.Platform.Security.Encryption;

public sealed class AesGcmEncryption : IDataEncryption, IDisposable
{
    // AES-256-GCM: 32-byte key, 12-byte nonce, 16-byte tag
    private const int KeySize = 32;
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private readonly byte[] _key;

    public AesGcmEncryption(byte[] key)
    {
        if (key.Length != KeySize)
            throw new ArgumentException($"Key must be {KeySize} bytes.", nameof(key));
        _key = key;
    }

    public string Encrypt(string plaintext)
    {
        ArgumentNullException.ThrowIfNull(plaintext);

        var plaintextBytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
        var nonce = new byte[NonceSize];
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[TagSize];

        RandomNumberGenerator.Fill(nonce);

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        // Layout: [nonce (12)] + [tag (16)] + [ciphertext]
        var result = new byte[NonceSize + TagSize + ciphertext.Length];
        nonce.CopyTo(result, 0);
        tag.CopyTo(result, NonceSize);
        ciphertext.CopyTo(result, NonceSize + TagSize);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string ciphertext)
    {
        ArgumentNullException.ThrowIfNull(ciphertext);

        var data = Convert.FromBase64String(ciphertext);

        var headerSize = NonceSize + TagSize;
        var nonce = data[..NonceSize];
        var tag = data[NonceSize..headerSize];
        var encrypted = data[headerSize..];
        var plaintext = new byte[encrypted.Length];

        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, encrypted, tag, plaintext);

        return System.Text.Encoding.UTF8.GetString(plaintext);
    }

    public void Dispose()
    {
        // key is managed memory, GC handles it
    }
}
