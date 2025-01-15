namespace MyWorkID.Server.Common
{
    /// <summary>
    /// Defines a contract for mapping endpoints in the application.
    /// </summary>
    public interface IEndpoint
    {
        /// <summary>
        /// Maps the endpoint routes.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder.</param>
        static abstract void MapEndpoint(IEndpointRouteBuilder endpoints);
    }
}
