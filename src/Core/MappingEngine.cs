using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Simple.AutoMapper.Intefaces;
using Simple.AutoMapper.Internal;
using Simple.AutoMapper.Core;

namespace Simple.AutoMapper.Core
{
    /// <summary>
    /// Mapping engine with pre-compiled mappings for better performance
    /// </summary>
    public class MappingEngine
    {
        private readonly ConcurrentDictionary<Internal.TypePair, IMappingExpression> _mappingExpressions = new();
        private readonly ConcurrentDictionary<Internal.TypePair, Delegate> _compiledMappings = new();
        private readonly object _compilationLock = new();

        /// <summary>
        /// Create a mapping configuration between source and destination types
        /// </summary>
        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
            where TDestination : new()
        {
            var typePair = new Internal.TypePair(typeof(TSource), typeof(TDestination));
            var expression = new MappingExpression<TSource, TDestination>(this);
            _mappingExpressions[typePair] = expression;
            return expression;
        }

        /// <summary>
        /// Map a single object using pre-compiled mapping (internal use)
        /// </summary>
        internal TDestination MapItem<TSource, TDestination>(TSource source)
                where TDestination : new()
        {
            if (source == null)
                return default(TDestination);

            var typePair = new Internal.TypePair(typeof(TSource), typeof(TDestination));
            var mapper = GetOrCompileMapper<TSource, TDestination>(typePair);
            return mapper(source);
        }

        /// <summary>
        /// Map a collection using pre-compiled mapping (internal use)
        /// </summary>
        internal List<TDestination> MapList<TSource, TDestination>(IEnumerable<TSource> sourceList)
                where TDestination : new()
        {
            if (sourceList == null)
                return null;

            var mapper = GetOrCompileMapper<TSource, TDestination>(new Internal.TypePair(typeof(TSource), typeof(TDestination)));
            return sourceList.Select(mapper).ToList();
        }

        /// <summary>
        /// Get or compile a mapper function for the given type pair
        /// </summary>
        private Func<TSource, TDestination> GetOrCompileMapper<TSource, TDestination>(Internal.TypePair typePair)
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
        private Func<TSource, TDestination> CompileMapper<TSource, TDestination>(Internal.TypePair typePair)
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

                        var mapMethod = typeof(MappingEngine).GetMethod(nameof(MapItem))
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

                            // Use MapList for collection mapping
                            var mapListMethod = typeof(MappingEngine).GetMethod(nameof(MapList))
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
    }
}