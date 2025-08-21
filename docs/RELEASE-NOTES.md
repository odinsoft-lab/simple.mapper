# Simple.AutoMapper v1.0.5 — Release Notes

Date: 2025-08-21

## Summary
A lean, fast object mapper for .NET with a simple public API (reflection-based) and an internal compiled engine. This release focuses on stability, multi-targeting, and clear documentation.

## Highlights
- Core mapper stabilized and simplified
- Targets: netstandard2.0, netstandard2.1, net8.0, net9.0
- Collection mapping and list synchronization helpers (SyncResult)
- Thread-safe caching, deterministic builds, embedded PDB + symbol packages (snupkg)
- Documentation refreshed (README, ROADMAP)

## Breaking/Behavioral Notes
- Circular reference handling is not supported in this release (may cause stack overflow on cyclic graphs)
- ForMember configuration is captured but not yet emitted in compiled mapping

## API Surface
- Mapper: `Map<TSource, TDestination>(TSource)`, `Map<TDestination>(object)`, Map for collections, and in-place `Map(source, destination)`
- Sync helpers return SyncResult { Added, Updated, Removed }

## Known Limitations
- No cycle detection/preserve references in this version
- Destination types need parameterless constructors
- Not for use inside IQueryable

## Migration and Usage
- Use Mapper for most scenarios; MappingEngine exists for future configuration needs
- See samples/ and README for patterns (single, collection, in-place update, EF materialization)

## Credits
Built by ODINSOFT. See README for team.
