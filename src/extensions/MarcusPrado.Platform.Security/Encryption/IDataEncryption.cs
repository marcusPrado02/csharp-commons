namespace MarcusPrado.Platform.Security.Encryption;

public interface IDataEncryption
{
    /// <summary>Encrypts plaintext. Returns a base64url-encoded ciphertext with embedded nonce.</summary>
    string Encrypt(string plaintext);

    /// <summary>Decrypts a value previously encrypted by <see cref="Encrypt"/>.</summary>
    string Decrypt(string ciphertext);
}
