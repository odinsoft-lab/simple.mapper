using System;
using System.Linq.Expressions;

namespace Simple.AutoMapper.Intefaces
{
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
}