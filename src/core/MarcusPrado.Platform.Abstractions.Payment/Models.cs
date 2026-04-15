namespace MarcusPrado.Platform.Abstractions.Payment;

/// <summary>Request to charge a customer.</summary>
public sealed record PaymentRequest(string CustomerId, decimal Amount, string Currency, string? Description = null);

/// <summary>A completed payment transaction.</summary>
public sealed record Payment(
    string Id,
    string CustomerId,
    decimal Amount,
    string Currency,
    PaymentStatus Status,
    DateTimeOffset CreatedAt
);

/// <summary>A refund issued against a payment.</summary>
public sealed record Refund(string Id, string PaymentId, decimal Amount, string Currency, DateTimeOffset CreatedAt);

/// <summary>Request to create a recurring subscription.</summary>
public sealed record SubscriptionRequest(string CustomerId, string PriceId, string? Description = null);

/// <summary>A recurring subscription.</summary>
public sealed record Subscription(
    string Id,
    string CustomerId,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CancelledAt
);

/// <summary>The lifecycle status of a payment transaction.</summary>
public enum PaymentStatus
{
    /// <summary>Payment is pending processing.</summary>
    Pending,

    /// <summary>Payment was successfully captured.</summary>
    Succeeded,

    /// <summary>Payment failed.</summary>
    Failed,

    /// <summary>Payment was fully refunded.</summary>
    Refunded,

    /// <summary>Payment was cancelled before capture.</summary>
    Cancelled,
}
