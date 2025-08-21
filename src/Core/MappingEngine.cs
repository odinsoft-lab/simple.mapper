using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Simple.AutoMapper.Intefaces;
using Simple.AutoMapper.Internal;

namespace Simple.AutoMapper.Core
{
    /// <summary>
    /// High-performance mapping engine with both static and instance-based APIs
    /// Provides reflection-based and compiled mapping capabilities
    /// </summary>
    public class MappingEngine
    {
        // Singleton instance for static API
        private static readonly Lazy<MappingEngine> _defaultInstance = new Lazy<MappingEngine>(() => new MappingEngine());
        
        // Configuration and compilation caches
        private readonly ConcurrentDictionary<TypePair, IMappingExpression> _mappingExpressions = new();
        private readonly ConcurrentDictionary<TypePair, Delegate> _compiledMappings = new();
        private readonly object _compilationLock = new();

        /// <summary>
        /// Get the default singleton instance
        /// </summary>
        public static MappingEngine Default => _defaultInstance.Value;

        #region Static API Methods

        /// <summary>
        /// Map a single entity to DTO using static API
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
            if (source == null)
                return default(TDestination);

            var destination = new TDestination();
            MapPropertiesReflection(source, destination);
            return destination;
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
            if (source == null || destination == null) return;
            MapPropertiesGeneric(source, destination);
        }

        #endregion

        #region Instance API Methods

        /// <summary>
        /// Create a mapping configuration between source and destination types
        /// </summary>
        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
            where TDestination : new()
        {
            var typePair = new TypePair(typeof(TSource), typeof(TDestination));
            var expression = new MappingExpression<TSource, TDestination>(this);
            _mappingExpressions[typePair] = expression;
            // Clear compiled mapping to force recompilation with new configuration
            _compiledMappings.TryRemove(typePair, out _);
            return expression;
        }

        /// <summary>
        /// Map a single object using instance API
        /// </summary>
        public TDestination MapInstance<TSource, TDestination>(TSource source)
            where TDestination : new()
        {
            if (source == null)
                return default(TDestination);

            var typePair = new TypePair(typeof(TSource), typeof(TDestination));
            var mapper = GetOrCompileMapper<TSource, TDestination>(typePair);
            return mapper(source);
        }

        /// <summary>
        /// Map a collection using instance API
        /// </summary>
        public List<TDestination> MapCollection<TSource, TDestination>(IEnumerable<TSource> sourceList)
            where TDestination : new()
        {
            if (sourceList == null)
                return null;

            var mapper = GetOrCompileMapper<TSource, TDestination>(new TypePair(typeof(TSource), typeof(TDestination)));
            return sourceList.Select(mapper).ToList();
        }

        #endregion

        #region Internal Methods (Used by both Static and Instance APIs)

        /// <summary>
        /// Map a single object using pre-compiled mapping (internal use for backward compatibility)
        /// </summary>
        internal TDestination MapItem<TSource, TDestination>(TSource source)
            where TDestination : new()
        {
            return MapInstance<TSource, TDestination>(source);
        }


        #endregion

        #region Compilation and Mapping Logic

        /// <summary>
        /// Get or compile a mapper function for the given type pair
        /// </summary>
        private Func<TSource, TDestination> GetOrCompileMapper<TSource, TDestination>(TypePair typePair)
            where TDestination : new()
        {
            if (_compiledMappings.TryGetValue(typePair, out var cached))
            {
                return (Func<TSource, TDestination>)cached;
            }

            lock (_compilationLock)
            {
                if (_compiledMappings.TryGetValue(typePair, out cached))
                {
                    return (Func<TSource, TDestination>)cached;
                }

                var mapper = CompileMapper<TSource, TDestination>(typePair);
                _compiledMappings[typePair] = mapper;
                return mapper;
            }
        }

