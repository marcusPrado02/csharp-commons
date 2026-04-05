using MarcusPrado.Platform.Contracts.Async;
using MarcusPrado.Platform.Domain.Events;

namespace MarcusPrado.Platform.EventRouting.Bridge;

/// <summary>
/// Converts an <see cref="IDomainEvent"/> into an <see cref="IEventContract"/> so that
/// it can be published across service boundaries (e.g. via a message broker).
/// </summary>
public sealed class CrossBoundaryEventBridge
{
    private readonly Dictionary<Type, Func<IDomainEvent, IEventContract>> _converters = [];

    /// <summary>
    /// Registers a converter that maps a specific domain-event type to an event contract.
    /// </summary>
    /// <typeparam name="TEvent">The source domain event type.</typeparam>
    /// <param name="converter">Function that produces an <see cref="IEventContract"/> from a <typeparamref name="TEvent"/>.</param>
    /// <returns>The current <see cref="CrossBoundaryEventBridge"/> for fluent chaining.</returns>
    public CrossBoundaryEventBridge Register<TEvent>(Func<TEvent, IEventContract> converter)
        where TEvent : IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(converter);
        _converters[typeof(TEvent)] = e => converter((TEvent)e);
        return this;
    }

    /// <summary>
    /// Attempts to convert <paramref name="domainEvent"/> into an <see cref="IEventContract"/>.
    /// </summary>
    /// <param name="domainEvent">The domain event to convert.</param>
    /// <param name="contract">
    /// When this method returns <see langword="true"/>, contains the produced contract;
    /// otherwise <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a converter was registered for the event type;
    /// <see langword="false"/> otherwise.
    /// </returns>
    public bool TryConvert(IDomainEvent domainEvent, out IEventContract? contract)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        if (_converters.TryGetValue(domainEvent.GetType(), out var converter))
        {
            contract = converter(domainEvent);
            return true;
        }

        contract = null;
        return false;
    }

    /// <summary>
    /// Converts <paramref name="domainEvent"/> into an <see cref="IEventContract"/>.
    /// </summary>
    /// <param name="domainEvent">The domain event to convert.</param>
    /// <returns>The converted event contract.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no converter has been registered for the event type.
    /// </exception>
    public IEventContract Convert(IDomainEvent domainEvent)
    {
        if (!TryConvert(domainEvent, out var contract))
        {
            throw new InvalidOperationException(
                $"No cross-boundary converter registered for domain event type '{domainEvent.GetType().FullName}'.");
        }

        return contract!;
    }

    /// <summary>Returns <see langword="true"/> if a converter exists for <typeparamref name="TEvent"/>.</summary>
    /// <typeparam name="TEvent">The event type to check.</typeparam>
    public bool HasConverter<TEvent>()
        where TEvent : IDomainEvent =>
        _converters.ContainsKey(typeof(TEvent));
}
