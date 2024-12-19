namespace c4a8.MyWorkID.Server.Common
{
    /// <summary>
    /// Provides extension methods for configuring OpenAPI documentation for various HTTP endpoints.
    /// </summary>
    public static class OpenApiConvenienceExtensions
    {
        /// <summary>
        /// Configures the response types for GET endpoints that return one or more items.
        /// </summary>
        /// <typeparam name="T">The type of the item(s) returned by the endpoint.</typeparam>
        /// <param name="builder">The route handler builder.</param>
        /// <returns>The updated route handler builder.</returns>
        public static RouteHandlerBuilder ProducesGet<T>(this RouteHandlerBuilder builder) =>
            builder
                .Produces<T>()
                .Produces(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

        /// <summary>
        /// Maps a GET endpoint with OpenAPI documentation.
        /// </summary>
        /// <typeparam name="T">The type of the item(s) returned by the endpoint.</typeparam>
        /// <param name="endpoints">The endpoint route builder.</param>
        /// <param name="pattern">The URL pattern of the endpoint.</param>
        /// <param name="handler">The delegate that handles the endpoint.</param>
        /// <returns>The route handler builder.</returns>
        public static RouteHandlerBuilder MapGetWithOpenApi<T>(
            this IEndpointRouteBuilder endpoints,
            string pattern,
            Delegate handler) =>
            endpoints.MapGet(pattern, handler)
                .ProducesGet<T>();

        /// <summary>
        /// Configures the response types for POST endpoints that create a single item.
        /// </summary>
        /// <param name="builder">The route handler builder.</param>
        /// <returns>The updated route handler builder.</returns>
        public static RouteHandlerBuilder ProducesPost(this RouteHandlerBuilder builder) =>
            builder
                .ProducesValidationProblem()
                .ProducesProblem(StatusCodes.Status500InternalServerError);

        /// <summary>
        /// Maps a POST endpoint with OpenAPI documentation for creating an item.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder.</param>
        /// <param name="pattern">The URL pattern of the endpoint.</param>
        /// <param name="handler">The delegate that handles the endpoint.</param>
        /// <returns>The route handler builder.</returns>
        public static RouteHandlerBuilder MapPostWithCreatedOpenApi(
            this IEndpointRouteBuilder endpoints,
            string pattern,
            Delegate handler) =>
            endpoints.MapPost(pattern, handler)
                .ProducesPost()
                .Produces(StatusCodes.Status201Created);

        /// <summary>
        /// Maps a POST endpoint with OpenAPI documentation for updating an item.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder.</param>
        /// <param name="pattern">The URL pattern of the endpoint.</param>
        /// <param name="handler">The delegate that handles the endpoint.</param>
        /// <returns>The route handler builder.</returns>
        public static RouteHandlerBuilder MapPostWithUpdatedOpenApi(
            this IEndpointRouteBuilder endpoints,
            string pattern,
            Delegate handler) =>
            endpoints.MapPost(pattern, handler)
                .ProducesPost()
                .Produces(StatusCodes.Status204NoContent);

        /// <summary>
        /// Configures the response types for PUT endpoints that update a single item.
        /// </summary>
        /// <param name="builder">The route handler builder.</param>
        /// <returns>The updated route handler builder.</returns>
        public static RouteHandlerBuilder ProducesPut(this RouteHandlerBuilder builder) =>
            builder
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .ProducesValidationProblem()
                .ProducesProblem(StatusCodes.Status500InternalServerError);

        /// <summary>
        /// Maps a PUT endpoint with OpenAPI documentation.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder.</param>
        /// <param name="pattern">The URL pattern of the endpoint.</param>
        /// <param name="handler">The delegate that handles the endpoint.</param>
        /// <returns>The route handler builder.</returns>
        public static RouteHandlerBuilder MapPutWithOpenApi(
            this IEndpointRouteBuilder endpoints,
            string pattern,
            Delegate handler) =>
            endpoints.MapPut(pattern, handler)
                .ProducesPut();

        /// <summary>
        /// Configures the response types for DELETE endpoints that delete a single item.
        /// </summary>
        /// <param name="builder">The route handler builder.</param>
        /// <returns>The updated route handler builder.</returns>
        public static RouteHandlerBuilder ProducesDelete(this RouteHandlerBuilder builder) =>
            builder
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .ProducesValidationProblem()
                .ProducesProblem(StatusCodes.Status500InternalServerError);

        /// <summary>
        /// Maps a DELETE endpoint with OpenAPI documentation.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder.</param>
        /// <param name="pattern">The URL pattern of the endpoint.</param>
        /// <param name="handler">The delegate that handles the endpoint.</param>
        /// <returns>The route handler builder.</returns>
        public static RouteHandlerBuilder MapDeleteWithOpenApi(
            this IEndpointRouteBuilder endpoints,
            string pattern,
            Delegate handler) =>
            endpoints.MapDelete(pattern, handler)
                .ProducesDelete();
    }
}
