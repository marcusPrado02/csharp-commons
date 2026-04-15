using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MarcusPrado.Platform.Security.Encryption;

public sealed class EncryptingValueConverter : ValueConverter<string, string>
{
    public EncryptingValueConverter(IDataEncryption encryption)
        : base(v => encryption.Encrypt(v), v => encryption.Decrypt(v)) { }
}
