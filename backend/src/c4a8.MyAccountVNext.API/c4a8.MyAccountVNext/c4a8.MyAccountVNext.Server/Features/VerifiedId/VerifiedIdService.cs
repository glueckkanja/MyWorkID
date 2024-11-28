using c4a8.MyAccountVNext.Server.Features.VerifiedId.Entities;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace c4a8.MyAccountVNext.Server.Features.VerifiedId
{
    public class VerifiedIdService
    {
        private readonly HttpClient _verifiedIdClient;
        private readonly VerifiedIdOptions _verifiedIdOptions;
        private readonly GraphServiceClient _graphClient;
        private readonly ILogger _logger;

        public VerifiedIdService(
            HttpClient verifiedIdClient,
            IOptions<VerifiedIdOptions> verifiedIdOptions,
            GraphServiceClient graphClient,
            ILogger<VerifiedIdService> logger)
        {
            _verifiedIdClient = verifiedIdClient;
            _verifiedIdOptions = verifiedIdOptions.Value;
            _graphClient = graphClient;
            _logger = logger;
        }

        public async Task<CreatePresentationResponse> CreatePresentationRequest(string userId)
        {
            RequestRegistration requestRegistration = new(clientName: "MyWorkID", purpose: "Verify your identity");
            var jwtSigningKey = _verifiedIdOptions.JwtSigningKey ?? Strings.JWT_SIGNING_KEY_DEFAULT;
            Callback callback = new(
                url: $"{_verifiedIdOptions.BackendUrl}/api/me/verifiedid/callback",
                state: userId,
                headers: new Dictionary<string, string>() { { "Authorization", $"Bearer {JwtTokenProvider.GenerateToken(userId, jwtSigningKey)}" } });

            FaceCheck faceCheck = new(sourcePhotoClaimName: "photo", matchConfidenceThreshold: 50);

            Validation validation = new(allowRevoked: false, validateLinkedDomain: true, faceCheck: faceCheck);

            List<RequestCredential> credentialList = new() {
                new RequestCredential(
                    type: "VerifiedEmployee",
                    purpose: "Verify users identity",
                    acceptedIssuers: null,
                    configuration: new Entities.Configuration(validation))
            };

            var request = new CreatePresentationRequest(
                authority: _verifiedIdOptions.DecentralizedIdentifier!,
                registration: requestRegistration,
                callback: callback,
                requestedCredentials: credentialList,
                includeQRCode: true,
                includeReceipt: false);

            var response = await _verifiedIdClient.PostAsJsonAsync("https://verifiedid.did.msidentity.com/v1.0/verifiableCredentials/createPresentationRequest", request);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create presentation request");
                _logger.LogError($"Reponse was: {await response.Content.ReadAsStringAsync()}");
                throw;
            }

            return await response.Content.ReadFromJsonAsync<CreatePresentationResponse>() ?? throw new Exception("Unexpected response");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="callbackBody"></param>
        /// <returns>Indicates success</returns>
        public async Task HandlePresentationCallback(string userId, CreatePresentationRequestCallback callbackBody)
        {
            if (callbackBody.RequestStatus == "request_retrieved")
            {
                return;
            }

            if (callbackBody.RequestStatus == "presentation_error" || callbackBody.Error != null)
            {
                return;
            }

            if (callbackBody.RequestStatus == "presentation_verified")
            {
                if (!string.Equals(callbackBody.State, userId, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("invalid state");
                }

                if (string.IsNullOrWhiteSpace(_verifiedIdOptions.TargetSecurityAttributeSet) || string.IsNullOrWhiteSpace(_verifiedIdOptions.TargetSecurityAttribute))
                {
                    return;
                }

                User requestBody = CreateSetTargetSecurityAttributeRequestBody(DateTime.UtcNow.ToString("O"));

                await _graphClient.Users[userId].PatchAsync(requestBody);
            }

        }

        public User CreateSetTargetSecurityAttributeRequestBody(string targetSecurityAttributeValue)
        {
            return new User
            {
                CustomSecurityAttributes = new CustomSecurityAttributeValue
                {
                    AdditionalData = new Dictionary<string, object>
                        {
                            {
                                _verifiedIdOptions.TargetSecurityAttributeSet! , new CustomSecurityAttributeValue()
                                {
                                    OdataType = "#Microsoft.DirectoryServices.CustomSecurityAttributeValue",
                                    AdditionalData = new Dictionary<string, object>
                                    {
                                        { _verifiedIdOptions.TargetSecurityAttribute!, targetSecurityAttributeValue }
                                    }
                                }
                            },
                        },
                },
            };
        }
    }
}
