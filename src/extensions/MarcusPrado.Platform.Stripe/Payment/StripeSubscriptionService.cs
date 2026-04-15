using MarcusPrado.Platform.Abstractions.Payment;
using Stripe;
using AbsPayment = MarcusPrado.Platform.Abstractions.Payment;

namespace MarcusPrado.Platform.Stripe.Payment;

/// <summary>Implements <see cref="ISubscriptionService"/> via the Stripe Billing API.</summary>
public sealed class StripeSubscriptionService : AbsPayment.ISubscriptionService
{
    private readonly SubscriptionService _subscriptions;

    /// <summary>Initializes a new instance using the provided Stripe client.</summary>
    public StripeSubscriptionService(IStripeClient stripeClient)
    {
        ArgumentNullException.ThrowIfNull(stripeClient);
        _subscriptions = new SubscriptionService(stripeClient);
    }

    /// <inheritdoc />
    public async Task<AbsPayment.Subscription> CreateAsync(SubscriptionRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var options = new SubscriptionCreateOptions
        {
            Customer = request.CustomerId,
            Items = [new SubscriptionItemOptions { Price = request.PriceId }],
            Description = request.Description,
        };

        var sub = await _subscriptions.CreateAsync(options, cancellationToken: ct).ConfigureAwait(false);

        return MapSubscription(sub);
    }

    /// <inheritdoc />
    public async Task<AbsPayment.Subscription> CancelAsync(string subscriptionId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionId);

        var sub = await _subscriptions.CancelAsync(subscriptionId, cancellationToken: ct).ConfigureAwait(false);

        return MapSubscription(sub);
    }

    /// <inheritdoc />
    public async Task<AbsPayment.Subscription> GetAsync(string subscriptionId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionId);

        var sub = await _subscriptions.GetAsync(subscriptionId, cancellationToken: ct).ConfigureAwait(false);

        return MapSubscription(sub);
    }

    private static AbsPayment.Subscription MapSubscription(global::Stripe.Subscription sub) =>
        new(sub.Id, sub.CustomerId, sub.Status, sub.Created, sub.CanceledAt);
}
