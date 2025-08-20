using System.Collections.Generic;

namespace OdinMapper.Extensions
{
    /// <summary>
    /// Extension methods for OdinMapper
    /// </summary>
    public static class MapperExtensions
    {
        /// <summary>
        /// Map entity to DTO using extension method
        /// </summary>
        public static TDestination MapTo<TDestination>(this object source)
            where TDestination : new()
        {
            if (source == null)
                return default(TDestination);

            return Mapper.Map<object, TDestination>(source);
        }

        /// <summary>
        /// Map collection of entities to DTOs using extension method
        /// </summary>
        public static List<TDestination> MapToList<TDestination>(this System.Collections.IEnumerable source)
            where TDestination : new()
        {
            if (source == null)
                return null;

            var result = new List<TDestination>();
            foreach (var item in source)
            {
                result.Add(item.MapTo<TDestination>());
            }
            return result;
        }
    }
}