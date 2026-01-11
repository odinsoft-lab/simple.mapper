using Simple.AutoMapper.Interfaces;
using System;
using System.Linq.Expressions;

namespace Simple.AutoMapper.Internal
{
    /// <summary>
    /// Member configuration expression implementation
    /// </summary>
    internal class MemberConfigurationExpression<TSource, TDestination, TMember>
        : IMemberConfigurationExpression<TSource, TDestination, TMember>
    {
        public Expression<Func<TSource, TMember>> SourceExpression { get; private set; }
        public bool IsIgnored { get; private set; }
        public Func<TSource, bool> ConditionFunc { get; private set; }
        public TMember NullSubstituteValue { get; private set; }
        public bool HasNullSubstitute { get; private set; }

        public void MapFrom(Expression<Func<TSource, TMember>> sourceMember)
        {
            SourceExpression = sourceMember;
        }

        public void Ignore()
        {
            IsIgnored = true;
        }

        public void Condition(Func<TSource, bool> condition)
        {
            ConditionFunc = condition;
        }

        public void NullSubstitute(TMember nullSubstitute)
        {
            NullSubstituteValue = nullSubstitute;
            HasNullSubstitute = true;
        }
    }
}