using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using MyWorkID.Server.Features.VerifiedId.Entities;
using MyWorkID.Server.Features.VerifiedId.Exceptions;
using MyWorkID.Server.Features.VerifiedId.SignalR;
using MyWorkID.Server.Options;
using System.Text.Json;

namespace MyWorkID.Server.Features.VerifiedId
{
    /// <summary>
    /// Service for handling Verified ID operations.
    /// </summary>
    public class VerifiedIdService
    {
        private readonly HttpClient _verifiedIdClient;
        private readonly VerifiedIdOptions _verifiedIdOptions;
        private readonly GraphServiceClient _graphClient;
        private readonly IVerifiedIdSignalRRepository _verifiedIdSignalRRepository;
        private readonly IHubContext<VerifiedIdHub, IVerifiedIdHub> _hubContext;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="VerifiedIdService"/> class.
        /// </summary>
        /// <param name="verifiedIdClient">The HTTP client for Verified ID operations.</param>
        /// <param name="verifiedIdOptions">The options for Verified ID operations.</param>
        /// <param name="graphClient">The Graph service client.</param>
        /// <param name="verifiedIdSignalRRepository">The SignalR repository for Verified ID operations.</param>
        /// <param name="hubContext">The SignalR hub context.</param>
        /// <param name="logger">The logger instance.</param>
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

        /// <summary>
        /// Creates a presentation request for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The response of the create presentation request.</returns>
        /// <exception cref="CreatePresentationException">Thrown when the presentation request fails.</exception>
        public async Task<CreatePresentationResponse?> CreatePresentationRequest(string userId, CancellationToken cancellationToken)
        {
            RequestRegistration requestRegistration = new(clientName: "MyWorkID", purpose: "Verify your identity");
            var jwtSigningKey = _verifiedIdOptions.JwtSigningKey!;
            Callback callback = new(
                url: $"{_verifiedIdOptions.BackendUrl}/api/me/verifiedid/callback",
                state: userId,
                headers: new Dictionary<string, string>() { { "Authorization", $"Bearer {JwtTokenProvider.GenerateToken(userId, jwtSigningKey)}" } });

            FaceCheck faceCheck = new(sourcePhotoClaimName: "photo", matchConfidenceThreshold: 50);

            Entities.Validation validation = new(allowRevoked: false, validateLinkedDomain: true, faceCheck: faceCheck);

            List<RequestCredential> credentialList = new()
            {
                new RequestCredential(
                    type: "VerifiedEmployee",
                    purpose: "Verify users identity",
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
                response = await _verifiedIdClient.PostAsJsonAsync(_verifiedIdOptions.CreatePresentationRequestUri, request, cancellationToken);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                var responseContent = await response!.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(e, "Failed to create presentation request. Response: {ResponseContent}", responseContent);
                if (responseContent.Contains(Strings.GRAPH_VERIFIED_ID_LICENSE_ERROR_MESSAGE, StringComparison.OrdinalIgnoreCase))
                {
                    throw new PremiumFeatureBillingMissingException(responseContent, e);
                }
                throw new CreatePresentationException(e.Message, e);
            }
            var createPresentationResponse = await response.Content.ReadFromJsonAsync<CreatePresentationResponse>(cancellationToken);
            if (createPresentationResponse == null)
            {
                _logger.LogError("Failed to create presentation request. Parsed response is null.");
                throw new CreatePresentationException();
            }
            return createPresentationResponse;
        }

        /// <summary>
        /// Hides the QR code for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        public async Task HideQrCodeForUser(string userId)
        {
            if (!_verifiedIdOptions.DisableQrCodeHide && _verifiedIdSignalRRepository.TryGetConnections(userId, out var connections))
            {
                await _hubContext.Clients.Clients(connections).HideQrCode();
            }
        }

        /// <summary>
        /// Parses the create presentation request callback from the HTTP context.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>The parsed create presentation request callback.</returns>
        /// <exception cref="CreatePresentationException">Thrown when the callback parsing fails.</exception>
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
                _logger.LogError(e, "{CallbackBody}", callbackBody);
                throw new CreatePresentationException();
            }

            if (parsedBody == null)
            {
                _logger.LogWarning("Parsed presentation callback is null. {CallbackBody}", callbackBody);
                throw new CreatePresentationException();
            }

            return parsedBody;
        }

        /// <summary>
        /// Handles the presentation callback for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="callbackBody">The callback body.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="PresentationCallbackException">Thrown when the callback handling fails.</exception>
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

                User requestBody = CreateSetTargetSecurityAttributeRequestBody(DateTime.UtcNow.ToString("O"));

                await _graphClient.Users[userId].PatchAsync(requestBody);
            }
        }

        /// <summary>
        /// Creates the request body for setting the target security attribute.
        /// </summary>
        /// <param name="targetSecurityAttributeValue">The value of the target security attribute.</param>
        /// <returns>The user object with the target security attribute set.</returns>
        public User CreateSetTargetSecurityAttributeRequestBody(string targetSecurityAttributeValue)
        {
            return new User
            {
                CustomSecurityAttributes = new CustomSecurityAttributeValue
                {
                    AdditionalData = new Dictionary<string, object>
                        {
                            {
                                _verifiedIdOptions.TargetSecurityAttributeSet!, new CustomSecurityAttributeValue()
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
