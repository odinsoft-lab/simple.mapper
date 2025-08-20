using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleMapper
{
    /// <summary>
    /// Simple mapping engine with pre-compiled mappings for better performance
    /// </summary>
    public class SimpleMappingEngine
    {
        private readonly ConcurrentDictionary<TypePair, IMappingExpression> _mappingExpressions = new();
        private readonly ConcurrentDictionary<TypePair, Delegate> _compiledMappings = new();
        private readonly object _compilationLock = new();

        /// <summary>
        /// Create a mapping configuration between source and destination types
        /// </summary>
        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
            where TDestination : new()
        {
            var typePair = new TypePair(typeof(TSource), typeof(TDestination));
            var expression = new MappingExpression<TSource, TDestination>(this);
            _mappingExpressions[typePair] = expression;
            return expression;
        }

        /// <summary>
        /// Map a single object using pre-compiled mapping
        /// </summary>
        public TDestination Map<TSource, TDestination>(TSource source)
            where TDestination : new()
        {
            if (source == null)
                return default(TDestination);

            var typePair = new TypePair(typeof(TSource), typeof(TDestination));
            var mapper = GetOrCompileMapper<TSource, TDestination>(typePair);
            return mapper(source);
        }

        /// <summary>
        /// Map a collection using pre-compiled mapping
        /// </summary>
        public List<TDestination> MapList<TSource, TDestination>(IEnumerable<TSource> sourceList)
            where TDestination : new()
        {
            if (sourceList == null)
                return null;

            var mapper = GetOrCompileMapper<TSource, TDestination>(new TypePair(typeof(TSource), typeof(TDestination)));
            return sourceList.Select(mapper).ToList();
        }

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
                    // Handle custom mapping (not implemented in this simple version)
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
                        
                        var mapMethod = typeof(SimpleMappingEngine).GetMethod(nameof(Map))
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
                            var mapListMethod = typeof(SimpleMappingEngine).GetMethod(nameof(MapList))
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

        /// <summary>
        /// Type pair for caching mappings
        /// </summary>
        private struct TypePair : IEquatable<TypePair>
        {
            public Type SourceType { get; }
            public Type DestinationType { get; }

            public TypePair(Type sourceType, Type destinationType)
            {
                SourceType = sourceType;
                DestinationType = destinationType;
            }

            public bool Equals(TypePair other)
            {
                return SourceType == other.SourceType && DestinationType == other.DestinationType;
            }

            public override bool Equals(object obj)
            {
                return obj is TypePair other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((SourceType?.GetHashCode() ?? 0) * 397) ^ (DestinationType?.GetHashCode() ?? 0);
                }
            }
        }
    }

    /// <summary>
    /// Mapping expression interface
    /// </summary>
    public interface IMappingExpression
    {
        Type SourceType { get; }
        Type DestinationType { get; }
    }

    /// <summary>
    /// Generic mapping expression interface
    /// </summary>
    public interface IMappingExpression<TSource, TDestination> : IMappingExpression
    {
        IMappingExpression<TSource, TDestination> Ignore(Expression<Func<TDestination, object>> destinationMember);
        IMappingExpression<TSource, TDestination> ForMember<TMember>(
            Expression<Func<TDestination, TMember>> destinationMember,
            Action<IMemberConfigurationExpression<TSource, TDestination, TMember>> memberOptions);
    }

    /// <summary>
    /// Member configuration expression interface
    /// </summary>
    public interface IMemberConfigurationExpression<TSource, TDestination, TMember>
    {
        void MapFrom(Expression<Func<TSource, TMember>> sourceMember);
        void Ignore();
    }

    /// <summary>
    /// Mapping expression implementation
    /// </summary>
    internal class MappingExpression<TSource, TDestination> : IMappingExpression<TSource, TDestination>
        where TDestination : new()
    {
        private readonly SimpleMappingEngine _engine;
        private readonly HashSet<string> _ignoredProperties = new();
        private readonly Dictionary<string, object> _customMappings = new();

        public Type SourceType => typeof(TSource);
        public Type DestinationType => typeof(TDestination);

        public MappingExpression(SimpleMappingEngine engine)
        {
            _engine = engine;
        }

        public IMappingExpression<TSource, TDestination> Ignore(Expression<Func<TDestination, object>> destinationMember)
        {
            var memberName = GetMemberName(destinationMember);
            _ignoredProperties.Add(memberName);
            return this;
        }

        public IMappingExpression<TSource, TDestination> ForMember<TMember>(
            Expression<Func<TDestination, TMember>> destinationMember,
            Action<IMemberConfigurationExpression<TSource, TDestination, TMember>> memberOptions)
        {
            var memberName = GetMemberName(destinationMember);
            var config = new MemberConfigurationExpression<TSource, TDestination, TMember>();
            memberOptions(config);
            _customMappings[memberName] = config;
            return this;
        }

        public bool IsPropertyIgnored(string propertyName)
        {
            return _ignoredProperties.Contains(propertyName);
        }

        public object GetCustomMapping(string propertyName)
        {
            return _customMappings.TryGetValue(propertyName, out var mapping) ? mapping : null;
        }

        private string GetMemberName<T>(Expression<Func<TDestination, T>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
                return memberExpression.Member.Name;
            
            if (expression.Body is UnaryExpression unaryExpression && 
                unaryExpression.Operand is MemberExpression unaryMemberExpression)
                return unaryMemberExpression.Member.Name;
            
            throw new ArgumentException("Expression must be a member expression");
        }
    }

    /// <summary>
    /// Member configuration expression implementation
    /// </summary>
    internal class MemberConfigurationExpression<TSource, TDestination, TMember> 
        : IMemberConfigurationExpression<TSource, TDestination, TMember>
    {
        public Expression<Func<TSource, TMember>> SourceExpression { get; private set; }
        public bool IsIgnored { get; private set; }

        public void MapFrom(Expression<Func<TSource, TMember>> sourceMember)
        {
            SourceExpression = sourceMember;
        }

        public void Ignore()
        {
            IsIgnored = true;
        }
    }
}