using FluentAssertions;
using MarcusPrado.Platform.Configuration;
using Xunit;

namespace MarcusPrado.Platform.Configuration.Tests;

public sealed class EncryptedEnvironmentVariableProviderTests
{
    private const string Key = "env-test-key-456";

    [Fact]
    public void Load_ShouldDecryptEncValues()
    {
        var plainText = "my-db-password";
        var encrypted = ConfigCipherTool.Encrypt(plainText, Key);

        var varName = $"TEST_ENC_VAR_{Guid.NewGuid():N}";
        Environment.SetEnvironmentVariable(varName, encrypted);

        try
        {
            var provider = new EncryptedEnvironmentVariableProvider(value => ConfigCipherTool.Decrypt(value, Key));
            provider.Load();

            var found = provider.TryGet(varName, out var result);
            found.Should().BeTrue();
            result.Should().Be(plainText);
        }
        finally
        {
            Environment.SetEnvironmentVariable(varName, null);
        }
    }

    [Fact]
    public void Load_ShouldPassThroughNonEncValues()
    {
        var varName = $"TEST_PLAIN_VAR_{Guid.NewGuid():N}";
        Environment.SetEnvironmentVariable(varName, "plain-value");

        try
        {
            var provider = new EncryptedEnvironmentVariableProvider(value => ConfigCipherTool.Decrypt(value, Key));
            provider.Load();

            var found = provider.TryGet(varName, out var result);
            found.Should().BeTrue();
            result.Should().Be("plain-value");
        }
        finally
        {
            Environment.SetEnvironmentVariable(varName, null);
        }
    }

    [Fact]
    public void Constructor_WithNullDecryptor_ShouldThrowArgumentNullException()
    {
        var act = () => new EncryptedEnvironmentVariableProvider(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("decryptor");
    }

    [Fact]
    public void Load_ShouldNormalizeDoubleUnderscoreToColon()
    {
        var varName = $"TEST__NESTED__{Guid.NewGuid():N}";
        Environment.SetEnvironmentVariable(varName, "nested-value");

        try
        {
            var provider = new EncryptedEnvironmentVariableProvider(v => v);
            provider.Load();

            var normalizedKey = varName.Replace("__", ":", StringComparison.Ordinal);
            var found = provider.TryGet(normalizedKey, out var result);
            found.Should().BeTrue();
            result.Should().Be("nested-value");
        }
        finally
        {
            Environment.SetEnvironmentVariable(varName, null);
        }
    }
}
