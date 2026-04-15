using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MarcusPrado.Platform.TestKit.Fixtures;

/// <summary>
/// Base class for API integration tests using <see cref="WebApplicationFactory{TProgram}"/>.
/// Provides a pre-configured <see cref="HttpClient"/> with correlation headers.
/// </summary>
/// <typeparam name="TProgram">The application entry point type.</typeparam>
public abstract class ApiFixture<TProgram> : IAsyncLifetime
    where TProgram : class
{
    private WebApplicationFactory<TProgram>? _factory;

    /// <summary>The HTTP client configured for the test server.</summary>
    public HttpClient Client { get; private set; } = null!;

    /// <summary>The underlying <see cref="WebApplicationFactory{TProgram}"/>.</summary>
    protected WebApplicationFactory<TProgram> Factory =>
        _factory ?? throw new InvalidOperationException("InitializeAsync has not been called.");

    /// <inheritdoc/>
    public virtual Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<TProgram>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(ConfigureTestServices);
        });

        Client = _factory.CreateClient();
        Client.DefaultRequestHeaders.Add("X-Correlation-Id", Guid.NewGuid().ToString());
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual async Task DisposeAsync()
    {
        Client.Dispose();
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }
    }

    /// <summary>Override to replace services in the test DI container.</summary>
    protected virtual void ConfigureTestServices(IServiceCollection services) { }
}
