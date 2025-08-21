using Simple.AutoMapper.Interfaces;
using System.Collections.Generic;

namespace Simple.AutoMapper.Core
{
    /// <summary>
    /// Static mapper facade for configuring and executing object mappings.
    /// Exposes simple, reflection-based APIs over the internal MappingEngine.
    /// </summary>
    public static class Mapper
    {
        /// <summary>
        /// Gets the default singleton instance of the mapping engine.
        /// Internal to keep <see cref="MappingEngine"/> hidden from package consumers.
        /// </summary>
        internal static MappingEngine Default => MappingEngine.Instance;

        /// <summary>
        /// Creates a mapping configuration between source and destination types.
        /// Define ignores and member rules via the returned expression.
        /// </summary>
        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TDestination">Destination type. Must have parameterless constructor.</typeparam>
        /// <returns>Mapping expression to configure the map.</returns>
        public static IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
            where TDestination : new()
        {
            return Default.CreateMap<TSource, TDestination>();
        }

        /// <summary>
        /// Maps a single <typeparamref name="TSource"/> instance to <typeparamref name="TDestination"/>.
        /// </summary>
        /// <param name="source">Source instance to map from.</param>
        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TDestination">Destination type. Must have parameterless constructor.</typeparam>
        /// <returns>Mapped destination instance, or default if source is null.</returns>
        public static TDestination Map<TSource, TDestination>(TSource source)
            where TDestination : new()
        {
            return Default.MapInstance<TSource, TDestination>(source);
        }

        /// <summary>
        /// Maps a single object to <typeparamref name="TDestination"/> by inferring the source type from the object instance.
        /// </summary>
        /// <param name="source">Source instance to map from.</param>
        /// <typeparam name="TDestination">Destination type. Must have parameterless constructor.</typeparam>
        /// <returns>Mapped destination instance, or default if source is null.</returns>
        public static TDestination Map<TDestination>(object source)
            where TDestination : new()
        {
            return Default.MapPropertiesReflection<TDestination>(source);
        }

        /// <summary>
        /// Maps a collection of <typeparamref name="TSource"/> to a List of <typeparamref name="TDestination"/>.
        /// </summary>
        /// <param name="sourceList">Source collection to map.</param>
        /// <typeparam name="TSource">Source element type.</typeparam>
        /// <typeparam name="TDestination">Destination element type. Must have parameterless constructor.</typeparam>
        /// <returns>List of mapped destination instances, or null if sourceList is null.</returns>
        public static List<TDestination> Map<TSource, TDestination>(IEnumerable<TSource> sourceList)
            where TDestination : new()
        {
            return Default.MapCollection<TSource, TDestination>(sourceList);
        }

        /// <summary>
        /// Performs an in-place update of an existing <paramref name="destination"/> instance from the <paramref name="source"/>.
        /// </summary>
        /// <param name="source">Source instance.</param>
        /// <param name="destination">Existing destination instance to be updated.</param>
        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TDestination">Destination type.</typeparam>
        public static void Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            Default.MapPropertiesGeneric(source, destination);
        }

        /// <summary>
        /// Resets the underlying singleton state. Intended for tests only.
        /// </summary>
        internal static MappingEngine Reset()
        {
            Default.Reset();

            return Default;
        }
    }
}