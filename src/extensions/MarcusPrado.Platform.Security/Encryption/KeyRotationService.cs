namespace MarcusPrado.Platform.Security.Encryption;

public sealed class KeyRotationService : IDataEncryption
{
    private readonly Dictionary<int, AesGcmEncryption> _keys;
    private readonly int _currentVersion;

    public KeyRotationService(IReadOnlyDictionary<int, byte[]> keys, int currentVersion)
    {
        if (!keys.ContainsKey(currentVersion))
            throw new ArgumentException("currentVersion must be a key in keys", nameof(currentVersion));

        _keys = keys.ToDictionary(kv => kv.Key, kv => new AesGcmEncryption(kv.Value));
        _currentVersion = currentVersion;
    }

    public string Encrypt(string plaintext)
    {
        var encrypted = _keys[_currentVersion].Encrypt(plaintext);
        // Prefix with version: "v{n}:{base64}"
        return $"v{_currentVersion}:{encrypted}";
    }

    public string Decrypt(string ciphertext)
    {
        ArgumentNullException.ThrowIfNull(ciphertext);

        var colonIdx = ciphertext.IndexOf(':');
        if (colonIdx < 2 || ciphertext[0] != 'v')
            throw new FormatException("Ciphertext is missing version prefix.");

        var version = int.Parse(ciphertext[1..colonIdx], System.Globalization.CultureInfo.InvariantCulture);
        var data = ciphertext[(colonIdx + 1)..];

        if (!_keys.TryGetValue(version, out var enc))
            throw new KeyNotFoundException($"No key registered for version {version}.");

        return enc.Decrypt(data);
    }
}
