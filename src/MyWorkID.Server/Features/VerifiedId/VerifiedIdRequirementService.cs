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
        /// <summary>
        /// How long a claim or a newly recorded verification waits before it reads the attribute back. Longer than
        /// the time a concurrent request needs between reading the verification and storing its own claim or discard.
        /// </summary>
        public static readonly TimeSpan CLAIM_SETTLE_DELAY = TimeSpan.FromSeconds(2);

        /// <summary>
        /// How long a claim waits before reading the attribute again while the directory still returns the claimed
        /// verification. Entra ID can take a few seconds until a freshly written attribute is readable.
        /// </summary>
        public static readonly TimeSpan CLAIM_READ_RETRY_INTERVAL = TimeSpan.FromSeconds(1);

        /// <summary>
        /// How often a claim reads the attribute back at most before it gives up waiting for its own write.
        /// </summary>
        public const int CLAIM_READ_MAXIMUM_ATTEMPTS = 10;

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
        public int ValidityInMinutes => _requirementOptions.ValidityInMinutes;

        /// <inheritdoc/>
        public bool RequiresVerificationPerAction =>
            _requirementOptions.RequiresVerificationPerAction;

        /// <inheritdoc/>
        public async Task<VerifiedIdVerificationState> GetVerificationStateAsync(
            string userId,
            CancellationToken cancellationToken
        )
        {
            (string attributeSetName, string attributeName) = GetTargetSecurityAttribute();

            User? user = await GetUserWithCustomSecurityAttributesAsync(userId, cancellationToken);

            if (
                !LastVerificationTimestampParser.TryGetLastVerification(
                    user,
                    attributeSetName,
                    attributeName,
                    out DateTimeOffset lastVerifiedAt,
                    out VerifiedIdAction? verifiedForAction
                )
            )
            {
                _logger.LogDebug(
                    "No Verified ID verification timestamp found for user {UserId}.",
                    userId
                );
                return VerifiedIdVerificationState.NeverVerified;
            }

            return Evaluate(lastVerifiedAt, verifiedForAction);
        }

        /// <summary>
        /// Evaluates a verification timestamp against the configured validity window.
        /// </summary>
        /// <param name="lastVerifiedAt">Timestamp of the last successful verification.</param>
        /// <param name="verifiedForAction">The action the verification is bound to, if any.</param>
        /// <returns>The resulting verification state.</returns>
        public VerifiedIdVerificationState Evaluate(
            DateTimeOffset lastVerifiedAt,
            VerifiedIdAction? verifiedForAction = null
        )
        {
            DateTimeOffset now = _timeProvider.GetUtcNow();
            DateTimeOffset validUntil = lastVerifiedAt + Validity;
            // A timestamp in the future cannot be trusted (clock skew or manual tampering of the attribute).
            bool isValid = lastVerifiedAt <= now && now < validUntil;
            return new VerifiedIdVerificationState(
                lastVerifiedAt,
                validUntil,
                isValid,
                verifiedForAction
            );
        }

        /// <inheritdoc/>
        public async Task MarkVerificationAsUsedAsync(
            string userId,
            CancellationToken cancellationToken
        )
        {
            await WriteTargetSecurityAttributeAsync(
                userId,
                VerificationAttributeValue.CreateUsed(_timeProvider.GetUtcNow()),
                cancellationToken
            );
        }

        /// <inheritdoc/>
        public async Task<VerifiedIdVerificationClaim?> TryClaimVerificationAsync(
            string userId,
            VerifiedIdVerificationState verificationState,
            CancellationToken cancellationToken
        )
        {
            if (!verificationState.LastVerifiedAt.HasValue)
            {
                return null;
            }

            (string attributeSetName, string attributeName) = GetTargetSecurityAttribute();
            string claimedValue = VerificationAttributeValue.CreateClaimed(_timeProvider.GetUtcNow());
            await WriteTargetSecurityAttributeAsync(userId, claimedValue, cancellationToken);

            // Graph has no conditional updates, so concurrent claims overwrite each other and the last write wins.
            // Waiting before reading the attribute back lets every claim of a request that read the verification
            // before this claim was stored land first, so exactly one of the concurrent requests reads its own claim.
            await Task.Delay(CLAIM_SETTLE_DELAY, _timeProvider, cancellationToken);

            // A read that still returns the claimed verification has not caught up with the claim yet. Only another
            // value proves that a concurrent request overwrote it, so such a read is repeated for a while instead of
            // giving up the verification that was already marked as used.
            string? storedValue = null;
            for (int readAttempt = 1; readAttempt <= CLAIM_READ_MAXIMUM_ATTEMPTS; readAttempt++)
            {
                if (readAttempt > 1)
                {
                    await Task.Delay(CLAIM_READ_RETRY_INTERVAL, _timeProvider, cancellationToken);
                }

                User? user = await GetUserWithCustomSecurityAttributesAsync(userId, cancellationToken);
                storedValue = LastVerificationTimestampParser.GetRawAttributeValue(
                    user,
                    attributeSetName,
                    attributeName
                );
                if (!IsClaimedVerificationStillStored(user, attributeSetName, attributeName, verificationState))
                {
                    break;
                }
            }

            if (!string.Equals(storedValue, claimedValue, StringComparison.Ordinal))
            {
                _logger.LogWarning(
                    "The Verified ID verification of user {UserId} was claimed by a concurrent request.",
                    userId
                );
                return null;
            }

            return new VerifiedIdVerificationClaim(
                userId,
                claimedValue,
                VerificationAttributeValue.Create(
                    verificationState.LastVerifiedAt.Value,
                    verificationState.VerifiedForAction
                )
            );
        }

        /// <inheritdoc/>
        public async Task ReleaseVerificationClaimAsync(
            VerifiedIdVerificationClaim claim,
            CancellationToken cancellationToken
        )
        {
            (string attributeSetName, string attributeName) = GetTargetSecurityAttribute();
            string? storedValue = await ReadTargetSecurityAttributeAsync(
                claim.UserId,
                attributeSetName,
                attributeName,
                cancellationToken
            );
            if (!string.Equals(storedValue, claim.ClaimedValue, StringComparison.Ordinal))
            {
                return;
            }

            await WriteTargetSecurityAttributeAsync(
                claim.UserId,
                claim.ReleasedValue,
                cancellationToken
            );
        }

        /// <inheritdoc/>
        public async Task<bool> DiscardVerificationAsync(
            string userId,
            VerifiedIdAction action,
            CancellationToken cancellationToken
        )
        {
            if (!IsEnabled || !RequiresVerificationPerAction)
            {
                return false;
            }

            VerifiedIdVerificationState verificationState = await GetVerificationStateAsync(
                userId,
                cancellationToken
            );
            if (
                verificationState.VerifiedForAction != action
                || !verificationState.LastVerifiedAt.HasValue
            )
            {
                return false;
            }

            // Graph has no conditional updates, so this overwrites a verification recorded after the lookup above.
            // The discard names the verification it was meant for, so that verification restores itself
            // (see RestoreVerificationOverwrittenByDiscardAsync).
            await WriteTargetSecurityAttributeAsync(
                userId,
                VerificationAttributeValue.CreateDiscarded(
                    _timeProvider.GetUtcNow(),
                    verificationState.LastVerifiedAt.Value
                ),
                cancellationToken
            );
            return true;
        }

        /// <inheritdoc/>
        public async Task RestoreVerificationOverwrittenByDiscardAsync(
            string userId,
            DateTimeOffset verifiedAt,
            VerifiedIdAction? action,
            CancellationToken cancellationToken
        )
        {
            // Verifications are only discarded when every action requires its own verification.
            if (!IsEnabled || !RequiresVerificationPerAction)
            {
                return;
            }

            (string attributeSetName, string attributeName) = GetTargetSecurityAttribute();

            // A discard that looked up the previous verification before this one was recorded stores its value
            // right after its lookup, so it has landed once the settle delay passed.
            await Task.Delay(CLAIM_SETTLE_DELAY, _timeProvider, cancellationToken);

            string? storedValue = await ReadTargetSecurityAttributeAsync(
                userId,
                attributeSetName,
                attributeName,
                cancellationToken
            );
            if (!VerificationAttributeValue.IsDiscardOfOtherVerification(storedValue, verifiedAt))
            {
                return;
            }

            _logger.LogWarning(
                "A discard of an earlier Verified ID verification overwrote the new verification of user {UserId}. Restoring the new verification.",
                userId
            );
            await WriteTargetSecurityAttributeAsync(
                userId,
                VerificationAttributeValue.Create(verifiedAt, action),
                cancellationToken
            );
        }

        private async Task WriteTargetSecurityAttributeAsync(
            string userId,
            string value,
            CancellationToken cancellationToken
        )
        {
            (string attributeSetName, string attributeName) = GetTargetSecurityAttribute();
            User requestBody = TargetSecurityAttributeRequestBody.Create(
                attributeSetName,
                attributeName,
                value
            );

            await _graphClient
                .Users[userId]
                .PatchAsync(requestBody, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// <c>true</c> if the attribute of <paramref name="user"/> still holds the verification described by
        /// <paramref name="verificationState"/>, i.e. a claim of it is not readable yet.
        /// </summary>
        private static bool IsClaimedVerificationStillStored(
            User? user,
            string attributeSetName,
            string attributeName,
            VerifiedIdVerificationState verificationState
        )
        {
            return LastVerificationTimestampParser.TryGetLastVerification(
                    user,
                    attributeSetName,
                    attributeName,
                    out DateTimeOffset storedLastVerifiedAt,
                    out VerifiedIdAction? storedVerifiedForAction
                )
                && storedLastVerifiedAt == verificationState.LastVerifiedAt
                && storedVerifiedForAction == verificationState.VerifiedForAction;
        }

        private async Task<string?> ReadTargetSecurityAttributeAsync(
            string userId,
            string attributeSetName,
            string attributeName,
            CancellationToken cancellationToken
        )
        {
            User? user = await GetUserWithCustomSecurityAttributesAsync(userId, cancellationToken);
            return LastVerificationTimestampParser.GetRawAttributeValue(
                user,
                attributeSetName,
                attributeName
            );
        }

        private Task<User?> GetUserWithCustomSecurityAttributesAsync(
            string userId,
            CancellationToken cancellationToken
        )
        {
            return _graphClient
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
        }

        private (string AttributeSetName, string AttributeName) GetTargetSecurityAttribute()
        {
            string attributeSetName = RequireSetting(
                _verifiedIdOptions.TargetSecurityAttributeSet,
                $"VerifiedId:{nameof(VerifiedIdOptions.TargetSecurityAttributeSet)}"
            );
            string attributeName = RequireSetting(
                _verifiedIdOptions.TargetSecurityAttribute,
                $"VerifiedId:{nameof(VerifiedIdOptions.TargetSecurityAttribute)}"
            );
            return (attributeSetName, attributeName);
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
