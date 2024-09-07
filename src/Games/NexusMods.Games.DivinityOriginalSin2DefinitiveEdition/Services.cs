using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Games;
using NexusMods.Games.DivinityOriginalSin2DefinitiveEdition;

namespace NexusMods.Games.DivinityOriginalSin2DefinitiveEdition;

public static class Services
{
    public static IServiceCollection AddDivinityOriginalSin2DefinitiveEdition(this IServiceCollection services)
    {
        services
            .AddGame<DivinityOriginalSin2DefinitiveEdition>();
        return services;
    }
}
