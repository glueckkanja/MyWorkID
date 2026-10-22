using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using MyWorkID.Server.Exceptions;
using MyWorkID.Server.Features.VerifiedId.Entities;
using MyWorkID.Server.Options;

namespace MyWorkID.Server.Features.VerifiedId
{
    /// <inheritdoc cref="IVerifiedIdRequirementService"/>
    public class VerifiedIdRequirementService : IVerifiedIdRequirementService
    {
        private readonly VerifiedIdRequirementOptions _requirementOptions;
        private readonly VerifiedIdOptions _verifiedIdOptions;
        private readonly GraphServiceClient _graphClient;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<VerifiedIdRequirementService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="VerifiedIdRequirementService"/> class.
        /// </summary>
        /// <param name="requirementOptions">The requirement options (enabled flag and validity).</param>
        /// <param name="verifiedIdOptions">The Verified ID options holding the target security attribute names.</param>
        /// <param name="graphClient">The Graph service client.</param>
        /// <param name="timeProvider">The time provider used to evaluate the validity window.</param>
        /// <param name="logger">The logger instance.</param>
        public VerifiedIdRequirementService(
            IOptions<VerifiedIdRequirementOptions> requirementOptions,
            IOptions<VerifiedIdOptions> verifiedIdOptions,
            GraphServiceClient graphClient,
            TimeProvider timeProvider,
            ILogger<VerifiedIdRequirementService> logger
        )
        {
            _requirementOptions = requirementOptions.Value;
            _verifiedIdOptions = verifiedIdOptions.Value;
            _graphClient = graphClient;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        /// <inheritdoc/>
        public bool IsEnabled => _requirementOptions.Enabled;

        /// <inheritdoc/>
        public TimeSpan Validity => _requirementOptions.Validity;

        /// <inheritdoc/>
        public async Task<VerifiedIdVerificationState> GetVerificationStateAsync(
            string userId,
            CancellationToken cancellationToken
        )
        {
            string attributeSetName = RequireSetting(
                _verifiedIdOptions.TargetSecurityAttributeSet,
                $"VerifiedId:{nameof(VerifiedIdOptions.TargetSecurityAttributeSet)}"
            );
            string attributeName = RequireSetting(
                _verifiedIdOptions.TargetSecurityAttribute,
                $"VerifiedId:{nameof(VerifiedIdOptions.TargetSecurityAttribute)}"
            );

            User? user = await _graphClient
                .Users[userId]
                .GetAsync(
                    requestConfiguration =>
                    {
                        requestConfiguration.QueryParameters.Select =
                        [
                            "id",
                            "customSecurityAttributes",
                        ];
                    },
                    cancellationToken
                );

            if (
                !LastVerificationTimestampParser.TryGetLastVerifiedAt(
                    user,
                    attributeSetName,
                    attributeName,
                    out DateTimeOffset lastVerifiedAt
                )
            )
            {
                _logger.LogDebug(
                    "No Verified ID verification timestamp found for user {UserId}.",
                    userId
                );
                return VerifiedIdVerificationState.NeverVerified;
            }

            return Evaluate(lastVerifiedAt);
        }

        /// <summary>
        /// Evaluates a verification timestamp against the configured validity window.
        /// </summary>
        /// <param name="lastVerifiedAt">Timestamp of the last successful verification.</param>
        /// <returns>The resulting verification state.</returns>
        public VerifiedIdVerificationState Evaluate(DateTimeOffset lastVerifiedAt)
        {
            DateTimeOffset now = _timeProvider.GetUtcNow();
            DateTimeOffset validUntil = lastVerifiedAt + Validity;
            // A timestamp in the future cannot be trusted (clock skew or manual tampering of the attribute).
            bool isValid = lastVerifiedAt <= now && now < validUntil;
            return new VerifiedIdVerificationState(lastVerifiedAt, validUntil, isValid);
        }

        private static string RequireSetting(string? value, string configurationKey)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ConfigurationMissingException(
                    configurationKey,
                    $"{Strings.ERROR_MISSING_OR_INVALID_SETTINGS_VERIFIED_ID_REQUIREMENT} Missing {configurationKey}."
                );
            }

            return value;
        }
    }
}
