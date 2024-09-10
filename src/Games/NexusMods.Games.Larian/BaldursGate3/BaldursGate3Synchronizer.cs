using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.GameLocators;
using NexusMods.Abstractions.Settings;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Abstractions.Loadouts.Synchronizers;
using NexusMods.Paths;

namespace NexusMods.Games.Larian.BaldursGate3;

public class BaldursGate3Synchronizer : ALoadoutSynchronizer
{
    private BaldursGate3Settings _settings;
    
    public BaldursGate3Synchronizer(IServiceProvider provider) : base(provider)
    {
        var SettingsManager = provider.GetRequiredService<ISettingsManager>();
        _settings = SettingsManager.Get<BaldursGate3Settings>();
    }

    private static readonly GamePath[] IgnoredBackupFolders =
    [
        new GamePath(LocationId.Game, "Data"),
        new GamePath(LocationId.Game, "DigitalDeluxe"),
        new GamePath(LocationId.Game, "DotNetCore"),
    ];

    public override bool IsIgnoredPath(GamePath path)
    {
        return IgnoredBackupFolders.Contains(path);
    }

    public override async Task<Loadout.ReadOnly> Synchronize(Loadout.ReadOnly loadout)
    {
        loadout = await base.Synchronize(loadout);
        
        return await base.Synchronize(loadout);
    }

    public override bool IsIgnoredBackupPath(GamePath path)
    {
        if (_settings.DoFullGameBackup) 
            return false;

        if (path.LocationId != LocationId.Game)
            return false;

        return IgnoredBackupFolders.Any(ignore => path.Path.InFolder(ignore.Path));
    }
}
