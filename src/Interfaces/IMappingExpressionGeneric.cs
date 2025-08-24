using System;
using System.Linq.Expressions;

namespace Simple.AutoMapper.Interfaces
{
    /// <summary>
    /// Generic mapping expression interface
    /// </summary>
    public interface IMappingExpression<TSource, TDestination> : IMappingExpression
    {
        /// <summary>
        /// Marks the specified destination member to be ignored during mapping.
        /// </summary>
        /// <param name="destinationMember">Expression selecting the destination member.</param>
        /// <returns>The current mapping expression for chaining.</returns>
        IMappingExpression<TSource, TDestination> Ignore(Expression<Func<TDestination, object>> destinationMember);

        /// <summary>
        /// Configures a destination member with custom mapping options.
        /// </summary>
        /// <typeparam name="TMember">The type of the destination member.</typeparam>
        /// <param name="destinationMember">Expression selecting the destination member.</param>
        /// <param name="memberOptions">Callback to configure mapping options.</param>
        /// <returns>The current mapping expression for chaining.</returns>
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
        /// <summary>
        /// Sets the maximum recursion depth to avoid stack overflow with deep graphs.
        /// </summary>
        /// <param name="depth">Maximum depth (0 means unlimited).</param>
        /// <returns>The current mapping expression for chaining.</returns>
        IMappingExpression<TSource, TDestination> MaxDepth(int depth);

        /// <summary>
        /// Preserve object references to handle circular references
        /// </summary>
        /// <summary>
        /// Enables preservation of object references to handle circular references.
        /// </summary>
        /// <returns>The current mapping expression for chaining.</returns>
        IMappingExpression<TSource, TDestination> PreserveReferences();
    }
}