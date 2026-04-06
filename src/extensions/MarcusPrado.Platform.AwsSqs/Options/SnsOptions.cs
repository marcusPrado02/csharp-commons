// <copyright file="SnsOptions.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

namespace MarcusPrado.Platform.AwsSqs.Options;

/// <summary>Configuration options for AWS SNS.</summary>
public sealed class SnsOptions
{
    /// <summary>Gets or sets an optional custom service URL (e.g., LocalStack endpoint).</summary>
    public string? ServiceUrl { get; set; }

    /// <summary>Gets or sets the AWS region. Defaults to <c>us-east-1</c>.</summary>
    public string Region { get; set; } = "us-east-1";
}
