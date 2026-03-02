namespace MarcusPrado.Platform.AspNetCore.Endpoints;

/// <summary>
/// Base class for endpoint groups that share a common route prefix.
/// Subclasses declare the prefix via <see cref="Group"/> and register
/// routes inside <see cref="MapRoutes"/>.
/// </summary>
public abstract class EndpointGroupBase : IEndpoint
{
    /// <summary>
    /// The route prefix shared by all endpoints in this group, e.g. <c>"api/orders"</c>.
    /// </summary>
    protected abstract string Group { get; }

    /// <inheritdoc/>
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(Group);
        MapRoutes(group);
    }

    /// <summary>Register individual routes on the pre-configured <paramref name="group"/>.</summary>
    protected abstract void MapRoutes(RouteGroupBuilder group);
}
