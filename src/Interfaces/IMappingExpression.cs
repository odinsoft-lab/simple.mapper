using System;

namespace Simple.AutoMapper.Interfaces
{
    /// <summary>
    /// Mapping expression interface
    /// </summary>
    public interface IMappingExpression
    {
        Type SourceType { get; }
        Type DestinationType { get; }
        
        /// <summary>
        /// Check if a property is configured to be ignored
        /// </summary>
        bool IsPropertyIgnored(string propertyName);
        
        /// <summary>
        /// Get custom mapping configuration for a property
        /// </summary>
        object GetCustomMapping(string propertyName);
        
        /// <summary>
        /// Maximum depth for recursive mapping (0 = unlimited)
        /// </summary>
        int MaxDepthValue { get; }
        
        /// <summary>
        /// Whether to preserve object references for circular reference handling
        /// </summary>
        bool PreserveReferencesValue { get; }
    }
}