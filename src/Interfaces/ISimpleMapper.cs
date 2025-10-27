using System.Collections.Generic;

namespace Simple.AutoMapper.Interfaces
{
    /// <summary>
    /// Dependency Injection-friendly mapper interface.
    /// Provides instance-based mapping operations that can be injected via DI.
    /// </summary>
    public interface ISimpleMapper
    {
        /// <summary>
        /// Maps a single source instance to destination type.
        /// </summary>
        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TDestination">Destination type. Must have parameterless constructor.</typeparam>
        /// <param name="source">Source instance to map from.</param>
        /// <returns>Mapped destination instance, or default if source is null.</returns>
        TDestination Map<TSource, TDestination>(TSource source)
            where TDestination : new();

        /// <summary>
        /// Maps a single object to destination type by inferring the source type from the object instance.
        /// </summary>
        /// <typeparam name="TDestination">Destination type. Must have parameterless constructor.</typeparam>
        /// <param name="source">Source instance to map from.</param>
        /// <returns>Mapped destination instance, or default if source is null.</returns>
        TDestination Map<TDestination>(object source)
            where TDestination : new();

        /// <summary>
        /// Maps a collection of source items to a list of destination items.
        /// </summary>
        /// <typeparam name="TSource">Source item type.</typeparam>
        /// <typeparam name="TDestination">Destination item type. Must have parameterless constructor.</typeparam>
        /// <param name="sourceList">Source collection to map from.</param>
        /// <returns>List of mapped destination items.</returns>
        List<TDestination> Map<TSource, TDestination>(IEnumerable<TSource> sourceList)
            where TDestination : new();

        /// <summary>
        /// Updates an existing destination instance with values from source.
        /// Only maps properties that exist in both source and destination.
        /// </summary>
        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TDestination">Destination type.</typeparam>
        /// <param name="source">Source instance to map from.</param>
        /// <param name="destination">Destination instance to update.</param>
        void MapTo<TSource, TDestination>(TSource source, TDestination destination);

        /// <summary>
        /// Updates an existing destination instance with values from source (non-generic version).
        /// </summary>
        /// <param name="source">Source instance to map from.</param>
        /// <param name="destination">Destination instance to update.</param>
        void MapTo(object source, object destination);
    }
}