        /// <summary>
        /// Compile a mapper function using expression trees
        /// </summary>
        private Func<TSource, TDestination> CompileMapper<TSource, TDestination>(TypePair typePair)
            where TDestination : new()
        {
            var sourceParam = Expression.Parameter(typeof(TSource), "source");
            var destinationVar = Expression.Variable(typeof(TDestination), "destination");

            var expressions = new List<Expression>();

            // Create new instance: var destination = new TDestination();
            expressions.Add(Expression.Assign(destinationVar, Expression.New(typeof(TDestination))));

            // Get mapping configuration if exists
            _mappingExpressions.TryGetValue(typePair, out var mappingExpression);
            var config = mappingExpression as MappingExpression<TSource, TDestination>;

            // Map properties
            var sourceProperties = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var destinationProperties = typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToDictionary(p => p.Name, p => p);

            foreach (var sourceProperty in sourceProperties)
            {
                if (!sourceProperty.CanRead)
                    continue;

                // Check if property is ignored in configuration
                if (config?.IsPropertyIgnored(sourceProperty.Name) == true)
                    continue;

                // Check for custom member configuration
                var customMapping = config?.GetCustomMapping(sourceProperty.Name);
                if (customMapping != null)
                {
                    // Handle custom mapping (not implemented in this version)
                    continue;
                }

                if (destinationProperties.TryGetValue(sourceProperty.Name, out var destinationProperty))
                {
                    var sourceValue = Expression.Property(sourceParam, sourceProperty);
                    var destinationValue = Expression.Property(destinationVar, destinationProperty);

                    // Handle simple types
                    if (IsSimpleType(sourceProperty.PropertyType) && sourceProperty.PropertyType == destinationProperty.PropertyType)
                    {
                        expressions.Add(Expression.Assign(destinationValue, sourceValue));
                    }
                    // Handle nullable to non-nullable or vice versa for simple types
                    else if (IsSimpleType(GetUnderlyingType(sourceProperty.PropertyType)) &&
                             IsSimpleType(GetUnderlyingType(destinationProperty.PropertyType)))
                    {
                        var convertedValue = Expression.Convert(sourceValue, destinationProperty.PropertyType);
                        expressions.Add(Expression.Assign(destinationValue, convertedValue));
                    }
                    // Handle complex types
                    else if (IsComplexType(sourceProperty.PropertyType) && IsComplexType(destinationProperty.PropertyType))
                    {
                        // Create a null check and recursive mapping
                        var nullCheck = Expression.NotEqual(sourceValue, Expression.Constant(null, sourceProperty.PropertyType));

                        var mapMethod = typeof(MappingEngine).GetMethod(nameof(MapInstance))
                            .MakeGenericMethod(sourceProperty.PropertyType, destinationProperty.PropertyType);

                        var mappedValue = Expression.Call(
                            Expression.Constant(this),
                            mapMethod,
                            sourceValue
                        );

                        var conditionalAssign = Expression.IfThen(
                            nullCheck,
                            Expression.Assign(destinationValue, mappedValue)
                        );

                        expressions.Add(conditionalAssign);
                    }
                    // Handle collections
                    else if (IsCollectionType(sourceProperty.PropertyType) && IsCollectionType(destinationProperty.PropertyType))
                    {
                        var sourceElementType = GetCollectionElementType(sourceProperty.PropertyType);
                        var destinationElementType = GetCollectionElementType(destinationProperty.PropertyType);

                        if (sourceElementType != null && destinationElementType != null)
                        {
                            var nullCheck = Expression.NotEqual(sourceValue, Expression.Constant(null, sourceProperty.PropertyType));

                            // Use MapCollection for collection mapping
                            var mapListMethod = typeof(MappingEngine).GetMethod(nameof(MapCollection))
                                .MakeGenericMethod(sourceElementType, destinationElementType);

                            var mappedCollection = Expression.Call(
                                Expression.Constant(this),
                                mapListMethod,
                                sourceValue
                            );

                            var conditionalAssign = Expression.IfThen(
                                nullCheck,
                                Expression.Assign(destinationValue, mappedCollection)
                            );

                            expressions.Add(conditionalAssign);
                        }
                    }
                }
            }

            // Return destination
            expressions.Add(destinationVar);

            var body = Expression.Block(new[] { destinationVar }, expressions);
            var lambda = Expression.Lambda<Func<TSource, TDestination>>(body, sourceParam);

            return lambda.Compile();
        }

