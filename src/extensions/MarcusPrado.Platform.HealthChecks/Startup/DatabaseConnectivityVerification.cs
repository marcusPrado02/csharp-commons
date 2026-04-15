namespace MarcusPrado.Platform.HealthChecks.Startup;

/// <summary>
/// Verifies database connectivity by executing a caller-supplied probe function.
/// No hard dependency on Entity Framework or any specific database driver.
/// </summary>
public sealed class DatabaseConnectivityVerification : IStartupVerification
{
    private readonly Func<Task<bool>> _probe;

    /// <summary>Initialises the verification with a connectivity probe.</summary>
    /// <param name="name">The name to identify this verification.</param>
    /// <param name="probe">A function that returns <c>true</c> when the database is reachable.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="probe"/> is null.</exception>
    public DatabaseConnectivityVerification(string name, Func<Task<bool>> probe)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name must not be null or whitespace.", nameof(name));
        ArgumentNullException.ThrowIfNull(probe);

        Name = name;
        _probe = probe;
    }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public async Task<VerificationResult> VerifyAsync(CancellationToken ct)
    {
        try
        {
            var reachable = await _probe().WaitAsync(ct);
            return reachable
                ? new VerificationResult(true, Name, null)
                : new VerificationResult(false, Name, "Database probe returned false.");
        }
        catch (OperationCanceledException ex)
        {
            return new VerificationResult(false, Name, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return new VerificationResult(false, Name, ex.Message);
        }
        catch (TimeoutException ex)
        {
            return new VerificationResult(false, Name, ex.Message);
        }
    }
}
