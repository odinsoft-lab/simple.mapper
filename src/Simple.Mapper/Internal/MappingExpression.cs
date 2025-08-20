using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SimpleMapper.Internal
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

        public Type SourceType => typeof(TSource);
        public Type DestinationType => typeof(TDestination);

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