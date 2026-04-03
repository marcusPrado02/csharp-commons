namespace MarcusPrado.Platform.Stripe.Options;

/// <summary>Configuration for the Stripe payment adapter.</summary>
public sealed class StripeOptions
{
    /// <summary>Gets or sets the Stripe secret API key.</summary>
    public string ApiKey { get; set; } = string.Empty;
}
