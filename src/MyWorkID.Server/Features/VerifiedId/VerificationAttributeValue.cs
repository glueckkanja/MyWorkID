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

            VerifiedIdAction? matchingAction = Enum.GetValues<VerifiedIdAction>()
                .Where(candidate => string.Equals(candidate.ToString(), value, StringComparison.Ordinal))
                .Select(candidate => (VerifiedIdAction?)candidate)
                .FirstOrDefault();

            if (!matchingAction.HasValue)
            {
                return false;
            }

            action = matchingAction.Value;
            return true;
        }

        private static string FormatTimestamp(DateTimeOffset timestamp)
        {
            return timestamp.UtcDateTime.ToString(TIMESTAMP_FORMAT, CultureInfo.InvariantCulture);
        }
    }
}
