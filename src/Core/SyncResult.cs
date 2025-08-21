using System;

namespace Simple.AutoMapper.Core
{
    /// <summary>
    /// Result summary for list synchronization
    /// </summary>
    public readonly struct SyncResult
    {
        public int Added { get; }
        public int Updated { get; }
        public int Removed { get; }

        public SyncResult(int added, int updated, int removed)
        {
            Added = added;
            Updated = updated;
            Removed = removed;
        }

        public override string ToString() => $"Added={Added}, Updated={Updated}, Removed={Removed}";
    }
}