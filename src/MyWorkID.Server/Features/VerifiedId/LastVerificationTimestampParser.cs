using System.Globalization;
using System.Text.Json;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Serialization;
using MyWorkID.Server.Features.VerifiedId.Entities;

namespace MyWorkID.Server.Features.VerifiedId
{
    /// <summary>
    /// Extracts the timestamp of the last Verified ID verification from the custom security
    /// attributes of a Graph <see cref="User"/>.
    /// </summary>
    public static class LastVerificationTimestampParser
    {
        /// <summary>
        /// Tries to read the last verification timestamp stored in the configured custom security attribute.
        /// </summary>
        /// <param name="user">The Graph user including its <c>customSecurityAttributes</c>.</param>
        /// <param name="attributeSetName">Name of the custom security attribute set.</param>
        /// <param name="attributeName">Name of the custom security attribute inside the set.</param>
        /// <param name="lastVerifiedAt">The parsed timestamp (UTC) when the method returns <c>true</c>.</param>
        /// <returns><c>true</c> if the attribute exists and contains a parsable timestamp carrying the
        /// <see cref="Strings.VERIFIED_ID_BOUND_TIMESTAMP_PREFIX"/> prefix.</returns>
        public static bool TryGetLastVerifiedAt(
            User? user,
            string attributeSetName,
            string attributeName,
            out DateTimeOffset lastVerifiedAt
        )
        {
            return TryGetLastVerification(
                user,
                attributeSetName,
                attributeName,
                out lastVerifiedAt,
                out _
            );
        }

        /// <summary>
        /// Tries to read the last verification stored in the configured custom security attribute, including the
        /// action the verification was started for.
        /// </summary>
        /// <param name="user">The Graph user including its <c>customSecurityAttributes</c>.</param>
        /// <param name="attributeSetName">Name of the custom security attribute set.</param>
        /// <param name="attributeName">Name of the custom security attribute inside the set.</param>
        /// <param name="lastVerifiedAt">The parsed timestamp (UTC) when the method returns <c>true</c>.</param>
        /// <param name="verifiedForAction">The action the verification is bound to, or <c>null</c> if it is not bound to an action.</param>
        /// <returns><c>true</c> if the attribute exists and contains a parsable timestamp carrying the
        /// <see cref="Strings.VERIFIED_ID_BOUND_TIMESTAMP_PREFIX"/> prefix, optionally followed by a known action.</returns>
        public static bool TryGetLastVerification(
            User? user,
            string attributeSetName,
            string attributeName,
            out DateTimeOffset lastVerifiedAt,
            out VerifiedIdAction? verifiedForAction
        )
        {
            lastVerifiedAt = default;
            verifiedForAction = null;

            string? rawValue = GetRawAttributeValue(user, attributeSetName, attributeName);
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return false;
            }

            // Values without the prefix were written before the presented credential was bound to the
            // user, so they do not prove that this user was verified.
            if (!rawValue.StartsWith(Strings.VERIFIED_ID_BOUND_TIMESTAMP_PREFIX, StringComparison.Ordinal))
            {
                return false;
            }

            string value = rawValue[Strings.VERIFIED_ID_BOUND_TIMESTAMP_PREFIX.Length..];
            string timestamp = value;
            int separatorIndex = value.IndexOf(Strings.VERIFIED_ID_ACTION_SEPARATOR);
            if (separatorIndex >= 0)
            {
                timestamp = value[..separatorIndex];
                // An unknown action must not turn into an unbound verification that unlocks any action.
                if (
                    !VerificationAttributeValue.TryParseAction(
                        value[(separatorIndex + 1)..],
                        out VerifiedIdAction action
                    )
                )
                {
                    return false;
                }
                verifiedForAction = action;
            }

            return DateTimeOffset.TryParse(
                timestamp,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out lastVerifiedAt
            );
        }

