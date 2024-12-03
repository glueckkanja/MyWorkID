using Azure.Core;

namespace c4a8.MyWorkID.Server.Features.VerifiedId.HttpClients
{
    /// <summary>
    /// Service for managing and retrieving access tokens for Verified ID operations.
    /// </summary>
    public class VerifiedIdAccessTokenService
    {
        private AccessToken? _cachedAccessToken;
        private readonly TokenCredential _tokenCredential;

        /// <summary>
        /// Initializes a new instance of the <see cref="VerifiedIdAccessTokenService"/> class with the specified token credential.
        /// </summary>
        /// <param name="tokenCredential">The token credential used to acquire access tokens.</param>
        public VerifiedIdAccessTokenService(TokenCredential tokenCredential)
        {
            _tokenCredential = tokenCredential;
        }

        /// <summary>
        /// Retrieves an access token, using a cached token if it is still valid.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>The access token.</returns>
        public async Task<AccessToken> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            if (!_cachedAccessToken.HasValue || (_cachedAccessToken.Value.ExpiresOn - DateTimeOffset.Now).TotalMinutes < 5)
            {
                _cachedAccessToken = await _tokenCredential.GetTokenAsync(new TokenRequestContext(new[] { "3db474b9-6a0c-4840-96ac-1fceb342124f/.default" }), cancellationToken);
            }

            return _cachedAccessToken.Value;
        }
    }
}
