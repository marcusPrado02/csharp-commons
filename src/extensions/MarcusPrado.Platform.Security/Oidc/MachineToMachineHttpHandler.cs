namespace MarcusPrado.Platform.Security.Oidc;

public sealed class MachineToMachineHttpHandler : DelegatingHandler
{
    private readonly IOidcClientService _oidc;

    public MachineToMachineHttpHandler(IOidcClientService oidc) => _oidc = oidc;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var token = await _oidc.GetAccessTokenAsync(cancellationToken);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}
