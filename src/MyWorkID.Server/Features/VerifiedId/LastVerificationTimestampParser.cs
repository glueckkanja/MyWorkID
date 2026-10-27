using System.Globalization;
using System.Text.Json;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Serialization;

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
        /// <returns><c>true</c> if the attribute exists and contains a parsable timestamp.</returns>
        public static bool TryGetLastVerifiedAt(
            User? user,
            string attributeSetName,
            string attributeName,
            out DateTimeOffset lastVerifiedAt
        )
        {
            lastVerifiedAt = default;

            IDictionary<string, object>? attributeSets = user
                ?.CustomSecurityAttributes
                ?.AdditionalData;
            if (
                attributeSets is null
                || !TryGetCaseInsensitive(attributeSets, attributeSetName, out object? attributeSet)
            )
            {
                return false;
            }

            string? rawValue = GetAttributeValue(attributeSet, attributeName);
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return false;
            }

            return DateTimeOffset.TryParse(
                rawValue,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out lastVerifiedAt
            );
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
