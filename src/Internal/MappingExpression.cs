using Simple.AutoMapper.Intefaces;
using Simple.AutoMapper.Core;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Simple.AutoMapper.Internal
{
    /// <summary>
    /// Mapping expression implementation
    /// </summary>
    internal class MappingExpression<TSource, TDestination> : IMappingExpression<TSource, TDestination>
        where TDestination : new()
    {
        private readonly MappingEngine _engine;
        private readonly HashSet<string> _ignoredProperties = new();
        private readonly Dictionary<string, object> _customMappings = new();
        private int _maxDepth = 0; // 0 means unlimited
        private bool _preserveReferences = false;

        public Type SourceType => typeof(TSource);
        public Type DestinationType => typeof(TDestination);
        public int MaxDepthValue => _maxDepth;
        public bool PreserveReferencesValue => _preserveReferences;

        public MappingExpression(MappingEngine engine)
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

        public IMappingExpression<TDestination, TSource> ReverseMap()
        {
            // Check if TSource has a parameterless constructor
            var sourceType = typeof(TSource);
            if (sourceType.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new InvalidOperationException($"Cannot create reverse mapping: {sourceType.Name} does not have a parameterless constructor.");
            }
            
            // Use reflection to call CreateMap with runtime types
            var createMapMethod = typeof(MappingEngine).GetMethod(nameof(MappingEngine.CreateMap));
            var genericMethod = createMapMethod.MakeGenericMethod(typeof(TDestination), typeof(TSource));
            var reverseMapping = genericMethod.Invoke(_engine, null);
            
            return (IMappingExpression<TDestination, TSource>)reverseMapping;
        }
        
        public IMappingExpression<TSource, TDestination> MaxDepth(int depth)
        {
            _maxDepth = depth;
            return this;
        }
        
        public IMappingExpression<TSource, TDestination> PreserveReferences()
        {
            _preserveReferences = true;
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
}