using NexusMods.Common;
using NexusMods.DataModel.Abstractions;
using NexusMods.DataModel.ArchiveContents;
using NexusMods.DataModel.Extensions;
using NexusMods.DataModel.Games;
using NexusMods.DataModel.Loadouts;
using NexusMods.DataModel.Loadouts.ModFiles;
using NexusMods.DataModel.ModInstallers;
using NexusMods.Hashing.xxHash64;
using NexusMods.Paths;
using NexusMods.Paths.Extensions;

namespace NexusMods.Games.Sifu;

public class SifuModInstaller : IModInstaller
{
    private static readonly Extension PakExt = new(".pak");
    private static readonly RelativePath ModsPath = @"Content\Paks\~mods".ToRelativePath();

    private readonly IDataStore _dataStore;

    public SifuModInstaller(IDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public Priority Priority(GameInstallation installation, EntityDictionary<RelativePath, AnalyzedFile> files)
    {
        return installation.Game is Sifu && ContainsUEModFile(files)
            ? Common.Priority.Normal
            : Common.Priority.None;
    }

    private static bool ContainsUEModFile(EntityDictionary<RelativePath, AnalyzedFile> files)
    {
        return files.Any(kv => kv.Key.Extension == PakExt);
    }

    public ValueTask<IEnumerable<Mod>> GetModsAsync(
        GameInstallation gameInstallation,
        Mod baseMod,
        Hash srcArchiveHash,
        EntityDictionary<RelativePath, AnalyzedFile> archiveFiles,
        CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(GetMods(baseMod, srcArchiveHash, archiveFiles));
    }

    private IEnumerable<Mod> GetMods(
        Mod baseMod,
        Hash srcArchiveHash,
        EntityDictionary<RelativePath, AnalyzedFile> archiveFiles)
    {
        var pakPath = archiveFiles.Keys.First(filePath => filePath.FileName.Extension == PakExt).Parent;

        var modFiles = archiveFiles
            .Where(kv => kv.Key.InFolder(pakPath))
            .Select(kv =>
            {
                var (path, file) = kv;

                return new FromArchive
                {
                    Id = ModFileId.New(),
                    From = new HashRelativePath(srcArchiveHash, path),
                    To = new GamePath(GameFolderType.Game, ModsPath.Join(path.RelativeTo(pakPath))),
                    Hash = file.Hash,
                    Size = file.Size
                };
            });

        yield return baseMod with
        {
            Files = modFiles.ToEntityDictionary(_dataStore)
        };
    }
}
