// <copyright file="SqsOptions.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

namespace MarcusPrado.Platform.AwsSqs.Options;

/// <summary>Configuration options for AWS SQS.</summary>
public sealed class SqsOptions
{
    /// <summary>Gets or sets an optional custom service URL (e.g., LocalStack endpoint).</summary>
    public string? ServiceUrl { get; set; }

    /// <summary>Gets or sets the AWS region. Defaults to <c>us-east-1</c>.</summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>Gets or sets the maximum number of messages to receive per poll. Defaults to <c>10</c>.</summary>
    public int MaxNumberOfMessages { get; set; } = 10;

    /// <summary>Gets or sets the long-polling wait time in seconds. Defaults to <c>20</c>.</summary>
    public int WaitTimeSeconds { get; set; } = 20;

    /// <summary>Gets or sets the visibility timeout in seconds. Defaults to <c>30</c>.</summary>
    public int VisibilityTimeoutSeconds { get; set; } = 30;

    /// <summary>Gets or sets the suffix appended to a queue name to derive its DLQ name. Defaults to <c>-dlq</c>.</summary>
    public string DlqSuffix { get; set; } = "-dlq";

    /// <summary>Gets or sets the maximum number of times a message may be received before it is sent to the DLQ. Defaults to <c>3</c>.</summary>
    public int MaxReceiveCount { get; set; } = 3;
}
