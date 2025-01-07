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
    }
}
