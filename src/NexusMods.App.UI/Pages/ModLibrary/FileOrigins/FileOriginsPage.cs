using JetBrains.Annotations;
using NexusMods.Abstractions.Loadouts;
using NexusMods.App.UI.Resources;
using NexusMods.App.UI.WorkspaceSystem;
using NexusMods.Icons;

namespace NexusMods.App.UI.Pages.ModLibrary;

[UsedImplicitly]
public class FileOriginsPageFactory(IServiceProvider serviceProvider) : APageFactory<IFileOriginsPageViewModel, LoadoutId>(serviceProvider)
{
    public static readonly PageFactoryId StaticId = PageFactoryId.From(Guid.Parse("B7E092D2-DF06-4329-ABEB-6FEE9373F238"));
    public override PageFactoryId Id => StaticId;

    public override IFileOriginsPageViewModel CreateViewModel(LoadoutId loadoutId)
    {
        return new FileOriginsPageViewModel(
            loadoutId,
            ServiceProvider
        );
    }
    
    public static PageData NewPageData(LoadoutId loadoutId) => CreatePageData(StaticId, loadoutId);

    public override IEnumerable<PageDiscoveryDetails?> GetDiscoveryDetails(IWorkspaceContext workspaceContext)
    {
        if (workspaceContext is not LoadoutContext loadoutContext) yield break;

        yield return new PageDiscoveryDetails
        {
            SectionName = "Mods",
            ItemName = Language.FileOriginsPageTitle,
            Icon = IconValues.ModLibrary,
            PageData = CreatePageData(loadoutContext.LoadoutId),
        };
    }
}
