using MarcusPrado.Platform.Abstractions.Payment;
using Stripe;
using AbsPayment = MarcusPrado.Platform.Abstractions.Payment;

namespace MarcusPrado.Platform.Stripe.Payment;

/// <summary>Implements <see cref="IPaymentService"/> via the Stripe Payments API.</summary>
public sealed class StripePaymentService : AbsPayment.IPaymentService
{
    private readonly PaymentIntentService _intents;
    private readonly RefundService _refunds;

    /// <summary>Initializes a new instance using the provided Stripe client.</summary>
    public StripePaymentService(IStripeClient stripeClient)
    {
        ArgumentNullException.ThrowIfNull(stripeClient);
        _intents = new PaymentIntentService(stripeClient);
        _refunds = new RefundService(stripeClient);
    }

    /// <inheritdoc />
    public async Task<AbsPayment.Payment> ChargeAsync(
        PaymentRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var options = new PaymentIntentCreateOptions
        {
            Customer    = request.CustomerId,
            Amount      = ToStripeAmount(request.Amount),
            Currency    = request.Currency.ToLowerInvariant(),
            Description = request.Description,
            Confirm     = true,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled       = true,
                AllowRedirects = "never",
            },
        };

        var intent = await _intents.CreateAsync(options, cancellationToken: ct)
            .ConfigureAwait(false);

        return MapIntent(intent);
    }

    /// <inheritdoc />
    public async Task<AbsPayment.Payment> GetPaymentAsync(
        string paymentId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paymentId);

        var intent = await _intents.GetAsync(paymentId, cancellationToken: ct)
            .ConfigureAwait(false);

        return MapIntent(intent);
    }

    /// <inheritdoc />
    public async Task<AbsPayment.Refund> RefundAsync(
        string paymentId, decimal? amount = null, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paymentId);

        var options = new RefundCreateOptions
        {
            PaymentIntent = paymentId,
            Amount        = amount.HasValue ? ToStripeAmount(amount.Value) : null,
        };

        var refund = await _refunds.CreateAsync(options, cancellationToken: ct)
            .ConfigureAwait(false);

        return new AbsPayment.Refund(
            refund.Id,
            refund.PaymentIntentId,
            FromStripeAmount(refund.Amount),
            refund.Currency.ToUpperInvariant(),
            refund.Created);
    }

    private static AbsPayment.Payment MapIntent(PaymentIntent intent) =>
        new(
            intent.Id,
            intent.CustomerId ?? string.Empty,
            FromStripeAmount(intent.Amount),
            intent.Currency.ToUpperInvariant(),
            MapStatus(intent.Status),
            intent.Created);

    private static PaymentStatus MapStatus(string status) => status switch
    {
        "succeeded"                  => PaymentStatus.Succeeded,
        "canceled"                   => PaymentStatus.Cancelled,
        "requires_payment_method"    => PaymentStatus.Pending,
        "requires_confirmation"      => PaymentStatus.Pending,
        "requires_action"            => PaymentStatus.Pending,
        "processing"                 => PaymentStatus.Pending,
        _                            => PaymentStatus.Failed,
    };

    private static long ToStripeAmount(decimal amount) => (long)(amount * 100);

    private static decimal FromStripeAmount(long amount) => amount / 100m;
}
