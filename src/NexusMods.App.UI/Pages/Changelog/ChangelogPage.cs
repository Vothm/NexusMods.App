using DynamicData.Kernel;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.App.BuildInfo;
using NexusMods.App.UI.WorkspaceSystem;
using NexusMods.Icons;

namespace NexusMods.App.UI.Pages.Changelog;

[UsedImplicitly]
public class ChangelogPageFactory(IServiceProvider serviceProvider) : APageFactory<IChangelogPageViewModel, Optional<Version>>(serviceProvider)
{
    public static readonly PageFactoryId StaticId = PageFactoryId.From(Guid.Parse("91b9bdb1-bd81-4407-af12-3ac35f05ab20"));
    public override PageFactoryId Id => StaticId;

    public override IChangelogPageViewModel CreateViewModel(Optional<Version> version)
    {
        var vm = ServiceProvider.GetRequiredService<IChangelogPageViewModel>();
        vm.TargetVersion = version;

        return vm;
    }

    public override IEnumerable<PageDiscoveryDetails?> GetDiscoveryDetails(IWorkspaceContext workspaceContext)
    {
        return
        [
            new PageDiscoveryDetails
            {
                SectionName = "Utilities",
                ItemName = "Changelog",
                Icon = IconValues.FileDocumentOutline,
                PageData = CreatePageData(ApplicationConstants.Version),
            },
        ];
    }
}
