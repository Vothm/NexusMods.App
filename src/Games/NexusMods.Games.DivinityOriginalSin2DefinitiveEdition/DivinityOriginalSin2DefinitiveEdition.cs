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

namespace NexusMods.Games.DivinityOriginalSin2DefinitiveEdition;

public class DivinityOriginalSin2DefinitiveEdition : AGame, ISteamGame
{
    public static readonly GameDomain StaticDomain = GameDomain.From("divinityoriginalsin2definitiveedition");
    private readonly IServiceProvider _serviceProvider;

    public DivinityOriginalSin2DefinitiveEdition(IServiceProvider provider) : base(provider)
    {
        _serviceProvider = provider;
    }
    
    public override string Name => "Divinity Original Sin 2 Definitive Edition";
    public override GameDomain Domain => StaticDomain;
    public override GamePath GetPrimaryFile(GameStore store) => new(LocationId.Game, "DefEd/bin/EoCApp.exe");

    protected override IReadOnlyDictionary<LocationId, AbsolutePath> GetLocations(IFileSystem fileSystem, GameLocatorResult installation)
    {
        var result = new Dictionary<LocationId, AbsolutePath>()
        {
            { LocationId.Game, installation.Path },
        };

        return result;
    }
    
    public IEnumerable<uint> SteamIds => new[] { 435150u };
    
    public override IStreamFactory Icon => 
        new EmbededResourceStreamFactory<DivinityOriginalSin2DefinitiveEdition>("NexusMods.Games.DivinityOriginalSin2DefinitiveEdition.Resources.icon.png");
    
    public override IStreamFactory GameImage => 
        new EmbededResourceStreamFactory<DivinityOriginalSin2DefinitiveEdition>("NexusMods.Games.DivinityOriginalSin2DefinitiveEdition.Resources.game_image .png");
    
    public override List<IModInstallDestination> GetInstallDestinations(IReadOnlyDictionary<LocationId, AbsolutePath> locations) => ModInstallDestinationHelpers.GetCommonLocations(locations);
    
}
