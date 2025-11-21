using System.Net;
using System.Net.Http.Headers;

namespace PayHelp.Client;

public sealed class AuthenticatedHttpMessageHandler : DelegatingHandler
{
    private readonly ITokenStore _tokenStore;

    public AuthenticatedHttpMessageHandler(ITokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Add Bearer token if available
        var token = await _tokenStore.GetAsync().ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(token) && request.Headers.Authorization is null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
        {
            await _tokenStore.ClearAsync().ConfigureAwait(false);
            // Let UI handle redirect to Login
            throw new ApiUnauthorizedException(statusCode: (int)response.StatusCode);
        }

        return response;
    }
}
