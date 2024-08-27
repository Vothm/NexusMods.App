using System.Text.Json;
using DynamicData.Kernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexusMods.Abstractions.Cli;
using NexusMods.Abstractions.Games.DTO;
using NexusMods.Abstractions.HttpDownloader;
using NexusMods.Abstractions.IO;
using NexusMods.Abstractions.Library;
using NexusMods.Abstractions.Library.Models;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Abstractions.NexusModsLibrary;
using NexusMods.Abstractions.NexusWebApi;
using NexusMods.Abstractions.NexusWebApi.Types;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.Networking.NexusWebApi.Collections;
using NexusMods.Networking.NexusWebApi.Collections.Json;
using NexusMods.Paths;
using NexusMods.ProxyConsole.Abstractions;
using NexusMods.ProxyConsole.Abstractions.VerbDefinitions;

namespace NexusMods.Networking.NexusWebApi;

internal static class NexusApiVerbs
{
    internal static IServiceCollection AddNexusApiVerbs(this IServiceCollection collection) =>
        collection.AddVerb(() => NexusApiVerify)
            .AddVerb(() => NexusDownloadLinks)
            .AddVerb(() => DownloadCollection)
            .AddVerb(() => NexusGames);


    [Verb("nexus-api-verify", "Verifies the logged in account via the Nexus API")]
    private static async Task<int> NexusApiVerify([Injected] IRenderer renderer,
        [Injected] NexusApiClient nexusApiClient,
        [Injected] IAuthenticatingMessageFactory messageFactory,
        [Injected] CancellationToken token)
    {
        var userInfo = await messageFactory.Verify(nexusApiClient, token);
        await renderer.Table(new[] { "Name", "Premium" },
            new[]
            {
                new object[]
                {
                    userInfo?.Name ?? "<Not logged in>",
                    userInfo?.IsPremium ?? false,
                }
            });

        return 0;
    }

    [Verb("nexus-download-links", "Generates download links for a given file")]
    private static async Task<int> NexusDownloadLinks([Injected] IRenderer renderer,
        [Option("g", "gameDomain", "Game domain")] string gameDomain,
        [Option("m", "modId", "Mod ID")] ModId modId,
        [Option("f", "fileId", "File ID")] FileId fileId,
        [Injected] NexusApiClient nexusApiClient,
        [Injected] CancellationToken token)
    {
        var links = await nexusApiClient.DownloadLinksAsync(gameDomain, modId, fileId, token);

        await renderer.Table(new[] { "Source", "Link" },
            links.Data.Select(x => new object[] { x.ShortName, x.Uri }));
        return 0;
    }

    [Verb("nexus-games", "Lists all games available on Nexus Mods")]
    private static async Task<int> NexusGames([Injected] IRenderer renderer,
        [Injected] NexusApiClient nexusApiClient,
        [Injected] CancellationToken token)
    {
        var results = await nexusApiClient.Games(token);

        await renderer.Table(new[] { "Name", "Domain", "Downloads", "Files" },
            results.Data
                .OrderByDescending(x => x.FileCount)
                .Select(x => new object[] { x.Name, x.DomainName, x.Downloads, x.FileCount }));

        return 0;
    }
    
