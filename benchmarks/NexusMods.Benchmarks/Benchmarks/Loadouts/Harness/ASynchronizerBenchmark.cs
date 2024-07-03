using System.Text;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.DiskState;
using NexusMods.Abstractions.GameLocators;
using NexusMods.Abstractions.Games.Loadouts;
using NexusMods.Abstractions.IO;
using NexusMods.Abstractions.IO.StreamFactories;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Abstractions.Loadouts.Files;
using NexusMods.Abstractions.Loadouts.Mods;
using NexusMods.DataModel.Tests.Harness;
using NexusMods.Hashing.xxHash64;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.Paths;
using NexusMods.StandardGameLocators;
using File = NexusMods.Abstractions.Loadouts.Files.File;
using Services = NexusMods.StandardGameLocators.Services;

namespace NexusMods.Benchmarks.Benchmarks.Loadouts.Harness;

/// <summary>
///     Helper for running synchronizer related benchmarks.
///     Gives us a fake synchronizer with specified mods/files.
/// </summary>
public class ASynchronizerBenchmark<TParent>() : ADataModelTest<TParent>(null)
{
    public async Task<Loadout.ReadOnly> AddMods(int mods, int files)
    {

        using var tx = Connection.BeginTransaction();
        
        var filesToBackup = new List<ArchivedFileEntry>();
        
        var toDelete = new List<(GamePath, Size)>();

        for (var modIdx = 0; modIdx < mods; modIdx += 1)
        {
            var mod = new Mod.New(tx)
            {
                LoadoutId = BaseLoadout.LoadoutId,
                Name = $"Mod {modIdx}",
                Version = "1.0.0",
                Category = ModCategory.Mod,
                Enabled = true,
                Revision = 0,
                Status = ModStatus.Installed,
            };
            
            for (var fileIdx = 0; fileIdx < files; fileIdx += 1)
            {
                var data = $"file_contents_{modIdx}_{fileIdx}";
                var bytes = Encoding.UTF8.GetBytes(data);
                var dataHash = data.XxHash64AsUtf8();
                var dataSize = Size.From((ulong)data.Length);

                var path = new GamePath(LocationId.Game, $"mods/mod_{modIdx}/{fileIdx}.txt");
                filesToBackup.Add(new ArchivedFileEntry(new MemoryStreamFactory(path.Path, new MemoryStream(bytes), true), dataHash, dataSize));

                var storedFile = new StoredFile.New(tx)
                {
                    File = new File.New(tx)
                    {
                        To = path,
                        LoadoutId = BaseLoadout,
                        ModId = mod,
                    },
                    Hash = dataHash,
                    Size = dataSize,
                };

                if (fileIdx == 1)
                {
                    toDelete.Add((path, dataSize));
                }

            }
        }

        var overridesMod = Synchronizer.GetOrCreateOverridesMod(BaseLoadout, tx);
        foreach (var (path, size) in toDelete)
        {
            var delete = new DeletedFile.New(tx)
            {
                File = new File.New(tx)
                {
                    To = path,
                    LoadoutId = BaseLoadout,
                    ModId = overridesMod,
                },
                Size = size,
            };
        }

        if (!overridesMod.Value.InPartition(PartitionId.Temp))
        {
            var mod = Mod.Load(Connection.Db, overridesMod);
            mod.Revise(tx);
        }

        await tx.Commit();
        Refresh(ref BaseLoadout);

        await FileStore.BackupFiles(filesToBackup, CancellationToken.None);

        return BaseLoadout;
    }
}
