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

        /// <summary>
        /// Specifies an action to execute before mapping occurs.
        /// </summary>
        /// <param name="beforeFunction">The action to execute before mapping. Takes source and destination as parameters.</param>
        /// <returns>The current mapping expression for chaining.</returns>
        IMappingExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination> beforeFunction);

        /// <summary>
        /// Specifies an action to execute after mapping occurs.
        /// </summary>
        /// <param name="afterFunction">The action to execute after mapping. Takes source and destination as parameters.</param>
        /// <returns>The current mapping expression for chaining.</returns>
        IMappingExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination> afterFunction);

        /// <summary>
        /// Specifies a custom constructor function to create the destination object.
        /// Use this for types without a parameterless constructor or when custom initialization is needed.
        /// </summary>
        /// <param name="constructor">A function that takes the source object and returns a new destination instance.</param>
        /// <returns>The current mapping expression for chaining.</returns>
        IMappingExpression<TSource, TDestination> ConstructUsing(Func<TSource, TDestination> constructor);
    }
}