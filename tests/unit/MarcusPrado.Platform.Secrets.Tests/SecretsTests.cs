using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MarcusPrado.Platform.Secrets;

namespace MarcusPrado.Platform.Secrets.Tests;

public sealed class InMemorySecretProviderTests
{
    [Fact]
    public async Task GetSecretAsync_ExistingKey_ReturnsValue()
    {
        var provider = new InMemorySecretProvider(new Dictionary<string, string>
        {
            ["db-password"] = "s3cr3t"
        });

        var result = await provider.GetSecretAsync("db-password");

        result.Should().Be("s3cr3t");
    }

    [Fact]
    public async Task GetSecretAsync_MissingKey_ReturnsNull()
    {
        var provider = new InMemorySecretProvider();

        var result = await provider.GetSecretAsync("nonexistent");

        result.Should().BeNull();
    }

    [Fact]
    public async Task Set_ThenGet_ReturnsUpdatedValue()
    {
        var provider = new InMemorySecretProvider();
        provider.Set("api-key", "abc123");

        var result = await provider.GetSecretAsync("api-key");

        result.Should().Be("abc123");
    }
}

public sealed class CachedSecretProviderTests
{
    private static IMemoryCache CreateMemoryCache()
        => new MemoryCache(new MemoryCacheOptions());

    [Fact]
    public async Task GetSecretAsync_FirstCall_FetchesFromInner()
    {
        var callCount = 0;
        var delegate_ = new DelegateSecretProvider((name, _) =>
        {
            callCount++;
            return Task.FromResult<string?>("value1");
        });
        var cached = new CachedSecretProvider(delegate_, CreateMemoryCache(), new SecretCacheOptions());

        var result = await cached.GetSecretAsync("my-secret");

        result.Should().Be("value1");
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task GetSecretAsync_SecondCall_UsesCacheAndInnerCalledOnlyOnce()
    {
        var callCount = 0;
        var delegate_ = new DelegateSecretProvider((name, _) =>
        {
            callCount++;
            return Task.FromResult<string?>("value1");
        });
        var cached = new CachedSecretProvider(delegate_, CreateMemoryCache(), new SecretCacheOptions());

        await cached.GetSecretAsync("my-secret");
        var result = await cached.GetSecretAsync("my-secret");

        result.Should().Be("value1");
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task InvalidateCacheAsync_ClearsCache_NextCallFetchesFromInner()
    {
        var callCount = 0;
        var delegate_ = new DelegateSecretProvider((name, _) =>
        {
            callCount++;
            return Task.FromResult<string?>($"value{callCount}");
        });
        var cached = new CachedSecretProvider(delegate_, CreateMemoryCache(), new SecretCacheOptions());

        var first = await cached.GetSecretAsync("my-secret");
        await cached.InvalidateCacheAsync("my-secret");
        var second = await cached.GetSecretAsync("my-secret");

        first.Should().Be("value1");
        second.Should().Be("value2");
        callCount.Should().Be(2);
    }

    [Fact]
    public async Task GetSecretAsync_NullValueFromInner_NotCached()
    {
        var callCount = 0;
        var delegate_ = new DelegateSecretProvider((name, _) =>
        {
            callCount++;
            return Task.FromResult<string?>(null);
        });
        var cached = new CachedSecretProvider(delegate_, CreateMemoryCache(), new SecretCacheOptions());

        var first = await cached.GetSecretAsync("missing");
        var second = await cached.GetSecretAsync("missing");

        first.Should().BeNull();
        second.Should().BeNull();
        callCount.Should().Be(2, "null values should not be cached");
    }
}

public sealed class EnvironmentSecretProviderTests
{
    [Fact]
    public async Task GetSecretAsync_MapsHyphenatedNameToUppercaseEnvVar()
    {
        Environment.SetEnvironmentVariable("DB_PASSWORD", "env-secret");
        try
        {
            var provider = new EnvironmentSecretProvider();

            var result = await provider.GetSecretAsync("db-password");

            result.Should().Be("env-secret");
        }
        finally
        {
            Environment.SetEnvironmentVariable("DB_PASSWORD", null);
        }
    }

    [Fact]
    public async Task GetSecretAsync_MapsSlashesToUnderscoresAndUppercases()
    {
        Environment.SetEnvironmentVariable("MY_APP_SECRET", "slash-secret");
        try
        {
            var provider = new EnvironmentSecretProvider();

            var result = await provider.GetSecretAsync("my/app/secret");

            result.Should().Be("slash-secret");
        }
        finally
        {
            Environment.SetEnvironmentVariable("MY_APP_SECRET", null);
        }
    }
}

public sealed class DelegateSecretProviderTests
{
    [Fact]
    public async Task GetSecretAsync_CallsDelegateWithCorrectName()
    {
        string? capturedName = null;
        var provider = new DelegateSecretProvider((name, _) =>
        {
            capturedName = name;
            return Task.FromResult<string?>("delegated-value");
        });

        var result = await provider.GetSecretAsync("target-secret");

        capturedName.Should().Be("target-secret");
        result.Should().Be("delegated-value");
    }

    [Fact]
    public async Task InvalidateCacheAsync_TriggersOnRotationCallback()
    {
        string? rotatedName = null;
        var provider = new DelegateSecretProvider(
            (name, _) => Task.FromResult<string?>("value"),
            onRotation: name => rotatedName = name);

        await provider.InvalidateCacheAsync("rotated-secret");

        rotatedName.Should().Be("rotated-secret");
    }

    [Fact]
    public async Task InvalidateCacheAsync_NoCallback_DoesNotThrow()
    {
        var provider = new DelegateSecretProvider((name, _) => Task.FromResult<string?>("value"));

        var act = async () => await provider.InvalidateCacheAsync("any-secret");

        await act.Should().NotThrowAsync();
    }
}
