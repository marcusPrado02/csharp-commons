using System.Net;
using System.Text.Json;

namespace MarcusPrado.Platform.AspNetCore.IpFiltering;

/// <summary>
/// ASP.NET Core middleware that filters requests based on the client's IP address
/// using configurable CIDR-based whitelist and blacklist rules.
/// </summary>
public sealed class IpFilterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IIpFilterStore _store;
    private readonly IpFilterOptions _options;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>Initialises the middleware.</summary>
    public IpFilterMiddleware(RequestDelegate next, IIpFilterStore store, IpFilterOptions options)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(options);

        _next = next;
        _store = store;
        _options = options;
    }

    /// <summary>Processes the request, applying IP filter rules.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = ResolveClientIp(context);

        if (clientIp is null)
        {
            await WriteForbiddenAsync(context);
            return;
        }

        var blacklist = await _store.GetBlacklistAsync(context.RequestAborted);
        if (IsMatch(clientIp, blacklist))
        {
            await WriteForbiddenAsync(context);
            return;
        }

        var whitelist = await _store.GetWhitelistAsync(context.RequestAborted);
        if (whitelist.Count > 0 && !IsMatch(clientIp, whitelist))
        {
            await WriteForbiddenAsync(context);
            return;
        }

        await _next(context);
    }

    private IPAddress? ResolveClientIp(HttpContext context)
    {
        if (_options.TrustForwardedFor)
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwardedFor))
            {
                // X-Forwarded-For may contain a comma-separated list; the first entry is the client IP.
                var firstEntry = forwardedFor.Split(',')[0].Trim();
                if (IPAddress.TryParse(firstEntry, out var ip))
                    return ip;
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(realIp) && IPAddress.TryParse(realIp.Trim(), out var realIpAddress))
                return realIpAddress;
        }

        return context.Connection.RemoteIpAddress;
    }

    private static bool IsMatch(IPAddress clientIp, IReadOnlyList<string> entries)
    {
        foreach (var entry in entries)
        {
            if (entry.Contains('/'))
            {
                // CIDR notation — use IPNetwork.Contains
                if (IPNetwork.TryParse(entry, out var network) && network.Contains(clientIp))
                    return true;
            }
            else
            {
                // Plain IP address — exact match
                if (IPAddress.TryParse(entry, out var parsedIp) && parsedIp.Equals(clientIp))
                    return true;
            }
        }

        return false;
    }

    private static async Task WriteForbiddenAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
            title = "Forbidden",
            status = 403,
            detail = "Access denied from your IP address."
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, _jsonOptions));
    }
}
