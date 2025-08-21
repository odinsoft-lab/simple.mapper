using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Simple.AutoMapper.Interfaces;
using Simple.AutoMapper.Internal;

namespace Simple.AutoMapper.Core
{
    /// <summary>
    /// High-performance mapping engine with both and instance-based APIs
    /// Provides reflection-based and compiled mapping capabilities
    /// </summary>
    internal sealed class MappingEngine
    {
        // Singleton instance
        private static readonly Lazy<MappingEngine> _instance = new Lazy<MappingEngine>(() => new MappingEngine());
        internal static MappingEngine Instance => _instance.Value;

        // Configuration and compilation caches
        private readonly ConcurrentDictionary<TypePair, IMappingExpression> _mappingExpressions = new();
        private readonly ConcurrentDictionary<TypePair, Delegate> _compiledMappings = new();
        private readonly object _compilationLock = new();
        
        // Default max depth for recursive mapping
        private const int DefaultMaxDepth = 10;

        // Prevent external instantiation; enforce singleton
        private MappingEngine() { }

        // Test/support: reset all mappings and compiled delegates
        internal void Reset()
        {
            _mappingExpressions.Clear();
            _compiledMappings.Clear();
        }

        #region API Methods

        /// <summary>
        /// Map a single entity to DTO (infers source type from parameter)
        /// </summary>
        public TDestination MapPropertiesReflection<TDestination>(object source)
            where TDestination : new()
        {
            if (source == null)
                return default(TDestination);

            var destination = new TDestination();
            MapPropertiesReflection(source, destination);
            return destination;
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
            
            // Get configuration if exists
            var config = _mappingExpressions.TryGetValue(typePair, out var expr) ? expr : null;
            var context = new MappingContext(config?.MaxDepthValue ?? DefaultMaxDepth, config?.PreserveReferencesValue ?? false);
            
            return mapper(source, context);
        }
        
        /// <summary>
        /// Map a single object using instance API with mapping context
        /// </summary>
        private TDestination MapInstanceWithContext<TSource, TDestination>(TSource source, MappingContext context)
            where TDestination : new()
        {
            if (source == null)
                return default(TDestination);
                
            // Always check if we have a cached instance (for circular references)
            var cached = context.GetCachedDestination(source, typeof(TDestination));
            if (cached != null)
                return (TDestination)cached;

            var typePair = new TypePair(typeof(TSource), typeof(TDestination));
            var mapper = GetOrCompileMapper<TSource, TDestination>(typePair);
            return mapper(source, context);
        }

        /// <summary>
        /// Map a collection using instance API
        /// </summary>
        public List<TDestination> MapCollection<TSource, TDestination>(IEnumerable<TSource> sourceList)
            where TDestination : new()
        {
            if (sourceList == null)
                return null;

            var typePair = new TypePair(typeof(TSource), typeof(TDestination));
            var mapper = GetOrCompileMapper<TSource, TDestination>(typePair);
            
            // Get configuration if exists
            var config = _mappingExpressions.TryGetValue(typePair, out var expr) ? expr : null;
            var context = new MappingContext(config?.MaxDepthValue ?? DefaultMaxDepth, config?.PreserveReferencesValue ?? false);
            
            return sourceList.Select(s => mapper(s, context)).ToList();
        }

        #endregion

        #region Compilation and Mapping Logic

        /// <summary>
        /// Get or compile a mapper function for the given type pair
        /// </summary>
        private Func<TSource, MappingContext, TDestination> GetOrCompileMapper<TSource, TDestination>(TypePair typePair)
            where TDestination : new()
        {
            if (_compiledMappings.TryGetValue(typePair, out var cached))
            {
                return (Func<TSource, MappingContext, TDestination>)cached;
            }

            lock (_compilationLock)
            {
                if (_compiledMappings.TryGetValue(typePair, out cached))
                {
                    return (Func<TSource, MappingContext, TDestination>)cached;
                }

                var mapper = CompileMapper<TSource, TDestination>(typePair);
                _compiledMappings[typePair] = mapper;
                return mapper;
            }
        }

