using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Loadouts;
using NexusMods.App.UI.Resources;
using NexusMods.App.UI.WorkspaceSystem;
using NexusMods.Icons;

namespace NexusMods.App.UI.Pages.Diagnostics;

[UsedImplicitly]
public class DiagnosticListPageFactory(IServiceProvider serviceProvider) : APageFactory<IDiagnosticListViewModel, LoadoutId>(serviceProvider)
{
    public static readonly PageFactoryId StaticId = PageFactoryId.From(Guid.Parse("db77a8c2-61ad-4d59-8e95-4bebbba9ea5f"));
    public override PageFactoryId Id => StaticId;

    public override IDiagnosticListViewModel CreateViewModel(LoadoutId loadoutId)
    {
        var vm = ServiceProvider.GetRequiredService<IDiagnosticListViewModel>();
        vm.LoadoutId = loadoutId;
        return vm;
    }

    public override IEnumerable<PageDiscoveryDetails?> GetDiscoveryDetails(IWorkspaceContext workspaceContext)
    {
        if (workspaceContext is not LoadoutContext loadoutContext) yield break;

        yield return new PageDiscoveryDetails
        {
            // TODO: translations?
            SectionName = "Utilities",
            ItemName = Language.DiagnosticListViewModel_DiagnosticListViewModel_Diagnostics,
            Icon = IconValues.Stethoscope,
            PageData = CreatePageData(loadoutContext.LoadoutId),
        };
    }
}
