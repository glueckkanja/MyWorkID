using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using MyWorkID.Server.Features.VerifiedId;
using MyWorkID.Server.Features.VerifiedId.Entities;

namespace MyWorkID.Server.UnitTests.Features.VerifiedId
{
    public class JwtTokenProviderTests
    {
        private const string SigningKey = "b1c2d3e4-f5a6-7b8c-9d0e-1f2a3b4c5123";

        [Fact]
        public void GenerateToken_WithAction_AddsActionClaim()
        {
            string token = JwtTokenProvider.GenerateToken("user-id", SigningKey, VerifiedIdAction.DismissUserRisk);

            JwtSecurityToken parsedToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            parsedToken.Claims.Should().ContainSingle(claim => claim.Type == "userId").Which.Value.Should().Be("user-id");
            parsedToken.Claims.Should().ContainSingle(claim => claim.Type == Strings.VERIFIED_ID_ACTION_CLAIM)
                .Which.Value.Should().Be(nameof(VerifiedIdAction.DismissUserRisk));
        }

        [Fact]
        public void GenerateToken_WithoutAction_HasNoActionClaim()
        {
            string token = JwtTokenProvider.GenerateToken("user-id", SigningKey);

            JwtSecurityToken parsedToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            parsedToken.Claims.Should().NotContain(claim => claim.Type == Strings.VERIFIED_ID_ACTION_CLAIM);
        }
    }
}
