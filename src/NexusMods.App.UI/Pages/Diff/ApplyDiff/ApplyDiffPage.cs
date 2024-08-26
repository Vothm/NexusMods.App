using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Loadouts;
using NexusMods.App.UI.WorkspaceSystem;
using NexusMods.Icons;

namespace NexusMods.App.UI.Pages.Diff.ApplyDiff;

[UsedImplicitly]
public class ApplyDiffPageFactory(IServiceProvider serviceProvider) : APageFactory<IApplyDiffViewModel, LoadoutId>(serviceProvider)
{
    public static readonly PageFactoryId StaticId = PageFactoryId.From(Guid.Parse("CA9E8FFE-02E0-4123-9CA2-F68D07D44583"));
    public override PageFactoryId Id => StaticId;

    public override IApplyDiffViewModel CreateViewModel(LoadoutId loadoutId)
    {
        var vm = ServiceProvider.GetRequiredService<IApplyDiffViewModel>();
        vm.Initialize(loadoutId);
        return vm;
    }

    public override IEnumerable<PageDiscoveryDetails?> GetDiscoveryDetails(IWorkspaceContext workspaceContext)
    {
        if (workspaceContext is not LoadoutContext loadoutContext) yield break;

        yield return new PageDiscoveryDetails
        {
            SectionName = "Utilities",
            ItemName = "Preview Apply Changes",
            Icon = IconValues.ListFilled,
            PageData = CreatePageData(loadoutContext.LoadoutId),
        };
    }

}
