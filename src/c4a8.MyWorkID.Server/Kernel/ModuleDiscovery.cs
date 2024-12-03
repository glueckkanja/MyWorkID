using c4a8.MyWorkID.Server.Common;
using System.Reflection;

namespace c4a8.MyWorkID.Server.Kernel
{
    /// <summary>
    /// Provides methods for discovering and configuring modules.
    /// </summary>
    public static class ModuleDiscovery
    {
        private static readonly Type _moduleType = typeof(IModule);

        /// <summary>
        /// Configures modules from the specified assemblies.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configurationManager">The configuration manager.</param>
        /// <param name="environment">The web host environment.</param>
        /// <param name="assemblies">The assemblies to search for modules.</param>
        /// <exception cref="ArgumentException">Thrown when no assemblies are provided.</exception>
        public static void ConfigureModules(this IServiceCollection services, IConfigurationManager configurationManager, IWebHostEnvironment environment, params Assembly[] assemblies)
        {
            if (assemblies.Length == 0)
            {
                throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies));
            }

            var moduleTypes = GetModuleTypes(assemblies);

            foreach (var type in moduleTypes)
            {
                var method = GetModuleConfigureMethod(type);
                method?.Invoke(null, new object[] { services, configurationManager, environment });
            }
        }

        /// <summary>
        /// Gets the types that implement the <see cref="IModule"/> interface from the specified assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies to search for module types.</param>
        /// <returns>A collection of types that implement the <see cref="IModule"/> interface.</returns>
        private static IEnumerable<Type> GetModuleTypes(params Assembly[] assemblies)
        {
            return assemblies.SelectMany(x => x.GetTypes())
                .Where(x => _moduleType.IsAssignableFrom(x) &&
                            x is { IsInterface: false, IsAbstract: false });
        }

        /// <summary>
        /// Gets the method to configure the module from the specified type.
        /// </summary>
        /// <param name="type">The type to search for the configure method.</param>
        /// <returns>The method info for the configure method, or null if not found.</returns>
        private static MethodInfo? GetModuleConfigureMethod(Type type)
        {
            return type.GetMethod(nameof(IModule.ConfigureServices),
                BindingFlags.Static | BindingFlags.Public);
        }
    }
}
