namespace MarcusPrado.Platform.AspNetCore.Endpoints;

/// <summary>
/// Marker interface for Minimal-API endpoint classes.
/// Implementations declare their routes inside <see cref="MapEndpoints"/>.
/// </summary>
public interface IEndpoint
{
    /// <summary>Maps the endpoint routes onto <paramref name="app"/>.</summary>
    void MapEndpoints(IEndpointRouteBuilder app);
}
