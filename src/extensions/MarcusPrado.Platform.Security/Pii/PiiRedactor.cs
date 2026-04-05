using System.Text.RegularExpressions;

namespace MarcusPrado.Platform.Security.Pii;

public static partial class PiiRedactor
{
    // email: keep domain, mask local part → "jo***@example.com"
    [GeneratedRegex(@"^([^@]{1,2})[^@]*(@.+)$")]
    private static partial Regex EmailPattern();

    // Brazilian CPF: 000.000.000-00 → "***.***.<digits>-**"
    [GeneratedRegex(@"\d{3}\.\d{3}\.(\d{3})-\d{2}")]
    private static partial Regex CpfPattern();

    public static string MaskEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return "***";
        var m = EmailPattern().Match(email);
        return m.Success ? m.Groups[1].Value + "***" + m.Groups[2].Value : "***";
    }

    public static string MaskCpf(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return "***";
        return CpfPattern().Replace(cpf, m => $"***.***.*{m.Groups[1].Value.Substring(1)}-**");
    }

    public static string MaskPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return "***";
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.Length < 4) return "***";
        return "****-" + digits[^4..];
    }

    /// <summary>Generic mask: show first 2 chars + asterisks.</summary>
    public static string Mask(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "***";
        return value.Length <= 2
            ? new string('*', value.Length)
            : value[..2] + new string('*', Math.Min(value.Length - 2, 6));
    }

    public static string Redact(string? value, PiiType type) => type switch
    {
        PiiType.Email => MaskEmail(value),
        PiiType.Phone => MaskPhone(value),
        PiiType.TaxId => MaskCpf(value),
        _             => Mask(value),
    };
}
