using System.Security.Cryptography;

namespace MarcusPrado.Platform.Security.Signatures;

public sealed class EcdsaSignatureService : ISignatureService, IDisposable
{
    private readonly ECDsa _ecdsa;

    public EcdsaSignatureService(ECDsa ecdsa) => _ecdsa = ecdsa;

    public static EcdsaSignatureService CreateEphemeral() =>
        new(ECDsa.Create(ECCurve.NamedCurves.nistP256));

    public string Sign(byte[] data)
    {
        var sig = _ecdsa.SignData(data, HashAlgorithmName.SHA256);
        return Convert.ToBase64String(sig);
    }

    public bool Verify(byte[] data, string signature)
    {
        try
        {
            var sig = Convert.FromBase64String(signature);
            return _ecdsa.VerifyData(data, sig, HashAlgorithmName.SHA256);
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

    public void Dispose() => _ecdsa.Dispose();
}
