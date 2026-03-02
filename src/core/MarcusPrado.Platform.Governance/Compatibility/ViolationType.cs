namespace MarcusPrado.Platform.Governance.Compatibility;

/// <summary>The kind of breaking change detected by the compatibility checker.</summary>
public enum ViolationType
{
    /// <summary>A field was removed from the schema.</summary>
    FieldRemoved,

    /// <summary>A field's type was changed in an incompatible way.</summary>
    TypeChanged,

    /// <summary>A previously optional field became required.</summary>
    RequiredAdded,
}
