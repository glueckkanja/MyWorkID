using System.Net;

namespace c4a8.MyWorkID.Server.Features.VerifiedId.HttpClients
{
    /// <summary>
    /// A message handler that adds authentication headers to HTTP requests for Verified ID operations.
    /// </summary>
    public class VerifiedIdAuthenticationHandler : DelegatingHandler
    {
        private readonly VerifiedIdAccessTokenService _verifiedIdAccessTokenService;

        /// <summary>
        /// Initializes a new instance of the <see cref="VerifiedIdAuthenticationHandler"/> class with the specified access token service.
        /// </summary>
        /// <param name="verifiedIdAccessTokenService">The service used to retrieve access tokens.</param>
        public VerifiedIdAuthenticationHandler(VerifiedIdAccessTokenService verifiedIdAccessTokenService)
        {
            _verifiedIdAccessTokenService = verifiedIdAccessTokenService;
        }

        /// <summary>
        /// Sends an HTTP request with an authorization header if it is not already present.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>The HTTP response message.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!request.Headers.Contains(HttpRequestHeader.Authorization.ToString()))
            {
                var token = await _verifiedIdAccessTokenService.GetAccessTokenAsync(cancellationToken);
                request.Headers.Add(HttpRequestHeader.Authorization.ToString(), $"Bearer {token.Token}");
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
