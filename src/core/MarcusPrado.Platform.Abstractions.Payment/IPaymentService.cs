namespace MarcusPrado.Platform.Abstractions.Payment;

/// <summary>Processes payments and refunds.</summary>
public interface IPaymentService
{
    /// <summary>Charges the customer and returns the resulting payment.</summary>
    Task<Payment> ChargeAsync(PaymentRequest request, CancellationToken ct = default);

    /// <summary>Returns a payment by its ID.</summary>
    Task<Payment> GetPaymentAsync(string paymentId, CancellationToken ct = default);

    /// <summary>Issues a full or partial refund for the given payment.</summary>
    Task<Refund> RefundAsync(string paymentId, decimal? amount = null, CancellationToken ct = default);
}

/// <summary>Manages recurring subscription plans.</summary>
public interface ISubscriptionService
{
    /// <summary>Creates a new subscription for the customer.</summary>
    Task<Subscription> CreateAsync(SubscriptionRequest request, CancellationToken ct = default);

    /// <summary>Cancels an active subscription.</summary>
    Task<Subscription> CancelAsync(string subscriptionId, CancellationToken ct = default);

    /// <summary>Returns a subscription by its ID.</summary>
    Task<Subscription> GetAsync(string subscriptionId, CancellationToken ct = default);
}
