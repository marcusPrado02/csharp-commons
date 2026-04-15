using MarcusPrado.Platform.Abstractions.Errors;

namespace MarcusPrado.Platform.ErrorCatalog;

/// <summary>
/// Central catalog of well-known domain errors, grouped by bounded context.
/// Each entry is a <c>static readonly</c> <see cref="Error"/> instance with a
/// stable machine-readable code and a default English message.
/// </summary>
/// <remarks>
/// Consumers should use these constants rather than constructing <see cref="Error"/>
/// values ad-hoc so that error codes remain consistent across the platform.
/// Use <see cref="IErrorTranslator"/> to obtain locale-specific messages.
/// </remarks>
public static class ErrorCatalog
{
    /// <summary>Payment domain errors.</summary>
    public static class Payment
    {
        /// <summary>The requested payment record could not be found.</summary>
        public static readonly Error NotFound = Error.NotFound("PAYMENT_001", "Payment not found.");

        /// <summary>The account does not have sufficient funds to complete the payment.</summary>
        public static readonly Error Insufficient = Error.Validation("PAYMENT_002", "Insufficient funds.");

        /// <summary>The payment could not be processed by the payment gateway.</summary>
        public static readonly Error ProcessingFailed = Error.Technical("PAYMENT_003", "Payment processing failed.");
    }

    /// <summary>User domain errors.</summary>
    public static class User
    {
        /// <summary>The requested user could not be found.</summary>
        public static readonly Error NotFound = Error.NotFound("USER_001", "User not found.");

        /// <summary>A user with the same identifier already exists.</summary>
        public static readonly Error AlreadyExists = Error.Conflict("USER_002", "User already exists.");

        /// <summary>The caller is not authorized to perform this operation.</summary>
        public static readonly Error Unauthorized = Error.Unauthorized("USER_003", "Unauthorized.");
    }

    /// <summary>Order domain errors.</summary>
    public static class Order
    {
        /// <summary>The requested order could not be found.</summary>
        public static readonly Error NotFound = Error.NotFound("ORDER_001", "Order not found.");

        /// <summary>The order is in an invalid state for the attempted operation.</summary>
        public static readonly Error InvalidState = Error.Validation(
            "ORDER_002",
            "Order is in an invalid state for this operation."
        );

        /// <summary>The order has already been cancelled and cannot be modified.</summary>
        public static readonly Error AlreadyCancelled = Error.Conflict(
            "ORDER_003",
            "Order has already been cancelled."
        );
    }
}
