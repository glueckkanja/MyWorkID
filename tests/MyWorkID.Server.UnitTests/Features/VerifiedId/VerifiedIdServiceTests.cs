using MyWorkID.Server.Features.VerifiedId;
using MyWorkID.Server.Features.VerifiedId.SignalR;
using MyWorkID.Server.Options;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using NSubstitute;

namespace MyWorkID.Server.UnitTests.Features.VerifiedId
{
    public class VerifiedIdServiceTests
    {
        [Fact]
        public void CreateSetTargetSecurityAttributeRequestBody_ReturnsCorrectBody()
        {
            var targetSecurityAttributeValue = Guid.NewGuid().ToString();
            VerifiedIdOptions verifiedIdOptions = new VerifiedIdOptions
            {
                TargetSecurityAttribute = "targetSecurityAttribute",
                TargetSecurityAttributeSet = "targetSecurityAttributeSet"
            };
            var options = Microsoft.Extensions.Options.Options.Create(verifiedIdOptions);
            var verifiedIdClient = Substitute.For<HttpClient>();
            var requestAdapter = Substitute.For<IRequestAdapter>();
            var graphClient = new GraphServiceClient(requestAdapter);
            var verifiedIdSignalRRepository = Substitute.For<IVerifiedIdSignalRRepository>();
            var hubContext = Substitute.For<IHubContext<VerifiedIdHub, IVerifiedIdHub>>();
            var logger = Substitute.For<ILogger<VerifiedIdService>>();
            var sut = new VerifiedIdService(verifiedIdClient, options, graphClient, verifiedIdSignalRRepository, hubContext, logger);

            var user = sut.CreateSetTargetSecurityAttributeRequestBody(targetSecurityAttributeValue);

            user.CustomSecurityAttributes!.AdditionalData.TryGetValue(verifiedIdOptions.TargetSecurityAttributeSet, out var targetSecurityAttributeObject);
            targetSecurityAttributeObject.Should().BeOfType<CustomSecurityAttributeValue>();
            var targetSecurityAttribute = targetSecurityAttributeObject as CustomSecurityAttributeValue;
            targetSecurityAttribute.Should().NotBeNull();
            targetSecurityAttribute!.OdataType.Should().Be("#Microsoft.DirectoryServices.CustomSecurityAttributeValue");
            targetSecurityAttribute!.AdditionalData.TryGetValue(verifiedIdOptions.TargetSecurityAttribute, out var targetSecurityAttributeValueSet);
            targetSecurityAttributeValueSet.Should().Be(targetSecurityAttributeValue);
        }

        [Fact]
        public async Task HasRecentVerifiedId_WhenUserHasNoCustomSecurityAttributes_ReturnsFalse()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var verifiedIdOptions = new VerifiedIdOptions
            {
                TargetSecurityAttribute = "targetSecurityAttribute",
                TargetSecurityAttributeSet = "targetSecurityAttributeSet",
                RequiredVerificationTimeWindowMinutes = 30
            };
            var options = Microsoft.Extensions.Options.Options.Create(verifiedIdOptions);
            var verifiedIdClient = Substitute.For<HttpClient>();
            var requestAdapter = Substitute.For<IRequestAdapter>();
            var graphClient = new GraphServiceClient(requestAdapter);
            var verifiedIdSignalRRepository = Substitute.For<IVerifiedIdSignalRRepository>();
            var hubContext = Substitute.For<IHubContext<VerifiedIdHub, IVerifiedIdHub>>();
            var logger = Substitute.For<ILogger<VerifiedIdService>>();

            var user = new User { CustomSecurityAttributes = null };
            graphClient.Users[userId].GetAsync(Arg.Any<Action<Microsoft.Kiota.Abstractions.RequestConfiguration<Microsoft.Graph.Users.Item.UserItemRequestBuilder.UserItemRequestBuilderGetQueryParameters>>>(), Arg.Any<CancellationToken>())
                .Returns(user);

            var sut = new VerifiedIdService(verifiedIdClient, options, graphClient, verifiedIdSignalRRepository, hubContext, logger);

