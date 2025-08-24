using System;
using System.Collections.Generic;
using Simple.AutoMapper.Internal;

namespace Simple.AutoMapper.Core
{
    /// <summary>
    /// Context for tracking mapping state during execution and mitigating circular references.
    /// Holds per-run caches and depth counters used by compiled mappers.
    /// </summary>
    public class MappingContext
    {
        // Cache for tracking object instances to prevent circular references
        private readonly Dictionary<ContextCacheKey, object> _instanceCache = new();

        // Track depth for each type pair to enforce MaxDepth
        private readonly Dictionary<TypePair, int> _typeDepth = new();

        // Current recursion depth
        private int _currentDepth = 0;

        // Configuration values
        private readonly int _maxDepth;
        private readonly bool _preserveReferences;

        /// <summary>
        /// Gets a value indicating whether object reference identity should be preserved during mapping.
        /// When enabled, previously mapped source instances are cached and reused to mitigate circular references.
        /// </summary>
        public bool PreserveReferences => _preserveReferences;

        /// <summary>
        /// Gets the soft recursion depth limit used by mapping operations.
        /// This value can be configured via mapping configuration and is used to prevent excessively deep graphs.
        /// </summary>
        public int MaxDepth => _maxDepth;

        /// <summary>
        /// Gets the current recursion depth for the ongoing mapping operation.
        /// Intended for internal diagnostics and guard checks.
        /// </summary>
        public int CurrentDepth => _currentDepth;

        /// <summary>
        /// Creates a new mapping context.
        /// </summary>
        /// <param name="maxDepth">Soft limit for recursion (implementation-specific).</param>
        /// <param name="preserveReferences">Whether to track and preserve object identity.</param>
        public MappingContext(int maxDepth = 10, bool preserveReferences = false)
        {
            _maxDepth = maxDepth > 0 ? maxDepth : 10; // Use default if 0 or negative
            _preserveReferences = preserveReferences;
        }

        /// <summary>
        /// Determines whether mapping <paramref name="source"/> into <paramref name="typePair"/> would introduce a circular reference.
        /// </summary>
        /// <summary>
        /// Determines whether mapping the given <paramref name="source"/> object for the specified <paramref name="typePair"/>
        /// would introduce a circular reference within the current mapping session.
        /// </summary>
        /// <param name="source">The source object being mapped.</param>
        /// <param name="typePair">The source and destination type pair for the mapping.</param>
        /// <returns><c>true</c> if a circular reference is detected or depth limits are exceeded; otherwise <c>false</c>.</returns>
        public bool IsCircularReference(object source, TypePair typePair)
        {
            if (source == null)
                return false;

            // Always check the cache to detect circular references
            var cacheKey = new ContextCacheKey(source, typePair.DestinationType);
            if (_instanceCache.ContainsKey(cacheKey))
            {
                return true; // Already processing or processed this instance - circular reference detected
            }

            // Check current recursion depth
            // Note: This is a simplified implementation. Proper depth tracking
            // would require incrementing on enter and decrementing on exit.
            // For now, we only use this for preventing extremely deep recursion.
            if (_currentDepth >= _maxDepth * 10) // Use a higher threshold
            {
                return true; // Max depth reached
            }

            // Register this mapping to track circular references
            _instanceCache[cacheKey] = null; // Placeholder until actual mapping completes
            _currentDepth++; // Increment depth when entering a nested mapping

            return false;
        }

        /// <summary>
        /// Caches the destination object associated with a source instance.
        /// </summary>
        /// <summary>
        /// Caches the <paramref name="destination"/> instance associated with the given <paramref name="source"/> object
        /// for the specified <paramref name="destinationType"/> so that subsequent mappings can reuse it.
        /// </summary>
        /// <param name="source">The source object instance.</param>
        /// <param name="destinationType">The destination CLR type to associate with the cached instance.</param>
        /// <param name="destination">The destination instance to cache.</param>
        public void CacheDestination(object source, Type destinationType, object destination)
        {
            if (source != null && destination != null)
            {
                var cacheKey = new ContextCacheKey(source, destinationType);
                _instanceCache[cacheKey] = destination;
            }
        }

        /// <summary>
        /// Gets a previously cached destination for the given source and destination type.
        /// </summary>
        /// <summary>
        /// Retrieves a previously cached destination instance for the specified <paramref name="source"/> and <paramref name="destinationType"/>.
        /// </summary>
        /// <param name="source">The source object instance to look up.</param>
        /// <param name="destinationType">The destination CLR type associated with the cache entry.</param>
        /// <returns>The cached destination instance if present; otherwise <c>null</c>.</returns>
        public object GetCachedDestination(object source, Type destinationType)
        {
            if (source == null)
                return null;

            var cacheKey = new ContextCacheKey(source, destinationType);
            _instanceCache.TryGetValue(cacheKey, out var destination);
            return destination;
        }

        /// <summary>
        /// Increment the depth counter for a type pair
        /// </summary>
        private void IncrementTypeDepth(TypePair typePair)
        {
            if (_typeDepth.ContainsKey(typePair))
            {
                _typeDepth[typePair]++;
            }
            else
            {
                _typeDepth[typePair] = 1;
            }
        }

        /// <summary>
        /// Decrement the depth counter for a type pair
        /// </summary>
        /// <summary>
        /// Decrements the recursion depth counter for the specified <paramref name="typePair"/> and removes it when it reaches zero.
        /// </summary>
        /// <param name="typePair">The source/destination type pair key.</param>
        public void DecrementTypeDepth(TypePair typePair)
        {
            if (_typeDepth.ContainsKey(typePair))
            {
                _typeDepth[typePair]--;
                if (_typeDepth[typePair] <= 0)
                {
                    _typeDepth.Remove(typePair);
                }
            }
        }

        /// <summary>
        /// Decrement the current recursion depth
        /// </summary>
        /// <summary>
        /// Decrements the global recursion depth counter for the current mapping operation.
        /// </summary>
        public void DecrementDepth()
        {
            if (_currentDepth > 0)
                _currentDepth--;
        }
    }

    /// <summary>
    /// Key for caching mapped instances to prevent circular references
    /// </summary>
    public readonly struct ContextCacheKey : IEquatable<ContextCacheKey>
    {
        /// <summary>
        /// Gets the source object instance serving as the cache key.
        /// </summary>
        public object Source { get; }

        /// <summary>
        /// Gets the destination CLR type associated with the cached mapping.
        /// </summary>
        public Type DestinationType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextCacheKey"/> struct.
        /// </summary>
        /// <param name="source">The source object instance.</param>
        /// <param name="destinationType">The destination CLR type.</param>
        public ContextCacheKey(object source, Type destinationType)
        {
            Source = source;
            DestinationType = destinationType;
        }

        /// <summary>
        /// Returns a hash code for this instance suitable for use in hash-based collections.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 31 + (DestinationType?.GetHashCode() ?? 0);
                hash = hash * 31 + System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(Source);
                return hash;
            }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another <see cref="ContextCacheKey"/>.
        /// </summary>
        /// <param name="other">Another cache key to compare with.</param>
        /// <returns><c>true</c> if both refer to the same destination type and the same source instance; otherwise <c>false</c>.</returns>
        public bool Equals(ContextCacheKey other)
        {
            return DestinationType == other.DestinationType && ReferenceEquals(Source, other.Source);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the objects are equal; otherwise <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is ContextCacheKey other && Equals(other);
        }
    }
}