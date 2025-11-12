using System.Net.Http.Headers;
using PayHelp.Mobile.Maui.Utilities;

namespace PayHelp.Mobile.Maui.Services;

public class AuthorizedHttpMessageHandler : DelegatingHandler
{
    public AuthorizedHttpMessageHandler()
    {
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await AppSettings.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return await base.SendAsync(request, cancellationToken);
    }
}
