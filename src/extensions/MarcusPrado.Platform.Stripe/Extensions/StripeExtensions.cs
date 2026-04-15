using MarcusPrado.Platform.Abstractions.Payment;
using MarcusPrado.Platform.Stripe.Options;
using MarcusPrado.Platform.Stripe.Payment;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace MarcusPrado.Platform.Stripe.Extensions;

/// <summary>Extension methods to register Stripe payment services.</summary>
public static class StripeExtensions
{
    /// <summary>
    /// Registers <see cref="IPaymentService"/> and <see cref="ISubscriptionService"/>
    /// backed by Stripe.
    /// </summary>
    public static IServiceCollection AddPlatformStripe(
        this IServiceCollection services,
        Action<StripeOptions>? configure = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        var opts = new StripeOptions();
        configure?.Invoke(opts);

        services.AddSingleton(opts);
        services.AddSingleton<IStripeClient>(_ => new StripeClient(opts.ApiKey));
        services.AddSingleton<IPaymentService, StripePaymentService>();
        services.AddSingleton<ISubscriptionService, StripeSubscriptionService>();

        return services;
    }
}
