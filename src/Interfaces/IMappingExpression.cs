using System;

namespace OdinMapper.Intefaces
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