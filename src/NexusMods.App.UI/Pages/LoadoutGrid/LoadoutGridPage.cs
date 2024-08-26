using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Loadouts;
using NexusMods.App.UI.Resources;
using NexusMods.App.UI.WorkspaceSystem;
using NexusMods.Icons;

namespace NexusMods.App.UI.Pages.LoadoutGrid;

[UsedImplicitly]
public class LoadoutGridPageFactory(IServiceProvider serviceProvider) : APageFactory<ILoadoutGridViewModel, LoadoutId>(serviceProvider)
{
    public static readonly PageFactoryId StaticId = PageFactoryId.From(Guid.Parse("c6221ce6-cf12-49bf-b32c-8138ef701cc5"));
    public override PageFactoryId Id => StaticId;

    public override ILoadoutGridViewModel CreateViewModel(LoadoutId loadoutId)
    {
        var vm = ServiceProvider.GetRequiredService<ILoadoutGridViewModel>();
        vm.LoadoutId = loadoutId;
        return vm;
    }
    
    /// <summary>
    /// Creates PageData for the LoadoutGridPage, with the provided LoadoutId.
    /// </summary>
    public static PageData NewPageData(LoadoutId loadoutId) => CreatePageData(StaticId, loadoutId);

    public override IEnumerable<PageDiscoveryDetails?> GetDiscoveryDetails(IWorkspaceContext workspaceContext)
    {
        if (workspaceContext is not LoadoutContext loadoutContext) yield break;

        yield return new PageDiscoveryDetails
        {
            SectionName = "Mods",
            ItemName = Language.LoadoutLeftMenuViewModel_LoadoutGridEntry,
            Icon = IconValues.Collections,
            PageData = CreatePageData(loadoutContext.LoadoutId),
        };
    }
}
