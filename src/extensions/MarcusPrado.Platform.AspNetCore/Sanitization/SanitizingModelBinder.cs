using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.AspNetCore.Sanitization;

/// <summary>An <see cref="IModelBinder"/> that sanitizes string values when <see cref="SanitizeInputAttribute"/> is present.</summary>
public sealed class SanitizingModelBinder : IModelBinder
{
    private readonly IModelBinder _innerBinder;
    private readonly IInputSanitizer _sanitizer;
    private readonly bool _stripAll;

    /// <summary>Initializes a new instance of <see cref="SanitizingModelBinder"/>.</summary>
    public SanitizingModelBinder(IModelBinder innerBinder, IInputSanitizer sanitizer, bool stripAll)
    {
        _innerBinder = innerBinder;
        _sanitizer = sanitizer;
        _stripAll = stripAll;
    }

    /// <inheritdoc/>
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        await _innerBinder.BindModelAsync(bindingContext);

        if (bindingContext.Result.IsModelSet && bindingContext.Result.Model is string raw)
        {
            var sanitized = _stripAll
                ? _sanitizer.StripHtml(raw)
                : _sanitizer.SanitizeHtml(raw);
            bindingContext.Result = ModelBindingResult.Success(sanitized);
        }
    }
}

/// <summary>Provides <see cref="SanitizingModelBinder"/> for string parameters decorated with <see cref="SanitizeInputAttribute"/>.</summary>
public sealed class SanitizingModelBinderProvider : IModelBinderProvider
{
    /// <inheritdoc/>
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Metadata.ModelType != typeof(string))
            return null;

        SanitizeInputAttribute? attr = null;

        if (context.Metadata is DefaultModelMetadata defaultMetadata)
        {
            attr = defaultMetadata.Attributes.ParameterAttributes?
                .OfType<SanitizeInputAttribute>()
                .FirstOrDefault()
                ?? defaultMetadata.Attributes.PropertyAttributes?
                .OfType<SanitizeInputAttribute>()
                .FirstOrDefault();
        }

        if (attr is null)
            return null;

        var innerBinder = context.CreateBinder(
            context.MetadataProvider.GetMetadataForType(typeof(string)));

        var sanitizer = context.Services.GetRequiredService<IInputSanitizer>();
        return new SanitizingModelBinder(innerBinder, sanitizer, attr.StripAll);
    }
}
