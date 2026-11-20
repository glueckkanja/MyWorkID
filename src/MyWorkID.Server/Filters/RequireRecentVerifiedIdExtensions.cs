using MyWorkID.Server.Features.VerifiedId.Entities;

namespace MyWorkID.Server.Filters
{
    /// <summary>
    /// Endpoint metadata naming the action a gated endpoint performs. When every action requires its own
    /// verification, <see cref="RequireRecentVerifiedIdEndpointFilter"/> only accepts a verification bound to it.
    /// </summary>
    /// <param name="Action">The action the endpoint performs.</param>
    public record VerifiedIdActionMetadata(VerifiedIdAction Action);

    /// <summary>
    /// Registers the Verified ID requirement on sensitive endpoints.
    /// </summary>
    public static class RequireRecentVerifiedIdExtensions
    {
        /// <summary>
        /// Requires a recent Verified ID verification for the endpoint, bound to <paramref name="action"/>
        /// when every action requires its own verification.
        /// </summary>
        /// <param name="builder">The route handler builder of the endpoint.</param>
        /// <param name="action">The action the endpoint performs.</param>
        /// <returns>The route handler builder for chaining.</returns>
        public static RouteHandlerBuilder RequireRecentVerifiedId(
            this RouteHandlerBuilder builder,
            VerifiedIdAction action
        )
        {
            return builder
                .WithMetadata(new VerifiedIdActionMetadata(action))
                .AddEndpointFilter<RequireRecentVerifiedIdEndpointFilter>();
        }
    }
}
