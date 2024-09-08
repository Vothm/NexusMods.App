using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Games;
using NexusMods.Abstractions.Settings;
using NexusMods.Games.Larian.BaldursGate3;
using NexusMods.Games.Larian.DivinityOriginalSin2DefinitiveEdition;

namespace NexusMods.Games.Larian;

public static class Services
{
    public static IServiceCollection AddLarianGames(this IServiceCollection services)
    {
        services
            // For Dos2
            .AddGame<DivinityOriginalSin2DefinitiveEdition.DivinityOriginalSin2DefinitiveEdition>()
            .AddSettings<DivinityOriginalSin2DefinitiveEditionSettings>()
            // For Baldur's Gate 3
            .AddGame<BaldursGate3.BaldursGate3>()
            .AddSettings<BaldursGate3Settings>();
        return services;
    }
}
