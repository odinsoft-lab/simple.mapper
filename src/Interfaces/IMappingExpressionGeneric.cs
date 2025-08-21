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
        
        /// <summary>
        /// Creates a reverse mapping configuration from TDestination to TSource
        /// </summary>
        IMappingExpression<TDestination, TSource> ReverseMap();
        
        /// <summary>
        /// Set the maximum depth for recursive mapping to prevent stack overflow
        /// </summary>
        IMappingExpression<TSource, TDestination> MaxDepth(int depth);
        
        /// <summary>
        /// Preserve object references to handle circular references
        /// </summary>
        IMappingExpression<TSource, TDestination> PreserveReferences();
    }
}