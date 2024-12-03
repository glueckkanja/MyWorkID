using Azure.Core;

namespace c4a8.MyWorkID.Server.Features.VerifiedId.HttpClients
{
    public class VerifiedIdAccessTokenService
    {
        private AccessToken? _cachedAccessToken;
        private readonly TokenCredential _tokenCredential;

        public VerifiedIdAccessTokenService(TokenCredential tokenCredential)
        {
            _tokenCredential = tokenCredential;
        }

        public async Task<AccessToken> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            if (!_cachedAccessToken.HasValue || (_cachedAccessToken.Value.ExpiresOn - DateTimeOffset.Now).TotalMinutes < 5)
            {
                _cachedAccessToken = await _tokenCredential.GetTokenAsync(new TokenRequestContext(["3db474b9-6a0c-4840-96ac-1fceb342124f/.default"]), cancellationToken);
            }

            return _cachedAccessToken.Value;
        }

    }
}
