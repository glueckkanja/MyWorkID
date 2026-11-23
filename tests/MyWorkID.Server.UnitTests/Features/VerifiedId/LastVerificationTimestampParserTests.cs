using System.Globalization;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Serialization;
using MyWorkID.Server.Features.VerifiedId;
using MyWorkID.Server.Features.VerifiedId.Entities;

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
            User user = CreateUserWithUntypedAttribute(BoundValue(_timestamp));

            bool found = LastVerificationTimestampParser.TryGetLastVerifiedAt(user, AttributeSetName, AttributeName, out DateTimeOffset lastVerifiedAt);

            found.Should().BeTrue();
            lastVerifiedAt.Should().Be(_timestamp);
            lastVerifiedAt.Offset.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void TryGetLastVerifiedAt_WithDifferentCasing_ParsesTimestamp()
        {
            User user = CreateUserWithUntypedAttribute(BoundValue(_timestamp));

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
                                [AttributeName] = BoundValue(_timestamp),
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
            using JsonDocument document = JsonDocument.Parse($$"""{"{{AttributeName}}":"{{BoundValue(_timestamp)}}"}""");
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
            User user = CreateUserWithUntypedAttribute(Strings.VERIFIED_ID_BOUND_TIMESTAMP_PREFIX + "2026-10-20T08:30:15");

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
            User user = CreateUserWithUntypedAttribute(BoundValue(_timestamp));

            bool found = LastVerificationTimestampParser.TryGetLastVerifiedAt(user, "OtherSet", AttributeName, out _);

            found.Should().BeFalse();
        }

        [Fact]
        public void TryGetLastVerifiedAt_WithOtherAttribute_ReturnsFalse()
        {
            User user = CreateUserWithUntypedAttribute(BoundValue(_timestamp));

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

        [Fact]
        public void TryGetLastVerifiedAt_WithLegacyUnboundTimestamp_ReturnsFalse()
        {
            User user = CreateUserWithUntypedAttribute(_timestamp.UtcDateTime.ToString("O", CultureInfo.InvariantCulture));

            bool found = LastVerificationTimestampParser.TryGetLastVerifiedAt(user, AttributeSetName, AttributeName, out _);

            found.Should().BeFalse();
        }

        [Fact]
        public void TryGetLastVerifiedAt_WithPrefixOnly_ReturnsFalse()
        {
            User user = CreateUserWithUntypedAttribute(Strings.VERIFIED_ID_BOUND_TIMESTAMP_PREFIX);

            bool found = LastVerificationTimestampParser.TryGetLastVerifiedAt(user, AttributeSetName, AttributeName, out _);

            found.Should().BeFalse();
        }

        [Theory]
        [InlineData(VerifiedIdAction.DismissUserRisk)]
        [InlineData(VerifiedIdAction.GenerateTemporaryAccessPass)]
        [InlineData(VerifiedIdAction.ResetPassword)]
        public void TryGetLastVerification_WithBoundAction_ParsesTimestampAndAction(VerifiedIdAction action)
        {
            User user = CreateUserWithUntypedAttribute(VerificationAttributeValue.Create(_timestamp, action));

            bool found = LastVerificationTimestampParser.TryGetLastVerification(user, AttributeSetName, AttributeName, out DateTimeOffset lastVerifiedAt, out VerifiedIdAction? verifiedForAction);

            found.Should().BeTrue();
            lastVerifiedAt.Should().Be(_timestamp);
            verifiedForAction.Should().Be(action);
        }

        [Fact]
        public void TryGetLastVerification_WithoutAction_ParsesTimestampWithoutAction()
        {
            User user = CreateUserWithUntypedAttribute(BoundValue(_timestamp));

            bool found = LastVerificationTimestampParser.TryGetLastVerification(user, AttributeSetName, AttributeName, out DateTimeOffset lastVerifiedAt, out VerifiedIdAction? verifiedForAction);

            found.Should().BeTrue();
            lastVerifiedAt.Should().Be(_timestamp);
            verifiedForAction.Should().BeNull();
        }

        [Theory]
        [InlineData("Unknown")]
        [InlineData("dismissUserRisk")] // only exact action names are accepted
        [InlineData("0")] // numeric enum values are not accepted
        [InlineData("")]
        public void TryGetLastVerification_WithUnknownAction_ReturnsFalse(string action)
        {
            User user = CreateUserWithUntypedAttribute(BoundValue(_timestamp) + Strings.VERIFIED_ID_ACTION_SEPARATOR + action);

            bool found = LastVerificationTimestampParser.TryGetLastVerification(user, AttributeSetName, AttributeName, out _, out VerifiedIdAction? verifiedForAction);

            found.Should().BeFalse("an unknown action must not turn into a verification that unlocks any action");
            verifiedForAction.Should().BeNull();
        }

        [Fact]
        public void VerificationAttributeValue_Create_AppendsActionOnlyWhenGiven()
        {
            VerificationAttributeValue.Create(_timestamp, null).Should().Be("bound-v1:2026-10-20T08:30:15Z");
            VerificationAttributeValue.Create(_timestamp, VerifiedIdAction.ResetPassword)
                .Should().Be("bound-v1:2026-10-20T08:30:15Z;ResetPassword");
        }

        [Fact]
        public void VerificationAttributeValue_CreateUsed_StoresTimestampWithUsedPrefix()
        {
            VerificationAttributeValue.CreateUsed(_timestamp).Should().Be("used-v1:2026-10-20T08:30:15Z");
        }

        [Fact]
        public void VerificationAttributeValue_Create_DropsFractionalSeconds()
        {
            DateTimeOffset timestampWithFraction = _timestamp.AddTicks(6428478);

            VerificationAttributeValue.Create(timestampWithFraction, null).Should().Be("bound-v1:2026-10-20T08:30:15Z");
        }

        [Fact]
        public void VerificationAttributeValue_Create_FitsEntraIdLimitForEveryAction()
        {
            DateTimeOffset latestTimestamp = new(9999, 12, 31, 23, 59, 59, TimeSpan.Zero);

            foreach (VerifiedIdAction action in Enum.GetValues<VerifiedIdAction>())
            {
                VerificationAttributeValue.Create(latestTimestamp, action).Length
                    .Should().BeLessThanOrEqualTo(VerificationAttributeValue.MAXIMUM_LENGTH, "Entra ID rejects longer values for {0}", action);
            }
            VerificationAttributeValue.CreateUsed(latestTimestamp).Length
                .Should().BeLessThanOrEqualTo(VerificationAttributeValue.MAXIMUM_LENGTH);
            VerificationAttributeValue.CreateClaimed(latestTimestamp).Length
                .Should().BeLessThanOrEqualTo(VerificationAttributeValue.MAXIMUM_LENGTH);
        }

        [Fact]
        public void VerificationAttributeValue_CreateClaimed_IsUniqueAndNotRecognizedAsVerification()
        {
            string firstClaim = VerificationAttributeValue.CreateClaimed(_timestamp);
            string secondClaim = VerificationAttributeValue.CreateClaimed(_timestamp);

            firstClaim.Should().StartWith("used-v1:2026-10-20T08:30:15Z;");
            firstClaim.Should().NotBe(secondClaim);
            LastVerificationTimestampParser.TryGetLastVerification(CreateUserWithUntypedAttribute(firstClaim), AttributeSetName, AttributeName, out _, out _)
                .Should().BeFalse();
        }

        [Fact]
        public void TryGetLastVerification_WithValueWrittenByCreate_RoundTrips()
        {
            User user = CreateUserWithUntypedAttribute(VerificationAttributeValue.Create(_timestamp.AddTicks(6428478), VerifiedIdAction.GenerateTemporaryAccessPass));

            bool found = LastVerificationTimestampParser.TryGetLastVerification(user, AttributeSetName, AttributeName, out DateTimeOffset lastVerifiedAt, out VerifiedIdAction? verifiedForAction);

            found.Should().BeTrue();
            lastVerifiedAt.Should().Be(_timestamp);
            verifiedForAction.Should().Be(VerifiedIdAction.GenerateTemporaryAccessPass);
        }

        private static string BoundValue(DateTimeOffset timestamp)
        {
            return Strings.VERIFIED_ID_BOUND_TIMESTAMP_PREFIX + timestamp.UtcDateTime.ToString("O", CultureInfo.InvariantCulture);
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
