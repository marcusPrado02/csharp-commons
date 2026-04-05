namespace MarcusPrado.Platform.AspNetCore.Sanitization;

/// <summary>Sanitizes untrusted user input.</summary>
public interface IInputSanitizer
{
    /// <summary>Strips or encodes dangerous HTML/script content from the input.</summary>
    string SanitizeHtml(string input);

    /// <summary>Removes all HTML tags, returning plain text.</summary>
    string StripHtml(string input);
}
