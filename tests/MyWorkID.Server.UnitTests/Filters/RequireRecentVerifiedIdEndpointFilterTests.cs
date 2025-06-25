using MyWorkID.Server.Features.VerifiedId;
using MyWorkID.Server.Filters;
using MyWorkID.Server.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Identity.Web;
using NSubstitute;
using System.Security.Claims;

namespace MyWorkID.Server.UnitTests.Filters
{
    public class RequireRecentVerifiedIdEndpointFilterTests
    {
        [Fact]
        public async Task InvokeAsync_WhenUserHasNoObjectId_ReturnsUnauthorized()
        {
            // Arrange
            var verifiedIdService = Substitute.For<IVerifiedIdService>();
            var filter = new RequireRecentVerifiedIdEndpointFilter(verifiedIdService);
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            var context = new DefaultEndpointFilterInvocationContext(httpContext);
            var next = Substitute.For<EndpointFilterDelegate>();

            // Act
            var result = await filter.InvokeAsync(context, next);

            // Assert
            result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.UnauthorizedHttpResult>();
            await next.DidNotReceive().Invoke(Arg.Any<EndpointFilterInvocationContext>());
        }

        [Fact]
        public async Task InvokeAsync_WhenUserDoesNotHaveRequireRecentVerifiedIdRole_CallsNext()
        {
            // Arrange
            var verifiedIdService = Substitute.For<IVerifiedIdService>();
            var filter = new RequireRecentVerifiedIdEndpointFilter(verifiedIdService);
            var httpContext = new DefaultHttpContext();
            var userId = Guid.NewGuid().ToString();
            var claims = new[]
            {
                new Claim(ClaimConstants.ObjectId, userId)
            };
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
            var context = new DefaultEndpointFilterInvocationContext(httpContext);
            var next = Substitute.For<EndpointFilterDelegate>();
            next.Invoke(Arg.Any<EndpointFilterInvocationContext>()).Returns("next_result");

            // Act
            var result = await filter.InvokeAsync(context, next);

            // Assert
            result.Should().Be("next_result");
            await next.Received(1).Invoke(context);
            await verifiedIdService.DidNotReceive().HasRecentVerifiedId(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task InvokeAsync_WhenUserHasRoleAndRecentVerifiedId_CallsNext()
        {
            // Arrange
            var verifiedIdService = Substitute.For<IVerifiedIdService>();
            verifiedIdService.HasRecentVerifiedId(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
            var filter = new RequireRecentVerifiedIdEndpointFilter(verifiedIdService);
            var httpContext = new DefaultHttpContext();
            var userId = Guid.NewGuid().ToString();
            var claims = new[]
            {
                new Claim(ClaimConstants.ObjectId, userId),
                new Claim(ClaimTypes.Role, Strings.REQUIRE_RECENT_VERIFIED_ID_ROLE)
            };
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
            var context = new DefaultEndpointFilterInvocationContext(httpContext);
            var next = Substitute.For<EndpointFilterDelegate>();
            next.Invoke(Arg.Any<EndpointFilterInvocationContext>()).Returns("next_result");

            // Act
            var result = await filter.InvokeAsync(context, next);

            // Assert
            result.Should().Be("next_result");
            await next.Received(1).Invoke(context);
            await verifiedIdService.Received(1).HasRecentVerifiedId(userId, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task InvokeAsync_WhenUserHasRoleButNoRecentVerifiedId_ReturnsProblem()
        {
            // Arrange
            var verifiedIdService = Substitute.For<IVerifiedIdService>();
            verifiedIdService.HasRecentVerifiedId(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
            var filter = new RequireRecentVerifiedIdEndpointFilter(verifiedIdService);
            var httpContext = new DefaultHttpContext();
            var userId = Guid.NewGuid().ToString();
            var claims = new[]
            {
                new Claim(ClaimConstants.ObjectId, userId),
                new Claim(ClaimTypes.Role, Strings.REQUIRE_RECENT_VERIFIED_ID_ROLE)
            };
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
            var context = new DefaultEndpointFilterInvocationContext(httpContext);
            var next = Substitute.For<EndpointFilterDelegate>();

            // Act
            var result = await filter.InvokeAsync(context, next);

            // Assert
            result.Should().BeOfType<ProblemHttpResult>();
            var problemResult = result as ProblemHttpResult;
            problemResult!.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
            problemResult.ProblemDetails.Title.Should().Be("Recent VerifiedID Required");
            await next.DidNotReceive().Invoke(Arg.Any<EndpointFilterInvocationContext>());
            await verifiedIdService.Received(1).HasRecentVerifiedId(userId, Arg.Any<CancellationToken>());
        }
    }
}