        /// <summary>
        /// Compile a mapper function using expression trees
        /// </summary>
        private Func<TSource, MappingContext, TDestination> CompileMapper<TSource, TDestination>(TypePair typePair)
            where TDestination : new()
        {
            var sourceParam = Expression.Parameter(typeof(TSource), "source");
            var contextParam = Expression.Parameter(typeof(MappingContext), "context");
            var destinationVar = Expression.Variable(typeof(TDestination), "destination");

            var expressions = new List<Expression>();

            // Create new instance: var destination = new TDestination();
            expressions.Add(Expression.Assign(destinationVar, Expression.New(typeof(TDestination))));
            
            // Cache the destination immediately to handle circular references
            // context.CacheDestination(source, typeof(TDestination), destination);
            var cacheMethod = typeof(MappingContext).GetMethod(nameof(MappingContext.CacheDestination));
            var cacheCall = Expression.Call(
                contextParam,
                cacheMethod,
                Expression.Convert(sourceParam, typeof(object)),
                Expression.Constant(typeof(TDestination)),
                Expression.Convert(destinationVar, typeof(object))
            );
            expressions.Add(cacheCall);

            // Get mapping configuration if exists
            IMappingExpression config = null;
            if (_mappingExpressions.TryGetValue(typePair, out var mappingExpression))
            {
                config = mappingExpression;
            }

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
                        // Create circular reference check using MappingContext
                        var nestedTypePair = new TypePair(sourceProperty.PropertyType, destinationProperty.PropertyType);
                        
                        // Build expression for circular reference check and mapping
                        // if (sourceValue != null && !context.IsCircularReference(sourceValue, nestedTypePair))
                        var nullCheck = Expression.NotEqual(sourceValue, Expression.Constant(null, sourceProperty.PropertyType));
                        
                        var isCircularMethod = typeof(MappingContext).GetMethod(nameof(MappingContext.IsCircularReference));
                        var circularCheck = Expression.Call(
                            contextParam,
                            isCircularMethod,
                            Expression.Convert(sourceValue, typeof(object)),
                            Expression.Constant(nestedTypePair)
                        );
                        
                        var notCircular = Expression.Not(circularCheck);
                        var canMap = Expression.AndAlso(nullCheck, notCircular);

                        // Call MapInstanceWithContext for recursive mapping
                        var mapMethod = typeof(MappingEngine).GetMethod(nameof(MapInstanceWithContext), BindingFlags.NonPublic | BindingFlags.Instance)
                            .MakeGenericMethod(sourceProperty.PropertyType, destinationProperty.PropertyType);

                        var mappedValue = Expression.Call(
                            Expression.Constant(this),
                            mapMethod,
                            sourceValue,
                            contextParam
                        );

                        var conditionalAssign = Expression.IfThen(
                            canMap,
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
            var lambda = Expression.Lambda<Func<TSource, MappingContext, TDestination>>(body, sourceParam, contextParam);

            return lambda.Compile();
        }

        #endregion

        #region Reflection-based Mapping (for non-generic overloads)

        /// <summary>
        /// Map properties from source to destination (non-generic version)
        /// </summary>
        private void MapPropertiesReflection(object source, object destination)
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
        public void MapPropertiesGeneric<TSource, TDestination>(TSource source, TDestination destination)
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
        private object MapComplexTypeReflection(object sourceValue, Type sourceType, Type destinationType)
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
        private object MapCollectionReflection(object sourceCollection, Type sourceType, Type destinationType)
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

        private Type GetUnderlyingType(Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        private bool IsSimpleType(Type type)
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

        private bool IsComplexType(Type type)
        {
            return type.IsClass && !IsSimpleType(type) && !IsCollectionType(type);
        }

        private bool IsCollectionType(Type type)
        {
            return type != typeof(string) && (
                type.IsArray ||
                (type.IsGenericType &&
                 (type.GetGenericTypeDefinition() == typeof(List<>) ||
                  type.GetGenericTypeDefinition() == typeof(IList<>) ||
                  type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                  type.GetGenericTypeDefinition() == typeof(ICollection<>))));
        }

        private Type GetCollectionElementType(Type collectionType)
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