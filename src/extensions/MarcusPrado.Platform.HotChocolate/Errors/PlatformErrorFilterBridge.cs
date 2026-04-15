using MarcusPrado.Platform.Abstractions.GraphQL;
using HC = global::HotChocolate;

namespace MarcusPrado.Platform.HotChocolate.Errors;

/// <summary>
/// Adapts the platform <see cref="IPlatformErrorFilter"/> to HotChocolate's
/// <see cref="HC.IErrorFilter"/> interface.
/// </summary>
public sealed class PlatformErrorFilterBridge : HC.IErrorFilter
{
    private readonly IPlatformErrorFilter _platformFilter;

    /// <summary>Initializes a new instance of <see cref="PlatformErrorFilterBridge"/>.</summary>
    public PlatformErrorFilterBridge(IPlatformErrorFilter platformFilter)
    {
        ArgumentNullException.ThrowIfNull(platformFilter);
        _platformFilter = platformFilter;
    }

    /// <inheritdoc />
    public HC.IError OnError(HC.IError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (error.Exception is null)
            return error;

        var wrapped = new HotChocolateErrorWrapper(error);
        var result = _platformFilter.OnError(wrapped, error.Exception);

        return error.WithMessage(result.Message).WithCode(result.Code);
    }
}
