using System;

namespace Simple.AutoMapper.Interfaces
{
    /// <summary>
    /// Mapping expression interface
    /// </summary>
    public interface IMappingExpression
    {
        /// <summary>
        /// Gets the CLR type configured as the source of the mapping.
        /// </summary>
        Type SourceType { get; }

        /// <summary>
        /// Gets the CLR type configured as the destination of the mapping.
        /// </summary>
        Type DestinationType { get; }

        /// <summary>
        /// Check if a property is configured to be ignored
        /// </summary>
        /// <summary>
        /// Determines whether the specified destination property is configured to be ignored.
        /// </summary>
        /// <param name="propertyName">Destination property name.</param>
        /// <returns><c>true</c> if the property is ignored; otherwise <c>false</c>.</returns>
        bool IsPropertyIgnored(string propertyName);

        /// <summary>
        /// Get custom mapping configuration for a property
        /// </summary>
        /// <summary>
        /// Gets custom mapping metadata for the specified destination property, if any.
        /// </summary>
        /// <param name="propertyName">Destination property name.</param>
        /// <returns>Custom mapping descriptor or <c>null</c> if none configured.</returns>
        object GetCustomMapping(string propertyName);

        /// <summary>
        /// Maximum depth for recursive mapping (0 = unlimited)
        /// </summary>
        /// <summary>
        /// Gets the maximum recursion depth for this map (0 means unlimited).
        /// </summary>
        int MaxDepthValue { get; }

        /// <summary>
        /// Whether to preserve object references for circular reference handling
        /// </summary>
        /// <summary>
        /// Gets a value indicating whether object references should be preserved to handle circular references.
        /// </summary>
        bool PreserveReferencesValue { get; }
    }
}