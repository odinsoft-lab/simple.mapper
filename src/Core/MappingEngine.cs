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
    /// High-performance mapping engine providing reflection-based and compiled mapping capabilities.
    /// This type is internal; consumers interact through the public <see cref="Mapper"/> facade.
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
        /// Maps a single object to <typeparamref name="TDestination"/> by inferring the source type from the object instance.
        /// </summary>
        /// <param name="source">The source object instance.</param>
        /// <typeparam name="TDestination">Destination type to create and map to.</typeparam>
        /// <returns>New destination instance with mapped values, or default if source is null.</returns>
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
        /// Creates a mapping configuration between source and destination types.
        /// Stores configuration for later use by compiled mappers.
        /// </summary>
        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TDestination">Destination type.</typeparam>
        /// <returns>Mapping expression to configure ignores and member rules.</returns>
        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
            where TDestination : new()
        {
            var typePair = new TypePair(typeof(TSource), typeof(TDestination));

            // Replace any existing configuration first to avoid reading a stale one during concurrent tests
            var expression = new MappingExpression<TSource, TDestination>(this);
            _mappingExpressions[typePair] = expression;

            // Clear compiled mapping to force recompilation with new configuration
            _compiledMappings.TryRemove(typePair, out _); 

            return expression;
        }

        /// <summary>
        /// Maps a single object using the instance API.
        /// </summary>
        /// <param name="source">Source instance to map.</param>
        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TDestination">Destination type.</typeparam>
        /// <returns>New destination instance with mapped values, or default if source is null.</returns>
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
        /// Maps a collection using the instance API.
        /// </summary>
        /// <param name="sourceList">Source collection to map.</param>
        /// <typeparam name="TSource">Source element type.</typeparam>
        /// <typeparam name="TDestination">Destination element type.</typeparam>
        /// <returns>List of mapped destination elements, or null if sourceList is null.</returns>
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
        /// Gets or compiles a mapper function for the given type pair.
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
        /// Compiles a mapper function using expression trees for the specified type pair.
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
            var sourceProperties = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .ToDictionary(p => p.Name, p => p);
            var destinationProperties = typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToDictionary(p => p.Name, p => p);

            foreach (var kv in destinationProperties)
            {
                var destProperty = kv.Value;
                var destinationValue = Expression.Property(destinationVar, destProperty);

                // Ignore takes precedence
                if (config?.IsPropertyIgnored(destProperty.Name) == true)
                    continue;

                // Custom mapping via ForMember(MapFrom/Ignore)
                var customConfig = config?.GetCustomMapping(destProperty.Name);
                if (customConfig != null)
                {
                    var cfgType = customConfig.GetType();
                    var isIgnoredProp = cfgType.GetProperty("IsIgnored");
                    var sourceExprProp = cfgType.GetProperty("SourceExpression");

                    var isIgnored = isIgnoredProp != null && (bool)isIgnoredProp.GetValue(customConfig);
                    if (isIgnored)
                        continue;

                    var srcSelector = sourceExprProp?.GetValue(customConfig) as LambdaExpression;
                    if (srcSelector != null)
                    {
                        // Replace the parameter in the lambda expression with our sourceParam
                        var replacer = new ParameterReplaceVisitor(srcSelector.Parameters[0], sourceParam);
                        var replacedBody = replacer.Visit(srcSelector.Body);
                        var projected = replacedBody;
                        var projectedType = replacedBody.Type;

                        // Simple/convertible assignment
                        if (IsSimpleType(GetUnderlyingType(projectedType)) && IsSimpleType(GetUnderlyingType(destProperty.PropertyType)))
                        {
                            Expression converted;
                            if (projectedType == destProperty.PropertyType)
                            {
                                converted = projected;
                            }
                            else
                            {
                                converted = Expression.Convert(projected, destProperty.PropertyType);
                            }

                            // Use a temp variable to guarantee evaluation of the projection before assignment
                            var tmp = Expression.Variable(destProperty.PropertyType, "_mfS");
                            var assignTmp = Expression.Assign(tmp, converted);
                            var assignDest = Expression.Assign(destinationValue, tmp);
                            expressions.Add(Expression.Block(new[] { tmp }, assignTmp, assignDest));
                            continue;
                        }

                        // Simple types: direct assignment or conversion
                        if (IsSimpleType(projectedType))
                        {
                            if (projectedType == destProperty.PropertyType)
                            {
                                // Direct assignment when types match
                                expressions.Add(Expression.Assign(destinationValue, projected));
                            }
                            else if (IsSimpleType(destProperty.PropertyType))
                            {
                                // Convert between compatible simple types
                                var convertedValue = Expression.Convert(projected, destProperty.PropertyType);
                                expressions.Add(Expression.Assign(destinationValue, convertedValue));
                            }
                            continue;
                        }

                        // Complex types: guard circular refs and map recursively, with cached reuse when preserving references
                        if (IsComplexType(projectedType) && IsComplexType(destProperty.PropertyType))
                        {
                            var valueVar = Expression.Variable(projectedType, "_mf");
                            var assignValue = Expression.Assign(valueVar, projected);

                            var nestedTypePair = new TypePair(projectedType, destProperty.PropertyType);
                            var nullCheck = Expression.NotEqual(valueVar, Expression.Constant(null, projectedType));

                            var isCircularMethod = typeof(MappingContext).GetMethod(nameof(MappingContext.IsCircularReference));
                            var circularCheck = Expression.Call(
                                contextParam,
                                isCircularMethod,
                                Expression.Convert(valueVar, typeof(object)),
                                Expression.Constant(nestedTypePair)
                            );
                            var preserveProp = Expression.Property(contextParam, nameof(MappingContext.PreserveReferences));

                            var mapMethod = typeof(MappingEngine).GetMethod(nameof(MapInstanceWithContext), BindingFlags.NonPublic | BindingFlags.Instance)
                                .MakeGenericMethod(projectedType, destProperty.PropertyType);
                            var mappedValue = Expression.Call(
                                Expression.Constant(this),
                                mapMethod,
                                valueVar,
                                contextParam
                            );
                            // destination = (TDest)context.GetCachedDestination(value, typeof(TDest))
                            var getCachedMethod = typeof(MappingContext).GetMethod(nameof(MappingContext.GetCachedDestination));
                            var getCachedCall = Expression.Call(
                                contextParam,
                                getCachedMethod,
                                Expression.Convert(valueVar, typeof(object)),
                                Expression.Constant(destProperty.PropertyType)
                            );
                            var cachedCast = Expression.Convert(getCachedCall, destProperty.PropertyType);

                            // if (preserve && circular) { destination = cached; } else if (!circular) { destination = map(value); }
                            var ifCircularAssignCached = Expression.IfThen(
                                Expression.AndAlso(preserveProp, circularCheck),
                                Expression.Assign(destinationValue, cachedCast)
                            );
                            var ifNotCircularMap = Expression.IfThen(
                                Expression.AndAlso(nullCheck, Expression.Not(circularCheck)),
                                Expression.Assign(destinationValue, mappedValue)
                            );

                            var block = Expression.Block(new[] { valueVar }, assignValue, ifCircularAssignCached, ifNotCircularMap);
                            expressions.Add(block);
                            continue;
                        }

                        // Collections mapping (minimal)
                        if (IsCollectionType(projectedType) && IsCollectionType(destProperty.PropertyType))
                        {
                            var srcElem = GetCollectionElementType(projectedType);
                            var dstElem = GetCollectionElementType(destProperty.PropertyType);
                            if (srcElem != null && dstElem != null)
                            {
                                var mapListMethod = typeof(MappingEngine).GetMethod(nameof(MapCollection))
                                    .MakeGenericMethod(srcElem, dstElem);
                                var mappedCollection = Expression.Call(
                                    Expression.Constant(this),
                                    mapListMethod,
                                    projected
                                );
                                expressions.Add(Expression.Assign(destinationValue, mappedCollection));
                                continue;
                            }
                        }
                    }
                    // If custom config exists but no lambda, fall through to default mapping
                }

                // Default: by-name mapping
                if (sourceProperties.TryGetValue(destProperty.Name, out var sourceProperty))
                {
                    var sourceValue = Expression.Property(sourceParam, sourceProperty);

                    // Handle simple types
                    if (IsSimpleType(sourceProperty.PropertyType) && sourceProperty.PropertyType == destProperty.PropertyType)
                    {
                        expressions.Add(Expression.Assign(destinationValue, sourceValue));
                    }
                    // Handle nullable to non-nullable or vice versa for simple types
                    else if (IsSimpleType(GetUnderlyingType(sourceProperty.PropertyType)) &&
                             IsSimpleType(GetUnderlyingType(destProperty.PropertyType)))
                    {
                        var convertedValue = Expression.Convert(sourceValue, destProperty.PropertyType);
                        expressions.Add(Expression.Assign(destinationValue, convertedValue));
                    }
                    // Handle complex types (with circular ref guard and cached reuse when preserving references)
                    else if (IsComplexType(sourceProperty.PropertyType) && IsComplexType(destProperty.PropertyType))
                    {
                        var nestedTypePair = new TypePair(sourceProperty.PropertyType, destProperty.PropertyType);
                        var nullCheck = Expression.NotEqual(sourceValue, Expression.Constant(null, sourceProperty.PropertyType));

                        var isCircularMethod = typeof(MappingContext).GetMethod(nameof(MappingContext.IsCircularReference));
                        var circularCheck = Expression.Call(
                            contextParam,
                            isCircularMethod,
                            Expression.Convert(sourceValue, typeof(object)),
                            Expression.Constant(nestedTypePair)
                        );
                        var preserveProp = Expression.Property(contextParam, nameof(MappingContext.PreserveReferences));

                        var mapMethod = typeof(MappingEngine).GetMethod(nameof(MapInstanceWithContext), BindingFlags.NonPublic | BindingFlags.Instance)
                            .MakeGenericMethod(sourceProperty.PropertyType, destProperty.PropertyType);

                        var mappedValue = Expression.Call(
                            Expression.Constant(this),
                            mapMethod,
                            sourceValue,
                            contextParam
                        );
                        // destination = (TDest)context.GetCachedDestination(sourceValue, typeof(TDest))
                        var getCachedMethod = typeof(MappingContext).GetMethod(nameof(MappingContext.GetCachedDestination));
                        var getCachedCall = Expression.Call(
                            contextParam,
                            getCachedMethod,
                            Expression.Convert(sourceValue, typeof(object)),
                            Expression.Constant(destProperty.PropertyType)
                        );
                        var cachedCast = Expression.Convert(getCachedCall, destProperty.PropertyType);

                        var ifCircularAssignCached = Expression.IfThen(
                            Expression.AndAlso(preserveProp, circularCheck),
                            Expression.Assign(destinationValue, cachedCast)
                        );
                        var ifNotCircularMap = Expression.IfThen(
                            Expression.AndAlso(nullCheck, Expression.Not(circularCheck)),
                            Expression.Assign(destinationValue, mappedValue)
                        );

                        expressions.Add(ifCircularAssignCached);
                        expressions.Add(ifNotCircularMap);
                    }
                    // Handle collections
                    else if (IsCollectionType(sourceProperty.PropertyType) && IsCollectionType(destProperty.PropertyType))
                    {
                        var sourceElementType = GetCollectionElementType(sourceProperty.PropertyType);
                        var destinationElementType = GetCollectionElementType(destProperty.PropertyType);

                        if (sourceElementType != null && destinationElementType != null)
                        {
                            var nullCheck = Expression.NotEqual(sourceValue, Expression.Constant(null, sourceProperty.PropertyType));

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

        #region Expression helpers

        // Replace the parameter of a lambda with the given source parameter and return the inlined expression body
        private static Expression InlineSelector(LambdaExpression selector, ParameterExpression sourceParam)
        {
            var parameter = selector.Parameters[0];
            var replacer = new ParameterReplaceVisitor(parameter, sourceParam);
            return replacer.Visit(selector.Body);
        }

        // Generic helper to invoke a compiled selector in expression trees reliably across runtimes
        private static TResult InvokeSelector<TSrc, TResult>(TSrc source, Func<TSrc, TResult> selector)
            => selector != null ? selector(source) : default;

        private sealed class ParameterReplaceVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _from;
            private readonly ParameterExpression _to;
            public ParameterReplaceVisitor(ParameterExpression from, ParameterExpression to)
            {
                _from = from; _to = to;
            }
            protected override Expression VisitParameter(ParameterExpression node)
                => node == _from ? _to : base.VisitParameter(node);
        }

        #endregion

        #region Reflection-based Mapping (for non-generic overloads)

        /// <summary>
        /// Maps properties from a source object to a destination object (non-generic version).
        /// </summary>
        /// <param name="source">Source object.</param>
        /// <param name="destination">Destination object.</param>
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
        /// Maps properties from a source object to an existing destination object (generic version).
        /// Performs an in-place update and supports simple, complex, and collection members.
        /// </summary>
        /// <param name="source">Source instance.</param>
        /// <param name="destination">Destination instance to update.</param>
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