            // Act
            var result = await sut.HasRecentVerifiedId(userId, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasRecentVerifiedId_WhenUserHasRecentVerifiedId_ReturnsTrue()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var recentTimestamp = DateTime.UtcNow.AddMinutes(-15).ToString("O"); // 15 minutes ago
            var verifiedIdOptions = new VerifiedIdOptions
            {
                TargetSecurityAttribute = "targetSecurityAttribute",
                TargetSecurityAttributeSet = "targetSecurityAttributeSet",
                RequiredVerificationTimeWindowMinutes = 30
            };
            var options = Microsoft.Extensions.Options.Options.Create(verifiedIdOptions);
            var verifiedIdClient = Substitute.For<HttpClient>();
            var requestAdapter = Substitute.For<IRequestAdapter>();
            var graphClient = new GraphServiceClient(requestAdapter);
            var verifiedIdSignalRRepository = Substitute.For<IVerifiedIdSignalRRepository>();
            var hubContext = Substitute.For<IHubContext<VerifiedIdHub, IVerifiedIdHub>>();
            var logger = Substitute.For<ILogger<VerifiedIdService>>();

            var user = new User
            {
                CustomSecurityAttributes = new CustomSecurityAttributeValue
                {
                    AdditionalData = new Dictionary<string, object>
                    {
                        {
                            verifiedIdOptions.TargetSecurityAttributeSet, new CustomSecurityAttributeValue
                            {
                                AdditionalData = new Dictionary<string, object>
                                {
                                    { verifiedIdOptions.TargetSecurityAttribute, recentTimestamp }
                                }
                            }
                        }
                    }
                }
            };

            graphClient.Users[userId].GetAsync(Arg.Any<Action<Microsoft.Kiota.Abstractions.RequestConfiguration<Microsoft.Graph.Users.Item.UserItemRequestBuilder.UserItemRequestBuilderGetQueryParameters>>>(), Arg.Any<CancellationToken>())
                .Returns(user);

            var sut = new VerifiedIdService(verifiedIdClient, options, graphClient, verifiedIdSignalRRepository, hubContext, logger);

            // Act
            var result = await sut.HasRecentVerifiedId(userId, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasRecentVerifiedId_WhenUserHasOldVerifiedId_ReturnsFalse()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var oldTimestamp = DateTime.UtcNow.AddMinutes(-60).ToString("O"); // 60 minutes ago
            var verifiedIdOptions = new VerifiedIdOptions
            {
                TargetSecurityAttribute = "targetSecurityAttribute",
                TargetSecurityAttributeSet = "targetSecurityAttributeSet",
                RequiredVerificationTimeWindowMinutes = 30
            };
            var options = Microsoft.Extensions.Options.Options.Create(verifiedIdOptions);
            var verifiedIdClient = Substitute.For<HttpClient>();
            var requestAdapter = Substitute.For<IRequestAdapter>();
            var graphClient = new GraphServiceClient(requestAdapter);
            var verifiedIdSignalRRepository = Substitute.For<IVerifiedIdSignalRRepository>();
            var hubContext = Substitute.For<IHubContext<VerifiedIdHub, IVerifiedIdHub>>();
            var logger = Substitute.For<ILogger<VerifiedIdService>>();

            var user = new User
            {
                CustomSecurityAttributes = new CustomSecurityAttributeValue
                {
                    AdditionalData = new Dictionary<string, object>
                    {
                        {
                            verifiedIdOptions.TargetSecurityAttributeSet, new CustomSecurityAttributeValue
                            {
                                AdditionalData = new Dictionary<string, object>
                                {
                                    { verifiedIdOptions.TargetSecurityAttribute, oldTimestamp }
                                }
                            }
                        }
                    }
                }
            };

            graphClient.Users[userId].GetAsync(Arg.Any<Action<Microsoft.Kiota.Abstractions.RequestConfiguration<Microsoft.Graph.Users.Item.UserItemRequestBuilder.UserItemRequestBuilderGetQueryParameters>>>(), Arg.Any<CancellationToken>())
                .Returns(user);

            var sut = new VerifiedIdService(verifiedIdClient, options, graphClient, verifiedIdSignalRRepository, hubContext, logger);

            // Act
            var result = await sut.HasRecentVerifiedId(userId, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }
    }
}
