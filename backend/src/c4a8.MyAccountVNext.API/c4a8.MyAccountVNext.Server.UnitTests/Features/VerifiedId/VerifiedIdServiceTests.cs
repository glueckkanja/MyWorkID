using c4a8.MyAccountVNext.Server.Features.VerifiedId;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using NSubstitute;

namespace c4a8.MyAccountVNext.Server.UnitTests.Features.VerifiedId
{
    public class VerifiedIdServiceTests
    {
        [Fact]
        public void CreateSetTargetSecurityAttributeRequestBody_ReturnsCorrectBody()
        {
            var targetSecurityAttributeValue = Guid.NewGuid().ToString();
            VerifiedIdOptions verifiedIdOptions = new VerifiedIdOptions();
            verifiedIdOptions.TargetSecurityAttribute = "targetSecurityAttribute";
            verifiedIdOptions.TargetSecurityAttributeSet = "targetSecurityAttributeSet";
            var options = Options.Create(verifiedIdOptions);
            var verifiedIdClient = Substitute.For<HttpClient>();
            var requestAdapter = Substitute.For<IRequestAdapter>();
            var graphClient = new GraphServiceClient(requestAdapter);
            var logger = Substitute.For<ILogger<VerifiedIdService>>();
            var sut = new VerifiedIdService(verifiedIdClient, options, graphClient, logger);

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
