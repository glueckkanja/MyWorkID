
using c4a8.MyAccountVNext.Server.Models.VerifiedId;
using c4a8.MyAccountVNext.Server.Options;
using Microsoft.Extensions.Options;
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

        public VerifiedIdService(HttpClient verifiedIdClient, IOptions<VerifiedIdOptions> verifiedIdOptions)
        {
            _verifiedIdClient = verifiedIdClient;
            _verifiedIdOptions = verifiedIdOptions.Value;
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
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<CreatePresentationResponse>() ?? throw new Exception("Unexpected response");
        }
    }
}
