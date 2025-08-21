using System;

namespace Simple.AutoMapper.Core
{
    /// <summary>
    /// Result summary for list synchronization.
    /// Tracks added, updated, and removed element counts.
    /// </summary>
    public readonly struct SyncResult
    {
        /// <summary>Number of elements added.</summary>
        public int Added { get; }
        /// <summary>Number of elements updated.</summary>
        public int Updated { get; }
        /// <summary>Number of elements removed.</summary>
        public int Removed { get; }

        /// <summary>
        /// Initializes a new <see cref="SyncResult"/> with the specified counts.
        /// </summary>
        /// <param name="added">Added count.</param>
        /// <param name="updated">Updated count.</param>
        /// <param name="removed">Removed count.</param>
        public SyncResult(int added, int updated, int removed)
        {
            Added = added;
            Updated = updated;
            Removed = removed;
        }

        /// <inheritdoc />
        public override string ToString() => $"Added={Added}, Updated={Updated}, Removed={Removed}";
    }
}