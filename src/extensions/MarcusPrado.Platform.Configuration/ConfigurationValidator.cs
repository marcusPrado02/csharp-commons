namespace MarcusPrado.Platform.Configuration;

/// <summary>
/// Validates options before applying them. Throws <see cref="OptionsValidationException"/> if any validator fails.
/// </summary>
/// <typeparam name="T">The options type to validate.</typeparam>
public sealed class ConfigurationValidator<T>
    where T : class
{
    private readonly List<Action<T>> _validators = new();

    /// <summary>
    /// Adds a validation rule. The action should throw an exception if the value is invalid.
    /// </summary>
    /// <param name="validator">An action that validates the options, throwing on failure.</param>
    /// <returns>This <see cref="ConfigurationValidator{T}"/> for chaining.</returns>
    public ConfigurationValidator<T> AddValidator(Action<T> validator)
    {
        ArgumentNullException.ThrowIfNull(validator);
        _validators.Add(validator);
        return this;
    }

    /// <summary>
    /// Validates the given options value against all registered validators.
    /// </summary>
    /// <param name="options">The options instance to validate.</param>
    /// <exception cref="OptionsValidationException">Thrown if any validator fails.</exception>
    public void Validate(T options)
    {
        ArgumentNullException.ThrowIfNull(options);

        foreach (var validator in _validators)
        {
            try
            {
                validator(options);
            }
            catch (Exception ex) when (ex is not OptionsValidationException)
            {
                throw new OptionsValidationException(
                    typeof(T).Name,
                    $"Validation failed for options type '{typeof(T).Name}': {ex.Message}",
                    ex);
            }
        }
    }
}
