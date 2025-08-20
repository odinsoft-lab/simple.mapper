using Simple.AutoMapper.Intefaces;
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