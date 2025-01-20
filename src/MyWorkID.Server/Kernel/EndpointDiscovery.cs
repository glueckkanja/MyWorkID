using MyWorkID.Server.Common;
using System.Reflection;

namespace MyWorkID.Server.Kernel
{
    /// <summary>
    /// Provides methods for discovering and registering endpoints.
    /// </summary>
    public static class EndpointDiscovery
    {
        private static readonly Type _endpointType = typeof(IEndpoint);

        /// <summary>
        /// Registers endpoints from the specified assemblies.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder.</param>
        /// <param name="assemblies">The assemblies to search for endpoints.</param>
        /// <exception cref="ArgumentException">Thrown when no assemblies are provided.</exception>
        public static void RegisterEndpoints(this IEndpointRouteBuilder endpoints, params Assembly[] assemblies)
        {
            if (assemblies.Length == 0)
            {
                throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies));
            }

            var endpointTypes = GetEndpointTypes(assemblies);

            foreach (var type in endpointTypes)
            {
                var method = GetMapEndpointMethod(type);
                method?.Invoke(null, new object[] { endpoints });
            }
        }

        /// <summary>
        /// Gets the types that implement the <see cref="IEndpoint"/> interface from the specified assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies to search for endpoint types.</param>
        /// <returns>A collection of types that implement the <see cref="IEndpoint"/> interface.</returns>
        private static IEnumerable<Type> GetEndpointTypes(params Assembly[] assemblies)
        {
            return assemblies.SelectMany(x => x.GetTypes())
                .Where(x => _endpointType.IsAssignableFrom(x) &&
                            x is { IsInterface: false, IsAbstract: false });
        }

        /// <summary>
        /// Gets the method to map the endpoint from the specified type.
        /// </summary>
        /// <param name="type">The type to search for the map endpoint method.</param>
        /// <returns>The method info for the map endpoint method, or null if not found.</returns>
        private static MethodInfo? GetMapEndpointMethod(Type type)
        {
            return type.GetMethod(nameof(IEndpoint.MapEndpoint),
                BindingFlags.Static | BindingFlags.Public);
        }
    }
}