        #endregion

        #region Reflection-based Mapping (for non-generic overloads)

        /// <summary>
        /// Map properties from source to destination (non-generic version)
        /// </summary>
        private static void MapPropertiesReflection(object source, object destination)
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
                        // Handle nested complex types
                        else if (IsComplexType(sourceProperty.PropertyType) && IsComplexType(destinationProperty.PropertyType))
                        {
                            var mappedValue = MapComplexTypeReflection(sourceValue, sourceProperty.PropertyType, destinationProperty.PropertyType);
                            destinationProperty.SetValue(destination, mappedValue);
                        }
                        // Handle collections
                        else if (IsCollectionType(sourceProperty.PropertyType) && IsCollectionType(destinationProperty.PropertyType))
                        {
                            var mappedCollection = MapCollectionReflection(sourceValue, sourceProperty.PropertyType, destinationProperty.PropertyType);
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
        /// Map properties from source to destination (generic version)
        /// </summary>
        private static void MapPropertiesGeneric<TSource, TDestination>(TSource source, TDestination destination)
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
                        // Handle nested complex types
                        else if (IsComplexType(sourceProperty.PropertyType) && IsComplexType(destinationProperty.PropertyType))
                        {
                            var mappedValue = MapComplexTypeReflection(sourceValue, sourceProperty.PropertyType, destinationProperty.PropertyType);
                            destinationProperty.SetValue(destination, mappedValue);
                        }
                        // Handle collections
                        else if (IsCollectionType(sourceProperty.PropertyType) && IsCollectionType(destinationProperty.PropertyType))
                        {
                            var mappedCollection = MapCollectionReflection(sourceValue, sourceProperty.PropertyType, destinationProperty.PropertyType);
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
        /// Map complex nested types using reflection
        /// </summary>
        private static object MapComplexTypeReflection(object sourceValue, Type sourceType, Type destinationType)
        {
            if (sourceValue == null)
                return null;

            // Create instance of destination type
            var destination = Activator.CreateInstance(destinationType);

            // Use reflection to call MapPropertiesReflection
            MapPropertiesReflection(sourceValue, destination);

            return destination;
        }

        /// <summary>
        /// Map collections using reflection
        /// </summary>
        private static object MapCollectionReflection(object sourceCollection, Type sourceType, Type destinationType)
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
                        var mappedItem = MapComplexTypeReflection(item, sourceElementType, destinationElementType);
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
                        var mappedItem = MapComplexTypeReflection(item, sourceElementType, destinationElementType);
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

        #endregion

        #region Type Helper Methods

        private static Type GetUnderlyingType(Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

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

        private static bool IsComplexType(Type type)
        {
            return type.IsClass && !IsSimpleType(type) && !IsCollectionType(type);
        }

        private static bool IsCollectionType(Type type)
        {
            return type != typeof(string) && (
                type.IsArray ||
                (type.IsGenericType &&
                 (type.GetGenericTypeDefinition() == typeof(List<>) ||
                  type.GetGenericTypeDefinition() == typeof(IList<>) ||
                  type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                  type.GetGenericTypeDefinition() == typeof(ICollection<>))));
        }

        private static Type GetCollectionElementType(Type collectionType)
        {
            if (collectionType.IsArray)
                return collectionType.GetElementType();

            if (collectionType.IsGenericType)
                return collectionType.GetGenericArguments()[0];

            return null;
        }

        #endregion
    }
}