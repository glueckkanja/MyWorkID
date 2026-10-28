using System.Globalization;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Serialization;
using MyWorkID.Server.Features.VerifiedId;

namespace MyWorkID.Server.UnitTests.Features.VerifiedId
{
    public class LastVerificationTimestampParserTests
    {
        private const string AttributeSetName = "MyWorkID";
        private const string AttributeName = "lastVerifiedFaceCheck";
        private static readonly DateTimeOffset _timestamp = new(2026, 10, 20, 8, 30, 15, TimeSpan.Zero);

        [Fact]
        public void TryGetLastVerifiedAt_WithUntypedObjectFromGraph_ParsesTimestamp()
        {
            User user = CreateUserWithUntypedAttribute(_timestamp.UtcDateTime.ToString("O", CultureInfo.InvariantCulture));

            bool found = LastVerificationTimestampParser.TryGetLastVerifiedAt(user, AttributeSetName, AttributeName, out DateTimeOffset lastVerifiedAt);

            found.Should().BeTrue();
            lastVerifiedAt.Should().Be(_timestamp);
            lastVerifiedAt.Offset.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void TryGetLastVerifiedAt_WithDifferentCasing_ParsesTimestamp()
        {
            User user = CreateUserWithUntypedAttribute(_timestamp.UtcDateTime.ToString("O", CultureInfo.InvariantCulture));

            bool found = LastVerificationTimestampParser.TryGetLastVerifiedAt(user, "myworkid", "LASTVERIFIEDFACECHECK", out DateTimeOffset lastVerifiedAt);

            found.Should().BeTrue();
            lastVerifiedAt.Should().Be(_timestamp);
        }

        [Fact]
        public void TryGetLastVerifiedAt_WithCustomSecurityAttributeValue_ParsesTimestamp()
        {
            User user = new()
            {
                CustomSecurityAttributes = new CustomSecurityAttributeValue
                {
                    AdditionalData = new Dictionary<string, object>
                    {
                        [AttributeSetName] = new CustomSecurityAttributeValue
                        {
                            AdditionalData = new Dictionary<string, object>
                            {
                                [AttributeName] = _timestamp.UtcDateTime.ToString("O", CultureInfo.InvariantCulture),
                            },
                        },
                    },
                },
            };

            bool found = LastVerificationTimestampParser.TryGetLastVerifiedAt(user, AttributeSetName, AttributeName, out DateTimeOffset lastVerifiedAt);

            found.Should().BeTrue();
            lastVerifiedAt.Should().Be(_timestamp);
        }

        [Fact]
        public void TryGetLastVerifiedAt_WithJsonElement_ParsesTimestamp()
        {
            using JsonDocument document = JsonDocument.Parse($$"""{"{{AttributeName}}":"{{_timestamp.UtcDateTime:O}}"}""");
            User user = new()
            {
                CustomSecurityAttributes = new CustomSecurityAttributeValue
                {
                    AdditionalData = new Dictionary<string, object>
                    {
                        [AttributeSetName] = document.RootElement.Clone(),
                    },
                },
            };

            bool found = LastVerificationTimestampParser.TryGetLastVerifiedAt(user, AttributeSetName, AttributeName, out DateTimeOffset lastVerifiedAt);

            found.Should().BeTrue();
            lastVerifiedAt.Should().Be(_timestamp);
        }

        [Fact]
        public void TryGetLastVerifiedAt_WithTimestampWithoutOffset_AssumesUtc()
        {
            User user = CreateUserWithUntypedAttribute("2026-10-20T08:30:15");

            bool found = LastVerificationTimestampParser.TryGetLastVerifiedAt(user, AttributeSetName, AttributeName, out DateTimeOffset lastVerifiedAt);

            found.Should().BeTrue();
            lastVerifiedAt.Should().Be(_timestamp);
        }

        [Fact]
        public void TryGetLastVerifiedAt_WithNullUser_ReturnsFalse()
        {
            bool found = LastVerificationTimestampParser.TryGetLastVerifiedAt(null, AttributeSetName, AttributeName, out _);

            found.Should().BeFalse();
        }

        [Fact]
        public void TryGetLastVerifiedAt_WithoutCustomSecurityAttributes_ReturnsFalse()
        {
            bool found = LastVerificationTimestampParser.TryGetLastVerifiedAt(new User(), AttributeSetName, AttributeName, out _);

            found.Should().BeFalse();
        }

        [Fact]
        public void TryGetLastVerifiedAt_WithOtherAttributeSet_ReturnsFalse()
        {
            User user = CreateUserWithUntypedAttribute(_timestamp.UtcDateTime.ToString("O", CultureInfo.InvariantCulture));

            bool found = LastVerificationTimestampParser.TryGetLastVerifiedAt(user, "OtherSet", AttributeName, out _);

            found.Should().BeFalse();
        }

        [Fact]
        public void TryGetLastVerifiedAt_WithOtherAttribute_ReturnsFalse()
        {
            User user = CreateUserWithUntypedAttribute(_timestamp.UtcDateTime.ToString("O", CultureInfo.InvariantCulture));

            bool found = LastVerificationTimestampParser.TryGetLastVerifiedAt(user, AttributeSetName, "otherAttribute", out _);

            found.Should().BeFalse();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("not-a-date")]
        public void TryGetLastVerifiedAt_WithUnparsableValue_ReturnsFalse(string value)
        {
            User user = CreateUserWithUntypedAttribute(value);

            bool found = LastVerificationTimestampParser.TryGetLastVerifiedAt(user, AttributeSetName, AttributeName, out _);

            found.Should().BeFalse();
        }

        [Fact]
        public void TryGetLastVerifiedAt_WithNonStringUntypedValue_ReturnsFalse()
        {
            User user = new()
            {
                CustomSecurityAttributes = new CustomSecurityAttributeValue
                {
                    AdditionalData = new Dictionary<string, object>
                    {
                        [AttributeSetName] = new UntypedObject(new Dictionary<string, UntypedNode>
                        {
                            [AttributeName] = new UntypedBoolean(true),
                        }),
                    },
                },
            };

            bool found = LastVerificationTimestampParser.TryGetLastVerifiedAt(user, AttributeSetName, AttributeName, out _);

            found.Should().BeFalse();
        }

        private static User CreateUserWithUntypedAttribute(string value)
        {
            return new User
            {
                CustomSecurityAttributes = new CustomSecurityAttributeValue
                {
                    AdditionalData = new Dictionary<string, object>
                    {
                        [AttributeSetName] = new UntypedObject(new Dictionary<string, UntypedNode>
                        {
                            ["@odata.type"] = new UntypedString("#microsoft.graph.customSecurityAttributeValue"),
                            [AttributeName] = new UntypedString(value),
                        }),
                    },
                },
            };
        }
    }
}
