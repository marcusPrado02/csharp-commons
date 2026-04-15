namespace MarcusPrado.Platform.Governance.Deprecation;

/// <summary>
/// Defines the timeline for transitioning a contract from <em>Active</em> to
/// <em>Deprecated</em> to <em>Retired</em>.
/// </summary>
public sealed class DeprecationSchedule
{
    /// <summary>Date when the contract becomes deprecated (warnings start).</summary>
    public DateTimeOffset DeprecationDate { get; }

    /// <summary>Date when the contract is fully retired (requests rejected).</summary>
    public DateTimeOffset RetirementDate { get; }

    /// <summary>Initializes the schedule.</summary>
    /// <exception cref="ArgumentException">When <paramref name="retirementDate"/> is not after <paramref name="deprecationDate"/>.</exception>
    public DeprecationSchedule(DateTimeOffset deprecationDate, DateTimeOffset retirementDate)
    {
        if (retirementDate <= deprecationDate)
        {
            throw new ArgumentException(
                "RetirementDate must be strictly after DeprecationDate.",
                nameof(retirementDate)
            );
        }

        DeprecationDate = deprecationDate;
        RetirementDate = retirementDate;
    }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="now"/> is in the deprecation
    /// window (after deprecation date but before retirement date).
    /// </summary>
    public bool IsWithinDeprecationWindow(DateTimeOffset now) => now >= DeprecationDate && now < RetirementDate;

    /// <summary>Returns <c>true</c> when the contract has been retired.</summary>
    public bool IsRetired(DateTimeOffset now) => now >= RetirementDate;

    /// <summary>Returns <c>true</c> when the contract is still fully active.</summary>
    public bool IsActive(DateTimeOffset now) => now < DeprecationDate;
}
