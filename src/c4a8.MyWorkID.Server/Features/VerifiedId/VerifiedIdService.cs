using c4a8.MyWorkID.Server.Features.VerifiedId.Entities;
using c4a8.MyWorkID.Server.Features.VerifiedId.Exceptions;
using c4a8.MyWorkID.Server.Features.VerifiedId.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.Text.Json;

namespace c4a8.MyWorkID.Server.Features.VerifiedId
{
    public class VerifiedIdService
    {
        private readonly HttpClient _verifiedIdClient;
        private readonly VerifiedIdOptions _verifiedIdOptions;
        private readonly GraphServiceClient _graphClient;
        private readonly IVerifiedIdSignalRRepository _verifiedIdSignalRRepository;
        private readonly IHubContext<VerifiedIdHub, IVerifiedIdHub> _hubContext;
        private readonly ILogger _logger;

        public VerifiedIdService(
            HttpClient verifiedIdClient,
            IOptions<VerifiedIdOptions> verifiedIdOptions,
            GraphServiceClient graphClient,
            IVerifiedIdSignalRRepository verifiedIdSignalRRepository,
            IHubContext<VerifiedIdHub, IVerifiedIdHub> hubContext,
            ILogger<VerifiedIdService> logger)
        {
            _verifiedIdClient = verifiedIdClient;
            _verifiedIdOptions = verifiedIdOptions.Value;
            _graphClient = graphClient;
            _verifiedIdSignalRRepository = verifiedIdSignalRRepository;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<CreatePresentationResponse?> CreatePresentationRequest(string userId)
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

            HttpResponseMessage? response = null;
            try
            {
                response = await _verifiedIdClient.PostAsJsonAsync(_verifiedIdOptions.CreatePresentationRequestUri, request);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                if (response != null)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError(e, "Failed to create presentation request. Response: {0}", responseContent);
                }
                else
                {
                    _logger.LogError(e, "Failed to create presentation request. No response received.");
                }
                throw new CreatePresentationException();
            }
            var createPresentationResponse = await response.Content.ReadFromJsonAsync<CreatePresentationResponse>();
            if (createPresentationResponse == null)
            {
                _logger.LogError("Failed to create presentation request. Parsed response is null.");
                throw new CreatePresentationException();
            }
            return createPresentationResponse;
        }
        public async Task HideQrCodeForUser(string userId)
        {
            if (!_verifiedIdOptions.DisableQrCodeHide && _verifiedIdSignalRRepository.TryGetConnections(userId, out var connections))
            {
                await _hubContext.Clients.Clients(connections).HideQrCode();
            }
        }

        public async Task<CreatePresentationRequestCallback> ParseCreatePresentationRequestCallback(HttpContext context)
        {
            using StreamReader streamReader = new StreamReader(context.Request.Body);
            var callbackBody = await streamReader.ReadToEndAsync();

            CreatePresentationRequestCallback? parsedBody = null;

            try
            {
                parsedBody = JsonSerializer.Deserialize<CreatePresentationRequestCallback>(callbackBody);
            }
            catch (Exception e) when (e is JsonException || e is ArgumentNullException)
            {
                _logger.LogError(e, "CallbackBody: {0}", callbackBody);
                throw new CreatePresentationException();
            }

            if (parsedBody == null)
            {
                _logger.LogWarning("Parsed presentation callback is null. CallbackBody: {0}", callbackBody);
                throw new CreatePresentationException();
            }

            return parsedBody;
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
                    throw new PresentationCallbackException($"Invalid state. Expected {userId} but state is {callbackBody.State}.");
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
