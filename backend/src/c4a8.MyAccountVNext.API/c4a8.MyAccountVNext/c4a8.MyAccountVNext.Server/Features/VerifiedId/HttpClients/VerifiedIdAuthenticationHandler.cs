using System.Net;

namespace c4a8.MyAccountVNext.Server.Features.VerifiedId.HttpClients
{
    public class VerifiedIdAuthenticationHandler : DelegatingHandler
    {
        private readonly VerifiedIdAccessTokenService _verifiedIdAccessTokenService;
        public VerifiedIdAuthenticationHandler(VerifiedIdAccessTokenService verifiedIdAccessTokenService)
        {
            _verifiedIdAccessTokenService = verifiedIdAccessTokenService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!request.Headers.Contains(HttpRequestHeader.Authorization.ToString()))
            {
                var token = await _verifiedIdAccessTokenService.GetAccessTokenAsync();
                request.Headers.Add(HttpRequestHeader.Authorization.ToString(), $"Bearer {token.Token}");
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
