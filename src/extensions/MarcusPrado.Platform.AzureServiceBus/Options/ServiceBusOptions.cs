// <copyright file="ServiceBusOptions.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

namespace MarcusPrado.Platform.AzureServiceBus.Options;

/// <summary>Configuration options for Azure Service Bus.</summary>
public sealed class ServiceBusOptions
{
    /// <summary>
    /// Gets or sets the Azure Service Bus connection string.
    /// Use this or <see cref="FullyQualifiedNamespace"/> — not both.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the fully-qualified namespace (e.g. <c>my-namespace.servicebus.windows.net</c>).
    /// When set, <see cref="DefaultAzureCredential"/> is used for authentication.
    /// </summary>
    public string? FullyQualifiedNamespace { get; set; }

    /// <summary>Gets or sets the maximum number of concurrent message processing calls. Defaults to <c>10</c>.</summary>
    public int MaxConcurrentCalls { get; set; } = 10;

    /// <summary>Gets or sets how long the processor automatically renews message locks. Defaults to <c>5 minutes</c>.</summary>
    public TimeSpan MaxAutoLockRenewalDuration { get; set; } = TimeSpan.FromMinutes(5);
}
