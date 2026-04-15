using System.Security.Cryptography;
using FluentAssertions;
using MarcusPrado.Platform.Security.Encryption;
using Xunit;

namespace MarcusPrado.Platform.Security.Tests;

public sealed class EncryptionTests
{
    private static byte[] CreateKey() => RandomNumberGenerator.GetBytes(32);

    // ── AesGcmEncryption ────────────────────────────────────────────────────

    [Fact]
    public void Encrypt_ResultDiffersFromPlaintext()
    {
        var enc = new AesGcmEncryption(CreateKey());
        var result = enc.Encrypt("hello world");

        result.Should().NotBe("hello world");
    }

    [Fact]
    public void EncryptDecrypt_RoundTrip_RecoverOriginalPlaintext()
    {
        var enc = new AesGcmEncryption(CreateKey());
        var plaintext = "sensitive data 1234";

        var ciphertext = enc.Encrypt(plaintext);
        var recovered = enc.Decrypt(ciphertext);

        recovered.Should().Be(plaintext);
    }

    [Fact]
    public void Encrypt_SamePlaintext_ProducesDifferentCiphertexts()
    {
        var enc = new AesGcmEncryption(CreateKey());

        var c1 = enc.Encrypt("same plaintext");
        var c2 = enc.Encrypt("same plaintext");

        c1.Should().NotBe(c2);
    }

    [Fact]
    public void Decrypt_TamperedCiphertext_ThrowsCryptographicException()
    {
        var enc = new AesGcmEncryption(CreateKey());
        var ciphertext = enc.Encrypt("tamper me");

        // Flip a byte deep in the ciphertext payload
        var bytes = Convert.FromBase64String(ciphertext);
        bytes[^1] ^= 0xFF;
        var tampered = Convert.ToBase64String(bytes);

        var act = () => enc.Decrypt(tampered);

        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Constructor_WrongKeyLength_ThrowsArgumentException()
    {
        var shortKey = new byte[16]; // 128-bit — should be 256-bit

        var act = () => new AesGcmEncryption(shortKey);

        act.Should().Throw<ArgumentException>().WithParameterName("key");
    }

    // ── EncryptingValueConverter ─────────────────────────────────────────────

    [Fact]
    public void EncryptingValueConverter_EncryptsThenDecryptsCorrectly()
    {
        var enc = new AesGcmEncryption(CreateKey());
        var converter = new EncryptingValueConverter(enc);

        var toProvider = (Func<string, string>)converter.ConvertToProviderExpression.Compile();
        var fromProvider = (Func<string, string>)converter.ConvertFromProviderExpression.Compile();

        var plaintext = "my secret value";
        var encrypted = toProvider(plaintext);
        var decrypted = fromProvider(encrypted);

        encrypted.Should().NotBe(plaintext);
        decrypted.Should().Be(plaintext);
    }

    // ── KeyRotationService ───────────────────────────────────────────────────

    [Fact]
    public void KeyRotationService_Encrypt_PrefixesWithVersion()
    {
        var keys = new Dictionary<int, byte[]> { [1] = CreateKey() };
        var svc = new KeyRotationService(keys, currentVersion: 1);

        var result = svc.Encrypt("data");

        result.Should().StartWith("v1:");
    }

    [Fact]
    public void KeyRotationService_Decrypt_CanDecryptWithOlderKeyVersion()
    {
        var oldKey = CreateKey();
        var newKey = CreateKey();

        // Encrypt with old key (version 1)
        var oldKeys = new Dictionary<int, byte[]> { [1] = oldKey };
        var oldSvc = new KeyRotationService(oldKeys, currentVersion: 1);
        var ciphertext = oldSvc.Encrypt("legacy secret");

        // Decrypt with service that knows both keys, current is v2
        var allKeys = new Dictionary<int, byte[]> { [1] = oldKey, [2] = newKey };
        var newSvc = new KeyRotationService(allKeys, currentVersion: 2);

        var recovered = newSvc.Decrypt(ciphertext);

        recovered.Should().Be("legacy secret");
    }

    [Fact]
    public void KeyRotationService_Decrypt_UnknownVersion_ThrowsKeyNotFoundException()
    {
        var keys = new Dictionary<int, byte[]> { [1] = CreateKey() };
        var svc = new KeyRotationService(keys, currentVersion: 1);

        // Forge a version-99 prefix
        var act = () => svc.Decrypt("v99:somedata");

        act.Should().Throw<KeyNotFoundException>();
    }
}
