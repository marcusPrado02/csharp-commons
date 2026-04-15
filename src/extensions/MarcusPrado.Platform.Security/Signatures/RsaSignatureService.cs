using System.Security.Cryptography;

namespace MarcusPrado.Platform.Security.Signatures;

public sealed class RsaSignatureService : ISignatureService, IDisposable
{
    private readonly RSA _rsa;

    public RsaSignatureService(RSA rsa) => _rsa = rsa;

    /// <summary>Creates a service with a new ephemeral RSA-2048 key.</summary>
    public static RsaSignatureService CreateEphemeral() => new(RSA.Create(2048));

    public string Sign(byte[] data)
    {
        var sig = _rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
        return Convert.ToBase64String(sig);
    }

    public bool Verify(byte[] data, string signature)
    {
        try
        {
            var sig = Convert.FromBase64String(signature);
            return _rsa.VerifyData(data, sig, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
        }
        catch (FormatException)
        {
            return false;
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    public void Dispose() => _rsa.Dispose();
}
