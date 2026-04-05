namespace MarcusPrado.Platform.DataAccess.Tracing;

/// <summary>Shared ActivitySource for database tracing.</summary>
public static class DbActivitySource
{
    public const string Name = "MarcusPrado.Platform.DataAccess";
    public static readonly ActivitySource Instance = new(Name, "1.0.0");
}
