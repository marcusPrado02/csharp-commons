namespace MarcusPrado.Platform.AwsSns.Options;

/// <summary>Configuration for the AWS SNS SMS adapter.</summary>
public sealed class AwsSnsOptions
{
    /// <summary>Gets or sets the AWS region (e.g. "us-east-1").</summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>Gets or sets the default sender ID shown to recipients (not supported in all countries).</summary>
    public string? SenderId { get; set; }

    /// <summary>Gets or sets the SMS type: "Transactional" (default) or "Promotional".</summary>
    public string SmsType { get; set; } = "Transactional";
}
