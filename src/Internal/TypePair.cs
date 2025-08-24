using System;

namespace Simple.AutoMapper.Internal
{
    /// <summary>
    /// Type pair for caching mappings
    /// </summary>
    public struct TypePair : IEquatable<TypePair>
    {
        /// <summary>
        /// Gets the source CLR type.
        /// </summary>
        public Type SourceType { get; }

        /// <summary>
        /// Gets the destination CLR type.
        /// </summary>
        public Type DestinationType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypePair"/> struct.
        /// </summary>
        /// <param name="sourceType">The source type.</param>
        /// <param name="destinationType">The destination type.</param>
        public TypePair(Type sourceType, Type destinationType)
        {
            SourceType = sourceType;
            DestinationType = destinationType;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another <see cref="TypePair"/>.
        /// </summary>
        public bool Equals(TypePair other)
        {
            return SourceType == other.SourceType && DestinationType == other.DestinationType;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is TypePair other && Equals(other);
        }

        /// <summary>
        /// Returns a hash code for this instance suitable for use in hash-based collections.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((SourceType?.GetHashCode() ?? 0) * 397) ^ (DestinationType?.GetHashCode() ?? 0);
            }
        }
    }
}