using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Diagnostics.Emitters;
using NexusMods.Abstractions.GameLocators;
using NexusMods.Abstractions.GameLocators.GameCapabilities;
using NexusMods.Abstractions.GameLocators.Stores.GOG;
using NexusMods.Abstractions.GameLocators.Stores.Steam;
using NexusMods.Abstractions.GameLocators.Stores.Xbox;
using NexusMods.Abstractions.Games;
using NexusMods.Abstractions.Games.DTO;
using NexusMods.Abstractions.IO;
using NexusMods.Abstractions.IO.StreamFactories;
using NexusMods.Abstractions.Library.Installers;
using NexusMods.Abstractions.Loadouts.Synchronizers;
using NexusMods.Paths;

namespace NexusMods.Games.Larian.BaldursGate3;

public class BaldursGate3 : AGame, ISteamGame
{
    public static readonly GameDomain StaticDomain = GameDomain.From("baldursgate3");
    private readonly IServiceProvider _serviceProvider;

    public BaldursGate3(IServiceProvider provider) : base(provider)
    {
        _serviceProvider = provider;
    }
    
    public override string Name => "Baldur's Gate 3";
    public override GameDomain Domain => StaticDomain;
    public override GamePath GetPrimaryFile(GameStore store) => new(LocationId.Game, "bin/bg3.exe");

    protected override ILoadoutSynchronizer MakeSynchronizer(IServiceProvider provider) 
        => new BaldursGate3Synchronizer(provider);

    protected override IReadOnlyDictionary<LocationId, AbsolutePath> GetLocations(IFileSystem fileSystem, GameLocatorResult installation)
    {
        var result = new Dictionary<LocationId, AbsolutePath>()
        {
            { LocationId.Game, installation.Path },
        };

        return result;
    }
    
    public IEnumerable<uint> SteamIds => new[] { 1086940u };
    
    public override IStreamFactory Icon => 
        new EmbededResourceStreamFactory<BaldursGate3>("NexusMods.Games.Larian.Resources.BaldursGate3.icon.png");
    
    public override IStreamFactory GameImage => 
        new EmbededResourceStreamFactory<BaldursGate3>("NexusMods.Games.Larian.Resources.BaldursGate3.game_image.png");
    
    public override List<IModInstallDestination> GetInstallDestinations(IReadOnlyDictionary<LocationId, AbsolutePath> locations) => ModInstallDestinationHelpers.GetCommonLocations(locations);
}
