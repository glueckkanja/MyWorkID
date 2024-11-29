using Azure.Core;

namespace c4a8.MyWorkID.Server.Features.VerifiedId.HttpClients
{
    public class VerifiedIdAccessTokenService
    {
        private AccessToken? cachedAccessToken;
        private readonly TokenCredential _tokenCredential;

        public VerifiedIdAccessTokenService(TokenCredential tokenCredential)
        {
            _tokenCredential = tokenCredential;
        }

        public async Task<AccessToken> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            if (!cachedAccessToken.HasValue || (cachedAccessToken.Value.ExpiresOn - DateTimeOffset.Now).TotalMinutes < 5)
            {
                cachedAccessToken = await _tokenCredential.GetTokenAsync(new TokenRequestContext(new[] { "3db474b9-6a0c-4840-96ac-1fceb342124f/.default" }), cancellationToken);
            }

            return cachedAccessToken.Value;
        }

    }
}
