using FluentAssertions;
using MarcusPrado.Platform.Configuration;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace MarcusPrado.Platform.Configuration.Tests;

public sealed class EncryptedJsonConfigurationProviderTests
{
    private const string Key = "json-test-key-789";

    [Fact]
    public void AddEncryptedJsonFile_ShouldDecryptEncValues()
    {
        var plainPassword = "super-secret-db-password";
        var encrypted = ConfigCipherTool.Encrypt(plainPassword, Key);

        var json = $$"""
            {
              "Database": {
                "Password": "{{encrypted}}"
              }
            }
            """;

        var tmpFile = Path.GetTempFileName();
        File.WriteAllText(tmpFile, json);

        try
        {
            var config = new ConfigurationBuilder()
                .AddEncryptedJsonFile(tmpFile, value => ConfigCipherTool.Decrypt(value, Key), optional: false)
                .Build();

            config["Database:Password"].Should().Be(plainPassword);
        }
        finally
        {
            File.Delete(tmpFile);
        }
    }

    [Fact]
    public void AddEncryptedJsonFile_ShouldLeaveNonEncValuesUnchanged()
    {
        var json = """
            {
              "App": {
                "Name": "MyApp"
              }
            }
            """;

        var tmpFile = Path.GetTempFileName();
        File.WriteAllText(tmpFile, json);

        try
        {
            var config = new ConfigurationBuilder()
                .AddEncryptedJsonFile(tmpFile, value => ConfigCipherTool.Decrypt(value, Key), optional: false)
                .Build();

            config["App:Name"].Should().Be("MyApp");
        }
        finally
        {
            File.Delete(tmpFile);
        }
    }

    [Fact]
    public void AddEncryptedJsonFile_WithNullBuilder_ShouldThrowArgumentNullException()
    {
        IConfigurationBuilder? builder = null;

        var act = () => builder!.AddEncryptedJsonFile("file.json", v => v);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddEncryptedJsonFile_WithNullDecryptor_ShouldThrowArgumentNullException()
    {
        var builder = new ConfigurationBuilder();

        var act = () => builder.AddEncryptedJsonFile("file.json", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddEncryptedJsonFile_MixedValues_ShouldDecryptOnlyEncValues()
    {
        var plainPassword = "secret123";
        var encrypted = ConfigCipherTool.Encrypt(plainPassword, Key);

        var json = $$"""
            {
              "Database": {
                "Host": "localhost",
                "Password": "{{encrypted}}"
              }
            }
            """;

        var tmpFile = Path.GetTempFileName();
        File.WriteAllText(tmpFile, json);

        try
        {
            var config = new ConfigurationBuilder()
                .AddEncryptedJsonFile(tmpFile, value => ConfigCipherTool.Decrypt(value, Key), optional: false)
                .Build();

            config["Database:Host"].Should().Be("localhost");
            config["Database:Password"].Should().Be(plainPassword);
        }
        finally
        {
            File.Delete(tmpFile);
        }
    }

    [Fact]
    public void AddEncryptedEnvironmentVariables_WithNullBuilder_ShouldThrowArgumentNullException()
    {
        IConfigurationBuilder? builder = null;

        var act = () => builder!.AddEncryptedEnvironmentVariables(v => v);

        act.Should().Throw<ArgumentNullException>();
    }
}
