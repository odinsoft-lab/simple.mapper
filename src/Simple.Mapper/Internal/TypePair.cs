using System;

namespace SimpleMapper.Internal
{
    /// <summary>
    /// Type pair for caching mappings
    /// </summary>
    internal struct TypePair : IEquatable<TypePair>
    {
        public Type SourceType { get; }
        public Type DestinationType { get; }

        public TypePair(Type sourceType, Type destinationType)
        {
            SourceType = sourceType;
            DestinationType = destinationType;
        }

        public bool Equals(TypePair other)
        {
            return SourceType == other.SourceType && DestinationType == other.DestinationType;
        }

        public override bool Equals(object obj)
        {
            return obj is TypePair other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((SourceType?.GetHashCode() ?? 0) * 397) ^ (DestinationType?.GetHashCode() ?? 0);
            }
        }
    }
}