using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Jobs;
using NexusMods.Abstractions.NexusModsLibrary;
using NexusMods.Abstractions.NexusWebApi;
using NexusMods.Abstractions.NexusWebApi.Types;
using NexusMods.Extensions.DependencyInjection;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.Networking.NexusWebApi.Auth;
using NexusMods.ProxyConsole.Abstractions.VerbDefinitions;

namespace NexusMods.Networking.NexusWebApi;

/// <summary>
/// Helps with registration of services for Microsoft DI container.
/// </summary>
public static class Services
{
    /// <summary>
    /// Adds the Nexus Web API to your DI Container's service collection.
    /// </summary>
    public static IServiceCollection AddNexusWebApi(this IServiceCollection collection, bool? apiKeyAuth = null)
    {
        collection.AddLoginVerbs();

        collection.AddGraphQLClient()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://api.nexusmods.com/v2/graphql"));

        apiKeyAuth ??= Environment.GetEnvironmentVariable(ApiKeyMessageFactory.NexusApiKeyEnvironmentVariable) != null;

        if (apiKeyAuth!.Value)
        {
            collection
                .AddAllSingleton<IHttpMessageFactory, ApiKeyMessageFactory>()
                .AddSingleton<IAuthenticatingMessageFactory, ApiKeyMessageFactory>();
        }
        else
        {
            collection
                .AddAllSingleton<IHttpMessageFactory, OAuth2MessageFactory>()
                .AddSingleton<IAuthenticatingMessageFactory, OAuth2MessageFactory>();
        }
        collection.AddSingleton<OAuth>();
        collection.AddSingleton<IIDGenerator, IDGenerator>();
        
        // JWToken
        collection.AddAttributeCollection(typeof(JWTToken));
        
        // Nexus API Key
        collection.AddAttributeCollection(typeof(ApiKey));
        
        return collection
            .AddNexusModsLibraryModels()
            .AddOptionParser(p => (CollectionSlug.From(p), (string?)null))
            .AddOptionParser(p => (RevisionNumber.From(uint.Parse(p)), (string?)null))
            .AddSingleton<NexusModsLibrary>()
            .AddWorker<NexusModsDownloadJobWorker>()
            .AddNexusModsDownloadJobPersistedStateModel()
            .AddAllSingleton<ILoginManager, LoginManager>()
            .AddAllSingleton<INexusApiClient, NexusApiClient>()
            .AddHostedService<HandlerRegistration>()
            .AddNexusApiVerbs();
    }
}
