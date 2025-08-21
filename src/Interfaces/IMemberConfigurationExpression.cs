using System;
using System.Linq.Expressions;

namespace Simple.AutoMapper.Interfaces
{
    /// <summary>
    /// Member configuration expression interface
    /// </summary>
    public interface IMemberConfigurationExpression<TSource, TDestination, TMember>
    {
        void MapFrom(Expression<Func<TSource, TMember>> sourceMember);
        void Ignore();
    }
}