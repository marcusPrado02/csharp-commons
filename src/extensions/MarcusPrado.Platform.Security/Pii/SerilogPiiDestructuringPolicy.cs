using Serilog.Core;
using Serilog.Events;

namespace MarcusPrado.Platform.Security.Pii;

public sealed class SerilogPiiDestructuringPolicy : IDestructuringPolicy
{
    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
    {
        var type = value.GetType();
        var piiProps = PiiClassifier.GetPiiProperties(type);

        if (piiProps.Count == 0)
        {
            result = null!;
            return false;
        }

        var redacted = PiiClassifier.Redact(value);
        var properties = redacted.Select(kv =>
            new LogEventProperty(kv.Key, propertyValueFactory.CreatePropertyValue(kv.Value)));

        result = new StructureValue(properties);
        return true;
    }
}
