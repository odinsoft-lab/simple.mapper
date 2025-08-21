using System;
using System.Collections.Generic;

namespace Simple.AutoMapper.Core
{
    /// <summary>
    /// Static mapper facade providing convenient access to MappingEngine functionality
    /// </summary>
    public static class Mapper
    {
        /// <summary>
        /// Map a single entity to DTO
        /// </summary>
        public static TDestination Map<TSource, TDestination>(TSource source)
            where TDestination : new()
        {
            return MappingEngine.Map<TSource, TDestination>(source);
        }

        /// <summary>
        /// Map a single entity to DTO (infers source type from parameter)
        /// </summary>
        public static TDestination Map<TDestination>(object source)
            where TDestination : new()
        {
            return MappingEngine.Map<TDestination>(source);
        }

        /// <summary>
        /// Map a collection of TSource to a List of TDestination
        /// </summary>
        public static List<TDestination> Map<TSource, TDestination>(IEnumerable<TSource> sourceList)
            where TDestination : new()
        {
            return MappingEngine.Map<TSource, TDestination>(sourceList);
        }

        /// <summary>
        /// In-place update of an existing destination from source
        /// </summary>
        public static void Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            MappingEngine.Map<TSource, TDestination>(source, destination);
        }
    }
}