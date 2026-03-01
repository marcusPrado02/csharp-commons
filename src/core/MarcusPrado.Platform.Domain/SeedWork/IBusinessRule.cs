namespace MarcusPrado.Platform.Domain.SeedWork;

/// <summary>
/// Represents a single invariant or business rule that an aggregate enforces.
/// Implement this interface and call <see cref="Entity{TId}.CheckRule"/> (or a
/// similar guard) inside command methods to keep the aggregate always valid.
/// </summary>
/// <example>
/// <code>
/// internal sealed class OrderMustHaveAtLeastOneItem : IBusinessRule
/// {
///     private readonly int _itemCount;
///     public OrderMustHaveAtLeastOneItem(int itemCount) => _itemCount = itemCount;
///     public bool IsBroken() => _itemCount == 0;
///     public string Message => "An order must contain at least one item.";
/// }
/// </code>
/// </example>
public interface IBusinessRule
{
    /// <summary>Returns <c>true</c> when the rule is violated.</summary>
    bool IsBroken();

    /// <summary>Human-readable description of the violated invariant.</summary>
    string Message { get; }
}
