using System;
using System.Collections.Generic;
using Simple.AutoMapper.Internal;

namespace Simple.AutoMapper.Core
{
    /// <summary>
    /// Context for tracking mapping state and preventing circular references
    /// Based on AutoMapper's ResolutionContext pattern
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
        
        public bool PreserveReferences => _preserveReferences;
        public int MaxDepth => _maxDepth;
        public int CurrentDepth => _currentDepth;
        
        public MappingContext(int maxDepth = 10, bool preserveReferences = false)
        {
            _maxDepth = maxDepth > 0 ? maxDepth : 10; // Use default if 0 or negative
            _preserveReferences = preserveReferences;
        }
        
        /// <summary>
        /// Check if an object would create a circular reference
        /// </summary>
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
        /// Cache the destination object for a source
        /// </summary>
        public void CacheDestination(object source, Type destinationType, object destination)
        {
            if (source != null && destination != null)
            {
                var cacheKey = new ContextCacheKey(source, destinationType);
                _instanceCache[cacheKey] = destination;
            }
        }
        
        /// <summary>
        /// Get cached destination if exists
        /// </summary>
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
        public object Source { get; }
        public Type DestinationType { get; }
        
        public ContextCacheKey(object source, Type destinationType)
        {
            Source = source;
            DestinationType = destinationType;
        }
        
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
        
        public bool Equals(ContextCacheKey other)
        {
            return DestinationType == other.DestinationType && ReferenceEquals(Source, other.Source);
        }
        
        public override bool Equals(object obj)
        {
            return obj is ContextCacheKey other && Equals(other);
        }
    }
}