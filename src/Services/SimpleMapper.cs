using Simple.AutoMapper.Interfaces;
using System.Collections.Generic;

namespace Simple.AutoMapper.Services
{
    /// <summary>
    /// Injectable mapper service that wraps the static Mapper API.
    /// Implements ISimpleMapper for dependency injection scenarios.
    /// </summary>
    public class SimpleMapper : ISimpleMapper
    {
        /// <summary>
        /// Maps a single source instance to destination type.
        /// </summary>
        public TDestination Map<TSource, TDestination>(TSource source)
            where TDestination : new()
        {
            return Core.Mapper.Map<TSource, TDestination>(source);
        }

        /// <summary>
        /// Maps a single object to destination type by inferring the source type.
        /// </summary>
        public TDestination Map<TDestination>(object source)
            where TDestination : new()
        {
            return Core.Mapper.Map<TDestination>(source);
        }

        /// <summary>
        /// Maps a collection of source items to a list of destination items.
        /// </summary>
        public List<TDestination> Map<TSource, TDestination>(IEnumerable<TSource> sourceList)
            where TDestination : new()
        {
            return Core.Mapper.Map<TSource, TDestination>(sourceList);
        }

        /// <summary>
        /// Updates an existing destination instance with values from source.
        /// </summary>
        public void Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            Core.Mapper.Map(source, destination);
        }

        /// <summary>
        /// Creates a new destination instance and patches it with non-null values from source.
        /// </summary>
        public TDestination Patch<TSource, TDestination>(TSource source)
            where TDestination : new()
        {
            return Core.Mapper.Patch<TSource, TDestination>(source);
        }

        /// <summary>
        /// Creates a new destination instance and patches it with non-null values from source (non-generic).
        /// </summary>
        public TDestination Patch<TDestination>(object source)
            where TDestination : new()
        {
            return Core.Mapper.Patch<TDestination>(source);
        }

        /// <summary>
        /// Patches a collection of source items to a list of destination items with null-skip semantics.
        /// </summary>
        public List<TDestination> Patch<TSource, TDestination>(IEnumerable<TSource> sourceList)
            where TDestination : new()
        {
            return Core.Mapper.Patch<TSource, TDestination>(sourceList);
        }

        /// <summary>
        /// Patches an existing destination instance with non-null values from source.
        /// </summary>
        public void Patch<TSource, TDestination>(TSource source, TDestination destination)
        {
            Core.Mapper.Patch(source, destination);
        }
    }
}
