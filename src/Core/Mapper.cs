using Simple.AutoMapper.Interfaces;
using System.Collections.Generic;

namespace Simple.AutoMapper.Core
{
    /// <summary>
    /// Static mapper facade providing convenient access to MappingEngine functionality
    /// </summary>
    public static class Mapper
    {
        /// <summary>
        /// Get the default singleton instance (internal to keep MappingEngine hidden from consumers)
        /// </summary>
        internal static MappingEngine Default => MappingEngine.Instance;

        /// <summary>
        /// Create a mapping configuration between source and destination types
        /// </summary>
        public static IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
            where TDestination : new()
        {
            return Default.CreateMap<TSource, TDestination>();
        }

        /// <summary>
        /// Map a single entity to DTO
        /// </summary>
        public static TDestination Map<TSource, TDestination>(TSource source)
            where TDestination : new()
        {
            return Default.MapInstance<TSource, TDestination>(source);
        }

        /// <summary>
        /// Map a single entity to DTO (infers source type from parameter)
        /// </summary>
        public static TDestination Map<TDestination>(object source)
            where TDestination : new()
        {
            return Default.MapPropertiesReflection<TDestination>(source);
        }

        /// <summary>
        /// Map a collection of TSource to a List of TDestination
        /// </summary>
        public static List<TDestination> Map<TSource, TDestination>(IEnumerable<TSource> sourceList)
            where TDestination : new()
        {
            return Default.MapCollection<TSource, TDestination>(sourceList);
        }

        /// <summary>
        /// In-place update of an existing destination from source
        /// </summary>
        public static void Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            Default.MapPropertiesGeneric(source, destination);
        }

        /// <summary>
        /// For tests only: reset the underlying singleton state
        /// </summary>
        internal static MappingEngine Reset()
        {
            Default.Reset();

            return Default;
        }
    }
}