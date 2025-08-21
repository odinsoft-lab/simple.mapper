using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Simple.AutoMapper.Core
{
    /// <summary>
    /// Mapping utility for Entity to DTO conversion without Simple.AutoMapper configuration
    /// </summary>
    public static class Mapper
    {
        // Internal compiled mapping engine (singleton) for cached mappings
        private static readonly MappingEngine _engine = new MappingEngine();
        // SyncResult is now a top-level struct in the same namespace

        /// <summary>
        /// Map a single entity to DTO
        /// </summary>
        public static TDestination Map<TSource, TDestination>(TSource source)
            where TDestination : new()
        {
            if (source == null)
                return default(TDestination);

            // Use compiled, cached mapping
            return _engine.MapItem<TSource, TDestination>(source);
        }

        /// <summary>
        /// Map a single entity to DTO (infers source type from parameter)
        /// </summary>
        public static TDestination Map<TDestination>(object source)
            where TDestination : new()
        {
            if (source == null)
                return default(TDestination);

            // Use reflection-based non-generic path to avoid runtime generic inference pitfalls.
            var destination = new TDestination();
            MapPropertiesNonGeneric(source, destination);
            return destination;
        }

        /// <summary>
        /// Map a collection of TSource to a List of TDestination (unified name overload).
        /// </summary>
        public static List<TDestination> Map<TSource, TDestination>(IEnumerable<TSource> sourceList)
            where TDestination : new()
        {
            if (sourceList == null)
                return null;

            // Use compiled, cached list mapping
            return _engine.MapList<TSource, TDestination>(sourceList);
        }

        /// <summary>
        /// In-place update of an existing destination from source (unified name overload).
        /// </summary>
        public static void Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            if (source == null || destination == null) return;
            MapProperties(source, destination);
        }

        /// <summary>
        /// Map properties from source to destination (non-generic version)
        /// </summary>
        private static void MapPropertiesNonGeneric(object source, object destination)
        {
            if (source == null || destination == null)
                return;

            var sourceType = source.GetType();
            var destinationType = destination.GetType();

            var sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var destinationProperties = destinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToDictionary(p => p.Name, p => p);

            foreach (var sourceProperty in sourceProperties)
            {
                if (!sourceProperty.CanRead)
                    continue;

                if (destinationProperties.TryGetValue(sourceProperty.Name, out var destinationProperty))
                {
                    try
                    {
                        var sourceValue = sourceProperty.GetValue(source);

                        if (sourceValue == null)
                        {
                            destinationProperty.SetValue(destination, null);
                            continue;
                        }

                        // Handle value types and strings
                        if (IsSimpleType(sourceProperty.PropertyType))
                        {
                            if (sourceProperty.PropertyType == destinationProperty.PropertyType)
                            {
                                destinationProperty.SetValue(destination, sourceValue);
                            }
                        }
                        // Handle nested complex types (Entity to DTO mapping)
                        else if (IsComplexType(sourceProperty.PropertyType) && IsComplexType(destinationProperty.PropertyType))
                        {
                            var mappedValue = MapComplexType(sourceValue, sourceProperty.PropertyType, destinationProperty.PropertyType);
                            destinationProperty.SetValue(destination, mappedValue);
                        }
                        // Handle collections
                        else if (IsCollectionType(sourceProperty.PropertyType) && IsCollectionType(destinationProperty.PropertyType))
                        {
                            var mappedCollection = MapCollection(sourceValue, sourceProperty.PropertyType, destinationProperty.PropertyType);
                            destinationProperty.SetValue(destination, mappedCollection);
                        }
                    }
                    catch
                    {
                        // Skip properties that cannot be mapped
                    }
                }
            }
        }

        // // Core implementation for list synchronization used by Map overloads
        // private static SyncResult SyncCore<TDto, TEntity>(
        //     IEnumerable<TDto> dtoList,
        //     IList<TEntity> entityList,
        //     Func<TDto, object> dtoKeySelector,
        //     Func<TEntity, object> entityKeySelector,
        //     bool removeMissing)
        //     where TEntity : new()
        // {
        //     int added = 0, updated = 0, removed = 0;

        //     var dtos = dtoList ?? Enumerable.Empty<TDto>();
        //     var entities = entityList ?? throw new ArgumentNullException(nameof(entityList));

        //     var entityIndex = new Dictionary<object, TEntity>();
        //     foreach (var e in entities)
        //     {
        //         var key = entityKeySelector(e);
        //         if (key == null) continue;
        //         if (!entityIndex.ContainsKey(key)) entityIndex.Add(key, e);
        //     }

        //     var dtoKeys = new HashSet<object>();

        //     foreach (var dto in dtos)
        //     {
        //         var key = dtoKeySelector(dto);
        //         dtoKeys.Add(key);

        //         if (key != null && entityIndex.TryGetValue(key, out var existing))
        //         {
        //             MapProperties(dto, existing);
        //             updated++;
        //         }
        //         else
        //         {
        //             var created = new TEntity();
        //             MapProperties(dto, created);
        //             entities.Add(created);
        //             added++;
        //         }
        //     }

        //     if (removeMissing)
        //     {
        //         var toRemove = new List<TEntity>();
        //         foreach (var e in entities)
        //         {
        //             var key = entityKeySelector(e);
        //             if (!dtoKeys.Contains(key)) toRemove.Add(e);
        //         }
        //         foreach (var e in toRemove)
        //         {
        //             entities.Remove(e);
        //             removed++;
        //         }
        //     }

        //     return new SyncResult(added, updated, removed);
        // }

        // /// <summary>
        // /// Unified name overload for list synchronization with explicit key selectors.
        // /// </summary>
        // public static SyncResult Map<TDto, TEntity>(
        //     IEnumerable<TDto> dtoList,
        //     IList<TEntity> entityList,
        //     Func<TDto, object> dtoKeySelector,
        //     Func<TEntity, object> entityKeySelector,
        //     bool removeMissing = true)
        //     where TEntity : new()
        //     => SyncCore(dtoList, entityList, dtoKeySelector, entityKeySelector, removeMissing);

        // /// <summary>
        // /// Unified name overload for list synchronization with conventional key (default: "Id").
        // /// </summary>
        // public static SyncResult Map<TDto, TEntity>(
        //     IEnumerable<TDto> dtoList,
        //     IList<TEntity> entityList,
        //     string keyPropertyName = "Id",
        //     bool removeMissing = true)
        //     where TEntity : new()
        // {
        //     if (string.IsNullOrWhiteSpace(keyPropertyName)) keyPropertyName = "Id";
        //     var dtoKey = CreatePropertyKeySelector<TDto>(keyPropertyName);
        //     var entityKey = CreatePropertyKeySelector<TEntity>(keyPropertyName);
        //     return SyncCore(dtoList, entityList, dtoKey, entityKey, removeMissing);
        // }

        // private static Func<T, object> CreatePropertyKeySelector<T>(string propertyName)
        // {
        //     var type = typeof(T);
        //     var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        //     if (prop == null)
        //         throw new InvalidOperationException($"Type {type.FullName} does not have a property named '{propertyName}'.");

        //     return (T instance) => instance == null ? null : prop.GetValue(instance);
        // }

        /// <summary>
        /// Map properties from source to destination
        /// </summary>
        private static void MapProperties<TSource, TDestination>(TSource source, TDestination destination)
        {
            if (source == null || destination == null)
                return;

            var sourceType = typeof(TSource);
            var destinationType = typeof(TDestination);

            var sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var destinationProperties = destinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToDictionary(p => p.Name, p => p);

            foreach (var sourceProperty in sourceProperties)
            {
                if (!sourceProperty.CanRead)
                    continue;

                if (destinationProperties.TryGetValue(sourceProperty.Name, out var destinationProperty))
                {
                    try
                    {
                        var sourceValue = sourceProperty.GetValue(source);

                        if (sourceValue == null)
                        {
                            destinationProperty.SetValue(destination, null);
                            continue;
                        }

                        // Handle value types and strings
                        if (IsSimpleType(sourceProperty.PropertyType))
                        {
                            if (sourceProperty.PropertyType == destinationProperty.PropertyType)
                            {
                                destinationProperty.SetValue(destination, sourceValue);
                            }
                        }
                        // Handle nested complex types (Entity to DTO mapping)
                        else if (IsComplexType(sourceProperty.PropertyType) && IsComplexType(destinationProperty.PropertyType))
                        {
                            var mappedValue = MapComplexType(sourceValue, sourceProperty.PropertyType, destinationProperty.PropertyType);
                            destinationProperty.SetValue(destination, mappedValue);
                        }
                        // Handle collections
                        else if (IsCollectionType(sourceProperty.PropertyType) && IsCollectionType(destinationProperty.PropertyType))
                        {
                            var mappedCollection = MapCollection(sourceValue, sourceProperty.PropertyType, destinationProperty.PropertyType);
                            destinationProperty.SetValue(destination, mappedCollection);
                        }
                    }
                    catch
                    {
                        // Skip properties that cannot be mapped
                    }
                }
            }
        }

        /// <summary>
        /// Map complex nested types
        /// </summary>
        private static object MapComplexType(object sourceValue, Type sourceType, Type destinationType)
        {
            if (sourceValue == null)
                return null;

            // Create instance of destination type
            var destination = Activator.CreateInstance(destinationType);

            // Use reflection to call MapProperties with runtime types
            var mapMethod = typeof(Mapper).GetMethod(nameof(MapProperties), BindingFlags.NonPublic | BindingFlags.Static);
            var genericMethod = mapMethod.MakeGenericMethod(sourceType, destinationType);
            genericMethod.Invoke(null, new[] { sourceValue, destination });

            return destination;
        }

        /// <summary>
        /// Map collections
        /// </summary>
        private static object MapCollection(object sourceCollection, Type sourceType, Type destinationType)
        {
            if (sourceCollection == null)
                return null;

            var sourceElementType = GetCollectionElementType(sourceType);
            var destinationElementType = GetCollectionElementType(destinationType);

            if (sourceElementType == null || destinationElementType == null)
                return null;

            // Handle List<T>
            if (destinationType.IsGenericType && destinationType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var listType = typeof(List<>).MakeGenericType(destinationElementType);
                var list = Activator.CreateInstance(listType);
                var addMethod = listType.GetMethod("Add");

                foreach (var item in (System.Collections.IEnumerable)sourceCollection)
                {
                    if (item == null)
                    {
                        addMethod.Invoke(list, new object[] { null });
                    }
                    else if (IsSimpleType(sourceElementType))
                    {
                        addMethod.Invoke(list, new[] { item });
                    }
                    else
                    {
                        var mappedItem = MapComplexType(item, sourceElementType, destinationElementType);
                        addMethod.Invoke(list, new[] { mappedItem });
                    }
                }

                return list;
            }

            // Handle Arrays
            if (destinationType.IsArray)
            {
                var sourceList = new List<object>();
                foreach (var item in (System.Collections.IEnumerable)sourceCollection)
                {
                    if (item == null)
                    {
                        sourceList.Add(null);
                    }
                    else if (IsSimpleType(sourceElementType))
                    {
                        sourceList.Add(item);
                    }
                    else
                    {
                        var mappedItem = MapComplexType(item, sourceElementType, destinationElementType);
                        sourceList.Add(mappedItem);
                    }
                }

                var array = Array.CreateInstance(destinationElementType, sourceList.Count);
                for (int i = 0; i < sourceList.Count; i++)
                {
                    array.SetValue(sourceList[i], i);
                }

                return array;
            }

            return null;
        }

        /// <summary>
        /// Check if type is a simple type (value type or string)
        /// </summary>
        private static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive
                || type.IsEnum
                || type == typeof(string)
                || type == typeof(decimal)
                || type == typeof(DateTime)
                || type == typeof(DateTimeOffset)
                || type == typeof(TimeSpan)
                || type == typeof(Guid)
                || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && IsSimpleType(type.GetGenericArguments()[0]));
        }

        /// <summary>
        /// Check if type is a complex type (class but not string)
        /// </summary>
        private static bool IsComplexType(Type type)
        {
            return type.IsClass && !IsSimpleType(type) && !IsCollectionType(type);
        }

        /// <summary>
        /// Check if type is a collection type
        /// </summary>
        private static bool IsCollectionType(Type type)
        {
            return type.IsArray ||
                (type.IsGenericType &&
                 (type.GetGenericTypeDefinition() == typeof(List<>) ||
                  type.GetGenericTypeDefinition() == typeof(IList<>) ||
                  type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                  type.GetGenericTypeDefinition() == typeof(ICollection<>)));
        }

        /// <summary>
        /// Get element type of a collection
        /// </summary>
        private static Type GetCollectionElementType(Type collectionType)
        {
            if (collectionType.IsArray)
                return collectionType.GetElementType();

            if (collectionType.IsGenericType)
                return collectionType.GetGenericArguments()[0];

            return null;
        }
    }
}