        /// <summary>
        /// Reads the unparsed value stored in the configured custom security attribute.
        /// </summary>
        /// <param name="user">The Graph user including its <c>customSecurityAttributes</c>.</param>
        /// <param name="attributeSetName">Name of the custom security attribute set.</param>
        /// <param name="attributeName">Name of the custom security attribute inside the set.</param>
        /// <returns>The stored value, or <c>null</c> if the attribute is not set.</returns>
        public static string? GetRawAttributeValue(
            User? user,
            string attributeSetName,
            string attributeName
        )
        {
            IDictionary<string, object>? attributeSets = user
                ?.CustomSecurityAttributes
                ?.AdditionalData;
            if (
                attributeSets is null
                || !TryGetCaseInsensitive(attributeSets, attributeSetName, out object? attributeSet)
            )
            {
                return null;
            }

            return GetAttributeValue(attributeSet, attributeName);
        }

        /// <summary>
        /// Reads the raw value of <paramref name="attributeName"/> from a deserialized attribute set.
        /// Graph returns the attribute set as an untyped object, but other serializers and tests may
        /// provide a <see cref="CustomSecurityAttributeValue"/>, a <see cref="JsonElement"/> or a dictionary.
        /// </summary>
        private static string? GetAttributeValue(object? attributeSet, string attributeName)
        {
            switch (attributeSet)
            {
                case UntypedObject untypedObject:
                    return TryGetCaseInsensitive(
                        untypedObject.GetValue(),
                        attributeName,
                        out UntypedNode? node
                    )
                        ? UntypedNodeToString(node)
                        : null;
                case CustomSecurityAttributeValue customSecurityAttributeValue:
                    return TryGetCaseInsensitive(
                        customSecurityAttributeValue.AdditionalData,
                        attributeName,
                        out object? value
                    )
                        ? ObjectToString(value)
                        : null;
                case IDictionary<string, object> dictionary:
                    return TryGetCaseInsensitive(
                        dictionary,
                        attributeName,
                        out object? dictionaryValue
                    )
                        ? ObjectToString(dictionaryValue)
                        : null;
                case JsonElement { ValueKind: JsonValueKind.Object } jsonElement:
                    return jsonElement
                        .EnumerateObject()
                        .Where(property =>
                            string.Equals(
                                property.Name,
                                attributeName,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                        .Select(property =>
                            property.Value.ValueKind == JsonValueKind.String
                                ? property.Value.GetString()
                                : property.Value.ToString()
                        )
                        .FirstOrDefault();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Converts an attribute value to a string. The switch expression handles null, strings,
        /// Kiota untyped nodes and JSON string values explicitly, and falls back to
        /// <see cref="object.ToString"/> for other types.
        /// </summary>
        /// <param name="value">The deserialized attribute value.</param>
        /// <returns>The string representation of the value, or <c>null</c> when it is null or unsupported.</returns>
        private static string? ObjectToString(object? value)
        {
            return value switch
            {
                null => null,
                string text => text,
                UntypedNode node => UntypedNodeToString(node),
                JsonElement { ValueKind: JsonValueKind.String } jsonElement => jsonElement.GetString(),
                _ => value.ToString(),
            };
        }

        /// <summary>
        /// Extracts the string value from a Kiota untyped node when it is a
        /// <see cref="UntypedString"/>.
        /// </summary>
        /// <param name="node">The untyped node to convert.</param>
        /// <returns>The string value, or <c>null</c> when the node is null or not a string.</returns>
        private static string? UntypedNodeToString(UntypedNode? node)
        {
            return node switch
            {
                UntypedString untypedString => untypedString.GetValue(),
                _ => null,
            };
        }

        private static bool TryGetCaseInsensitive<TValue>(
            IDictionary<string, TValue>? dictionary,
            string key,
            out TValue? value
        )
        {
            value = default;
            if (dictionary is null)
            {
                return false;
            }

            if (dictionary.TryGetValue(key, out TValue? exactMatch))
            {
                value = exactMatch;
                return true;
            }

            // Dictionary keys are never null, so a null key identifies the "not found" default.
            KeyValuePair<string, TValue> caseInsensitiveMatch = dictionary.FirstOrDefault(entry =>
                string.Equals(entry.Key, key, StringComparison.OrdinalIgnoreCase)
            );
            if (caseInsensitiveMatch.Key is null)
            {
                return false;
            }

            value = caseInsensitiveMatch.Value;
            return true;
        }
    }
}
