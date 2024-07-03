using NexusMods.Abstractions.FileStore;
using NexusMods.Abstractions.Games;
using NexusMods.Abstractions.GameLocators;
using NexusMods.Abstractions.Installers;
using NexusMods.Abstractions.IO;
using NexusMods.Abstractions.Loadouts;
using NexusMods.DataModel;
using NexusMods.DataModel.Loadouts;
using NexusMods.DataModel.Tests.Harness;
using NexusMods.Paths;

namespace NexusMods.Benchmarks.Benchmarks.Loadouts.Harness;

/// <summary>
///     Provides a datamodel library setup.
/// </summary>
public class ABenchmarkDatamodel() : ADataModelTest<ABenchmarkDatamodel>(null!)
{
    public new TemporaryFileManager TemporaryFileManager => base.TemporaryFileManager;
    public new IFileStore FileStore => base.FileStore;
    public new IArchiveInstaller ArchiveInstaller => base.ArchiveInstaller;
    public new FileHashCache FileHashCache => base.FileHashCache;
    public new IFileSystem FileSystem => base.FileSystem;
    public new IFileOriginRegistry FileOriginRegistry => base.FileOriginRegistry;
    public new DiskStateRegistry DiskStateRegistry => base.DiskStateRegistry;
    public new IToolManager ToolManager => base.ToolManager;

    public new IGame Game => base.Game;
    public new GameInstallation Install => base.Install;
    public new Loadout.ReadOnly BaseLoadout => base.BaseLoadout;
}
