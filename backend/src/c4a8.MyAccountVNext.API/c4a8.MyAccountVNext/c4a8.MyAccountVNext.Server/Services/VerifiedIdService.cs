
using c4a8.MyAccountVNext.Server.Models.VerifiedId;
using c4a8.MyAccountVNext.Server.Options;
using Microsoft.Extensions.Options;

namespace c4a8.MyAccountVNext.Server.Services
{
    public class VerifiedIdService
    {
        private readonly HttpClient _verifiedIdClient;
        private readonly VerifiedIdOptions _verifiedIdOptions;

        public VerifiedIdService(HttpClient verifiedIdClient, IOptions<VerifiedIdOptions> verifiedIdOptions)
        {
            _verifiedIdClient = verifiedIdClient;
            _verifiedIdOptions = verifiedIdOptions.Value;
        }

        public async Task<CreatePresentationResponse> CreatePresentationRequest()
        {
            RequestRegistration requestRegistration = new(clientName: "My Account VNext", purpose: "Verify your identity");
            Callback callback = new(url: $"{_verifiedIdOptions.BackendUrl}/api/verifiedid/callback", state: "08158312-7f5f-4229-97e9-45b1102ad9ed", headers: new Dictionary<string, string>() { { "api-key", "THISISOPTIONAL" } });

            FaceCheck faceCheck = new(sourcePhotoClaimName: "photo", matchConfidenceThreshold: 50);

            Validation validation = new(allowRevoked: false, validateLinkedDomain: true, faceCheck: faceCheck);

            List<RequestCredential> credentialList = new() {
                new RequestCredential(type: "VerifiedEmployee", purpose: "Verify users identity", acceptedIssuers: null, configuration: new Configuration(validation))
            };

            var request = new CreatePresentationRequest(authority: _verifiedIdOptions.DecentralizedIdentifier!, registration: requestRegistration, callback: callback, requestedCredentials: credentialList, includeQRCode: true, includeReceipt: false);

            var response = await _verifiedIdClient.PostAsJsonAsync("https://verifiedid.did.msidentity.com/v1.0/verifiableCredentials/createPresentationRequest", request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<CreatePresentationResponse>() ?? throw new Exception("Unexpected response");
        }
    }
}
