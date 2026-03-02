namespace MarcusPrado.Platform.Redis.Tests.Caching;

public sealed class RedisCacheOptionsTests
{
    [Fact]
    public void Defaults_AreExpected()
    {
        var opts = new RedisCacheOptions();
        Assert.Equal("localhost:6379", opts.ConnectionString);
        Assert.Equal("cache:", opts.KeyPrefix);
        Assert.Equal(TimeSpan.FromMinutes(30), opts.DefaultExpiry);
    }

    [Fact]
    public void KeyPrefix_CanBeSet()
    {
        var opts = new RedisCacheOptions { KeyPrefix = "test:" };
        Assert.Equal("test:", opts.KeyPrefix);
    }
}
