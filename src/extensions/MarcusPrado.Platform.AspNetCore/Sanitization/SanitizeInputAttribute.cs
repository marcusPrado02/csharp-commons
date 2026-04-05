namespace MarcusPrado.Platform.AspNetCore.Sanitization;

/// <summary>Marks a string parameter or property for automatic input sanitization.</summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public sealed class SanitizeInputAttribute : Attribute
{
    /// <summary>When true, strips all HTML. When false (default), sanitizes (allows safe HTML).</summary>
    public bool StripAll { get; set; }
}
