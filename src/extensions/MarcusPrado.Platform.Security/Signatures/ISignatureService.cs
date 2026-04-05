namespace MarcusPrado.Platform.Security.Signatures;

public interface ISignatureService
{
    /// <summary>Signs the data and returns a base64-encoded signature.</summary>
    string Sign(byte[] data);

    /// <summary>Returns true if the signature is valid for the given data.</summary>
    bool Verify(byte[] data, string signature);
}
