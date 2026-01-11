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

        /// <summary>
        /// Specifies a condition that must be true for the mapping to occur.
        /// If the condition returns false, the destination member retains its default value.
        /// </summary>
        /// <param name="condition">A predicate that takes the source object and returns true if mapping should occur.</param>
        void Condition(Func<TSource, bool> condition);

        /// <summary>
        /// Specifies a value to use when the source value is null.
        /// </summary>
        /// <param name="nullSubstitute">The value to use when the source is null.</param>
        void NullSubstitute(TMember nullSubstitute);
    }
}