    [Verb("download-collection", "Downloads a collection and adds it to the library")]
    private static async Task<int> DownloadCollection([Injected] IRenderer renderer,
        [Option("s", "slug", "Collection slug")] CollectionSlug slug,
        [Option("r", "revision", "Revision number")] RevisionNumber revision,
        [Option("l", "loadout", "Loadout name")] Loadout.ReadOnly loadout,
        [Injected] ILogger<NexusApiClient> _logger,
        [Injected] NexusApiClient nexusApiClient,
        [Injected] ILibraryService libraryService,
        [Injected] TemporaryFileManager temporaryFileManager,
        [Injected] NexusModsLibrary nexusModsLibrary,
        [Injected] IHttpDownloader downloader,
        [Injected] IFileStore fileStore,
        [Injected] IConnection connection,
        [Injected] CancellationToken token)
    {
        var downloadLinks = await nexusApiClient.CollectionDownloadLinksAsync(slug, revision, token);

        await using var tmpFile = temporaryFileManager.CreateFile();

        await downloader.DownloadAsync(downloadLinks.Data.Links.Select(f => f.Uri).ToArray(), tmpFile.Path, token: token);
        
        await using var addToLibraryJob = libraryService.AddLocalFile(tmpFile.Path);
        await addToLibraryJob.StartAsync(token);
        var result = await addToLibraryJob.WaitToFinishAsync(token);
        
        if (!result.TryGetCompleted(out var addedFile))
        {
            return 1;
        }
        
        if (!addedFile.TryGetData<LocalFile.ReadOnly>(out var file))
        {
            return 1;
        }
        
        if (!file.AsLibraryFile().TryGetAsLibraryArchive(out var archive))
        {
            return 1;
        }
        
        var collectionJson = archive.Children.FirstOrDefault(x => x.Path == "collection.json");

        CollectionDefinitionJson collectionJsonRoot;
        using (var stream = await fileStore.GetFileStream(collectionJson.AsLibraryFile().Hash, token))
        {
            collectionJsonRoot = (await JsonSerializer.DeserializeAsync<CollectionDefinitionJson>(stream, cancellationToken: token))!;
        }
        
        
        var tasks = new List<Task>();
        
        foreach (var mod in collectionJsonRoot.Mods.Where(m => m.Optional != true))
        {
            if (mod.Name == "WTNC Config")
                continue;
            _logger.LogInformation("Mod: {Name} {Version}", mod.Name, mod.Version);

            var fileId = FileId.From((ulong)mod.Source.FileId!.Value);
            var existing = NexusModsFileMetadata
                .FindByFileId(connection.Db, fileId)
                .Where(f => f.ModPage.ModId == ModId.From((ulong)mod.Source.ModId!))
                .Any(f => f.LibraryFiles.Count != 0);
            
            if (existing)
            {
                _logger.LogInformation("Mod already exists in the library");
                continue;
            }
            
            var modPage = await nexusModsLibrary.GetOrAddModPage(ModId.From((ulong)mod.Source.ModId!), GameDomain.From(collectionJsonRoot.Info.DomainName), token);
            var fileMetadata = await nexusModsLibrary.GetOrAddFile(FileId.From((ulong)mod.Source.FileId!), modPage, GameDomain.From(collectionJsonRoot.Info.DomainName), token);
            
            await using var destination = temporaryFileManager.CreateFile();
            
            _logger.LogInformation("Downloading {File}", fileMetadata.Name);

            var downloadJob = await nexusModsLibrary.CreateDownloadJob(destination, fileMetadata, Optional<(NXMKey, DateTime)>.None, token);
            await using var addJob = libraryService.AddDownload(downloadJob);
            await addJob.StartAsync(token);
            tasks.Add(addJob.WaitToFinishAsync(token));
        }
        
        await Task.WhenAll(tasks);

        var db = connection.Db;
        
        using var tx = connection.BeginTransaction();
        var collectionGroup = new LoadoutItemGroup.New(tx, out var id)
        {
            LoadoutItem = new LoadoutItem.New(tx, id)
            {
                IsDisabled = false,
                LoadoutId = loadout.Id,
                Name = collectionJsonRoot.Info.Name,
            },
        };
        var txResults = await tx.Commit();
        var group = txResults.Remap(collectionGroup);
        
        
        foreach (var mod in collectionJsonRoot.Mods.Where(m => m.Optional != true))
        {
            if (mod.Name == "WTNC Config")
                continue;

            var fileId = FileId.From((ulong)mod.Source.FileId!.Value);
            var fileMetadata = NexusModsFileMetadata
                .FindByFileId(connection.Db, fileId)
                .First(f => f.ModPage.ModId == ModId.From((ulong)mod.Source.ModId!));

            _logger.LogInformation("Installing {File}", fileMetadata.Name);
            var libraryItem = fileMetadata.LibraryFiles.First();

            await using var job = libraryService.InstallItem(libraryItem.AsDownloadedFile().AsLibraryFile().AsLibraryItem(), loadout, null, group.AsLoadoutItem().LoadoutItemId);
            await job.StartAsync(token);
            var res = await job.WaitToFinishAsync(token);
            
            if (res.TryGetCompleted(out var completed))
            {
                _logger.LogInformation("Installed {File}", fileMetadata.Name);
            }
            else
            {
                _logger.LogError("Failed to install {File}", fileMetadata.Name);
            }
        }
        
        

        return 0;
    }

}
