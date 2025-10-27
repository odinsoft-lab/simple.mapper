using System;
using System.Linq;

#if NET8_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
using Microsoft.Extensions.DependencyInjection;
#endif

namespace Simple.AutoMapper.DependencyInjection
{
    /// <summary>
    /// Extension methods for configuring Simple.AutoMapper in IServiceCollection.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
#if NET8_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Adds Simple.AutoMapper configuration to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configure">Configuration action for mapper profiles.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddSimpleMapper(
            this IServiceCollection services,
            Action<MapperConfiguration> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var config = new MapperConfiguration();
            configure(config);

            return services;
        }

        /// <summary>
        /// Adds Simple.AutoMapper configuration to the service collection.
        /// Automatically scans and registers all Profile types from the specified assemblies.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assemblies">Assemblies to scan for Profile types.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddSimpleMapper(
            this IServiceCollection services,
            params System.Reflection.Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
                throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies));

            var profileType = typeof(Core.Profile);

            foreach (var assembly in assemblies)
            {
                var profileTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && profileType.IsAssignableFrom(t));

                foreach (var type in profileTypes)
                {
                    var profile = (Core.Profile)Activator.CreateInstance(type);
                    Core.Mapper.AddProfile(profile);
                }
            }

            return services;
        }
#endif
    }
}
