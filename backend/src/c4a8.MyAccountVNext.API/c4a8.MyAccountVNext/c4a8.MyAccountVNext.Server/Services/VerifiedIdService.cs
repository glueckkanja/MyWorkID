
using c4a8.MyAccountVNext.Server.Models.VerifiedId;
using c4a8.MyAccountVNext.Server.Options;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace c4a8.MyAccountVNext.Server.Services
{
    public class VerifiedIdService
    {
        private readonly HttpClient _verifiedIdClient;
        private readonly VerifiedIdOptions _verifiedIdOptions;
        private readonly GraphServiceClient _graphClient;
        private readonly ILogger _logger;

        public VerifiedIdService(HttpClient verifiedIdClient, IOptions<VerifiedIdOptions> verifiedIdOptions, GraphServiceClient graphClient, ILogger<VerifiedIdService> logger)
        {
            _verifiedIdClient = verifiedIdClient;
            _verifiedIdOptions = verifiedIdOptions.Value;
            _graphClient = graphClient;
            _logger = logger;
        }

        private string GenerateToken(string userId)
        {
            // generate token that is valid for 30 minutes
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_verifiedIdOptions.JwtSigningKey ?? "GodDamnitYouForgottToSpecifyASigningKey");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("userId", userId) }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<CreatePresentationResponse> CreatePresentationRequest(string userId)
        {
            RequestRegistration requestRegistration = new(clientName: "My Account VNext", purpose: "Verify your identity");
            Callback callback = new(url: $"{_verifiedIdOptions.BackendUrl}/api/verifiedid/callback", state: userId, headers: new Dictionary<string, string>() { { "Authorization", $"Bearer {GenerateToken(userId)}" } });

            FaceCheck faceCheck = new(sourcePhotoClaimName: "photo", matchConfidenceThreshold: 50);

            Validation validation = new(allowRevoked: false, validateLinkedDomain: true, faceCheck: faceCheck);

            List<RequestCredential> credentialList = new() {
                new RequestCredential(type: "VerifiedEmployee", purpose: "Verify users identity", acceptedIssuers: null, configuration: new Configuration(validation))
            };

            var request = new CreatePresentationRequest(authority: _verifiedIdOptions.DecentralizedIdentifier!, registration: requestRegistration, callback: callback, requestedCredentials: credentialList, includeQRCode: true, includeReceipt: false);

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

                var requestBody = new User
                {
                    CustomSecurityAttributes = new CustomSecurityAttributeValue
                    {
                        AdditionalData = new Dictionary<string, object>
                        {
                            {
                                "myAccountVNext" , new CustomSecurityAttributeValue()
                                {
                                    OdataType = "#Microsoft.DirectoryServices.CustomSecurityAttributeValue",
                                    AdditionalData = new Dictionary<string, object>
                                    {
                                        { "lastVerifiedFaceCheck", DateTime.UtcNow.ToString("O") }
                                    }
                                }
                            },
                        },
                    },
                };

                await _graphClient.Users[userId].PatchAsync(requestBody);
            }

        }
    }
}
