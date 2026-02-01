using Simple.AutoMapper.Examples;

Console.WriteLine("╔══════════════════════════════════════════════════╗");
Console.WriteLine("║    Simple.AutoMapper — Complete Feature Demo     ║");
Console.WriteLine("╚══════════════════════════════════════════════════╝\n");

// ── Map Overloads (4) ─────────────────────────────────────
MapExamples.BasicMapExample();
MapExamples.TypeInferredMapExample();
MapExamples.CollectionMapExample();
MapExamples.InPlaceMapExample();

// ── Patch Overloads (4) — null-skip semantics ─────────────
PatchExamples.PatchNewObjectExample();
PatchExamples.PatchTypeInferredExample();
PatchExamples.PatchCollectionExample();
PatchExamples.PatchInPlaceExample();

// ── Configuration Options ─────────────────────────────────
ConfigurationExamples.IgnoreExample();
ConfigurationExamples.ForMemberMapFromExample();
ConfigurationExamples.ConditionExample();
ConfigurationExamples.NullSubstituteExample();
ConfigurationExamples.BeforeAfterMapExample();
ConfigurationExamples.ConstructUsingExample();
ConfigurationExamples.ReverseMapExample();
ConfigurationExamples.PreserveReferencesExample();
ConfigurationExamples.ProfileExample();

// ── Comparison & Performance ──────────────────────────────
ComparisonExamples.MapVsPatchComparison();
ComparisonExamples.PerformanceExample();

Console.WriteLine("Done. All examples executed successfully.");
