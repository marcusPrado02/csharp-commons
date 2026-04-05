using FluentAssertions;
using MarcusPrado.Platform.Configuration;
using Xunit;

namespace MarcusPrado.Platform.Configuration.Tests;

public sealed class ConfigCipherToolTests
{
    private const string Key = "test-secret-key-123";

    [Fact]
    public void Encrypt_ShouldReturnEncEnvelope()
    {
        var result = ConfigCipherTool.Encrypt("hello world", Key);

        result.Should().StartWith("ENC(").And.EndWith(")");
    }

    [Fact]
    public void Decrypt_AfterEncrypt_ShouldReturnOriginalValue()
    {
        const string plainText = "my-secret-password";

        var encrypted = ConfigCipherTool.Encrypt(plainText, Key);
        var decrypted = ConfigCipherTool.Decrypt(encrypted, Key);

        decrypted.Should().Be(plainText);
    }

    [Fact]
    public void Encrypt_ShouldProduceDifferentOutputForSamePlainText()
    {
        // Each encryption call generates a new IV
        var enc1 = ConfigCipherTool.Encrypt("same-value", Key);
        var enc2 = ConfigCipherTool.Encrypt("same-value", Key);

        enc1.Should().NotBe(enc2);
    }

    [Fact]
    public void Decrypt_WithWrongKey_ShouldThrow()
    {
        var encrypted = ConfigCipherTool.Encrypt("secret", Key);

        var act = () => ConfigCipherTool.Decrypt(encrypted, "wrong-key");

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Decrypt_WithoutEncEnvelope_ShouldThrowArgumentException()
    {
        var act = () => ConfigCipherTool.Decrypt("plain-text", Key);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*ENC(*)*");
    }

    [Fact]
    public void Encrypt_WithEmptyString_ShouldReturnEncEnvelope()
    {
        var result = ConfigCipherTool.Encrypt(string.Empty, Key);

        result.Should().StartWith("ENC(").And.EndWith(")");
        var decrypted = ConfigCipherTool.Decrypt(result, Key);
        decrypted.Should().BeEmpty();
    }

    [Fact]
    public void Decrypt_WithNullInput_ShouldThrowArgumentNullException()
    {
        var act = () => ConfigCipherTool.Decrypt(null!, Key);

        act.Should().Throw<ArgumentNullException>();
    }
}
