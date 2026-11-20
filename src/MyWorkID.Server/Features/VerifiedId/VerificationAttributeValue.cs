using System.Globalization;
using MyWorkID.Server.Features.VerifiedId.Entities;

namespace MyWorkID.Server.Features.VerifiedId
{
    /// <summary>
    /// Formats the value stored in the custom security attribute after a successful verification:
    /// <c>bound-v1:&lt;timestamp&gt;</c>, followed by <c>;&lt;action&gt;</c> when the verification was
    /// started for a specific action.
    /// </summary>
    public static class VerificationAttributeValue
    {
        /// <summary>
        /// Entra ID rejects custom security attribute string values that are longer than this.
        /// </summary>
        public const int MAXIMUM_LENGTH = 64;

        // Second precision keeps the longest value (bound to GenerateTemporaryAccessPass) within MAXIMUM_LENGTH.
        private const string TIMESTAMP_FORMAT = "yyyy-MM-dd'T'HH:mm:ss'Z'";

        /// <summary>
        /// Creates the attribute value for a verification completed at <paramref name="verifiedAt"/>.
        /// </summary>
        /// <param name="verifiedAt">When the verification was completed.</param>
        /// <param name="action">The action the verification is bound to, if any.</param>
        /// <returns>The value to store in the custom security attribute.</returns>
        public static string Create(DateTimeOffset verifiedAt, VerifiedIdAction? action)
        {
            string value = Strings.VERIFIED_ID_BOUND_TIMESTAMP_PREFIX + FormatTimestamp(verifiedAt);
            return action.HasValue
                ? value + Strings.VERIFIED_ID_ACTION_SEPARATOR + action.Value
                : value;
        }

        /// <summary>
        /// Creates the attribute value that marks a verification as used at <paramref name="usedAt"/>.
        /// </summary>
        /// <param name="usedAt">When the verification was used.</param>
        /// <returns>The value to store in the custom security attribute.</returns>
        public static string CreateUsed(DateTimeOffset usedAt)
        {
            return Strings.VERIFIED_ID_USED_TIMESTAMP_PREFIX + FormatTimestamp(usedAt);
        }

        /// <summary>
        /// Creates the attribute value that claims a verification for a single action at <paramref name="claimedAt"/>.
        /// It marks the verification as used and carries a unique claim ID, so a request can tell by reading the
        /// attribute back whether its own claim or the claim of a concurrent request was stored.
        /// </summary>
        /// <param name="claimedAt">When the verification was claimed.</param>
        /// <returns>The value to store in the custom security attribute.</returns>
        public static string CreateClaimed(DateTimeOffset claimedAt)
        {
            return CreateUsed(claimedAt)
                + Strings.VERIFIED_ID_ACTION_SEPARATOR
                + Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Creates the attribute value that discards the verification completed at
        /// <paramref name="discardedVerifiedAt"/>. Like <see cref="CreateUsed"/>, it no longer unlocks any action,
        /// but it names the discarded verification, so a verification recorded concurrently can tell that the
        /// discard was not meant for it (see <see cref="IsDiscardOfOtherVerification"/>).
        /// </summary>
        /// <param name="discardedAt">When the verification was discarded.</param>
        /// <param name="discardedVerifiedAt">When the discarded verification was completed.</param>
        /// <returns>The value to store in the custom security attribute.</returns>
        public static string CreateDiscarded(DateTimeOffset discardedAt, DateTimeOffset discardedVerifiedAt)
        {
            return CreateUsed(discardedAt)
                + Strings.VERIFIED_ID_ACTION_SEPARATOR
                + FormatTimestamp(discardedVerifiedAt);
        }

        /// <summary>
        /// <c>true</c> if <paramref name="value"/> was created by <see cref="CreateDiscarded"/> for a verification
        /// other than the one completed at <paramref name="verifiedAt"/>.
        /// </summary>
        /// <param name="value">The value stored in the custom security attribute.</param>
        /// <param name="verifiedAt">When the verification to compare with was completed.</param>
        /// <returns><c>true</c> if the value discards another verification.</returns>
        public static bool IsDiscardOfOtherVerification(string? value, DateTimeOffset verifiedAt)
        {
            if (
                value is null
                || !value.StartsWith(Strings.VERIFIED_ID_USED_TIMESTAMP_PREFIX, StringComparison.Ordinal)
            )
            {
                return false;
            }

            int separatorIndex = value.IndexOf(Strings.VERIFIED_ID_ACTION_SEPARATOR);
            if (separatorIndex < 0)
            {
                return false;
            }

            // Claims carry a claim ID instead of a timestamp, so they are not discards.
            string discardedTimestamp = value[(separatorIndex + 1)..];
            if (
                !DateTimeOffset.TryParseExact(
                    discardedTimestamp,
                    TIMESTAMP_FORMAT,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out _
                )
            )
            {
                return false;
            }

            return !string.Equals(discardedTimestamp, FormatTimestamp(verifiedAt), StringComparison.Ordinal);
        }

        /// <summary>
        /// Parses an action name. Only exact names of defined actions are accepted, so neither numbers
        /// nor differently cased names can bind a verification to an action.
        /// </summary>
        /// <param name="value">The action name.</param>
        /// <param name="action">The parsed action when the method returns <c>true</c>.</param>
        /// <returns><c>true</c> if <paramref name="value"/> names a defined action.</returns>
        public static bool TryParseAction(string? value, out VerifiedIdAction action)
        {
            action = default;
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            foreach (VerifiedIdAction candidate in Enum.GetValues<VerifiedIdAction>())
            {
                if (string.Equals(candidate.ToString(), value, StringComparison.Ordinal))
                {
                    action = candidate;
                    return true;
                }
            }

            return false;
        }

        private static string FormatTimestamp(DateTimeOffset timestamp)
        {
            return timestamp.UtcDateTime.ToString(TIMESTAMP_FORMAT, CultureInfo.InvariantCulture);
        }
    }
}
