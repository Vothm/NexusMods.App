using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.App.UI.Resources;
using NexusMods.App.UI.WorkspaceSystem;
using NexusMods.Icons;

namespace NexusMods.App.UI.Pages.MyLoadouts;

[UsedImplicitly]
public class MyLoadoutsPageFactory(IServiceProvider serviceProvider) : APageFactory<IMyLoadoutsViewModel>(serviceProvider)
{
    public static readonly PageFactoryId StaticId = PageFactoryId.From(Guid.Parse("AF0C54D6-04AE-4F80-812E-1DB31A599C58"));
    public override PageFactoryId Id => StaticId;

    protected override IMyLoadoutsViewModel CreateViewModel()
    {
        return ServiceProvider.GetRequiredService<IMyLoadoutsViewModel>();
    }

    public override IEnumerable<PageDiscoveryDetails?> GetDiscoveryDetails(IWorkspaceContext workspaceContext)
    {
        if (workspaceContext is not HomeContext) yield break;

        yield return new PageDiscoveryDetails
        {
            // TODO: Update with design mandate names
            SectionName = "Loadouts",
            ItemName = Language.MyLoadoutsPageTitle,
            Icon = IconValues.ViewCarousel,
            PageData = CreatePageData(),
        };
    }
}
