using BenchmarkDotNet.Attributes;
using NexusMods.Abstractions.DiskState;
using NexusMods.Abstractions.Loadouts.Files;
using NexusMods.Abstractions.Loadouts.Mods;
using NexusMods.Benchmarks.Benchmarks.Loadouts.Harness;
using NexusMods.Benchmarks.Interfaces;

namespace NexusMods.Benchmarks.Benchmarks.Loadouts;

[MemoryDiagnoser]
[BenchmarkInfo("Synchronizer", "Tests how quickly the synchronizer works, sans disk access")]
public class SynchronizerBenchmarks : ASynchronizerBenchmark<SynchronizerBenchmarks>, IBenchmark
{
    private DiskStateTree _diskState;

    [GlobalSetup]
    public async Task Setup()
    {
        await InitializeAsync();
        await AddMods(1000, 1000);

        var files = BaseLoadout.Mods.Where(m => m.Category == ModCategory.Mod)
            .SelectMany(m => m.Files.OfTypeStoredFile())
            .Select(f => KeyValuePair.Create(f.AsFile().To, new DiskStateEntry
                {
                    Hash = f.Hash,
                    Size = f.Size,
                    LastModified = DateTime.UtcNow,
                }
            ));
        _diskState = DiskStateTree.Create(files);
    }

    [Benchmark]
    public int TestBuildSyncTree()
    {
        var syncTree = Synchronizer.BuildSyncTree(_diskState, _diskState, BaseLoadout);
        return syncTree.GetHashCode();
    }
}
