using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MarcusPrado.Platform.AspNetCore.OpenApi;

/// <summary>
/// Adds standard platform request headers (X-Correlation-Id, X-Tenant-Id) to every OpenAPI operation.
/// </summary>
public sealed class PlatformOperationTransformer : IOpenApiOperationTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Correlation-Id",
            In = ParameterLocation.Header,
            Required = false,
            Schema = new OpenApiSchema { Type = "string" },
            Description = "Client-supplied correlation identifier for request tracing."
        });

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Tenant-Id",
            In = ParameterLocation.Header,
            Required = false,
            Schema = new OpenApiSchema { Type = "string" },
            Description = "Tenant identifier for multi-tenant routing."
        });

        return Task.CompletedTask;
    }
}
