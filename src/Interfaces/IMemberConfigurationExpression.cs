using System;
using System.Linq.Expressions;

namespace Simple.AutoMapper.Interfaces
{
    /// <summary>
    /// Member configuration expression interface
    /// </summary>
    public interface IMemberConfigurationExpression<TSource, TDestination, TMember>
    {
        /// <summary>
        /// Specifies a mapping for the destination member from the provided source member expression.
        /// </summary>
        /// <param name="sourceMember">Expression selecting the source member.</param>
        void MapFrom(Expression<Func<TSource, TMember>> sourceMember);

        /// <summary>
        /// Instructs the mapper to ignore the destination member.
        /// </summary>
        void Ignore();
    }
}