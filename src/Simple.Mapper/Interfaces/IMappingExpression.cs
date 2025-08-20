using System;

namespace SimpleMapper
{
    /// <summary>
    /// Mapping expression interface
    /// </summary>
    public interface IMappingExpression
    {
        Type SourceType { get; }
        Type DestinationType { get; }
    }
}