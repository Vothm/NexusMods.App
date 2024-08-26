using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Abstractions.Settings;
using NexusMods.App.UI.Settings;
using NexusMods.App.UI.Windows;
using NexusMods.App.UI.WorkspaceSystem;
using NexusMods.Icons;

namespace NexusMods.App.UI.Pages.LibraryPage;


[UsedImplicitly]
public class LibraryPageFactory : APageFactory<ILibraryViewModel, LoadoutId>
{
    private readonly ISettingsManager _settingsManager;
    public LibraryPageFactory(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _settingsManager = serviceProvider.GetRequiredService<ISettingsManager>();
    }

    public static readonly PageFactoryId StaticId = PageFactoryId.From(Guid.Parse("547926e3-56ba-4ed1-912d-d0d7e8b7e287"));
    public override PageFactoryId Id => StaticId;

    public override ILibraryViewModel CreateViewModel(LoadoutId loadoutId)
    {
        var vm = new LibraryViewModel(ServiceProvider.GetRequiredService<IWindowManager>(), ServiceProvider, loadoutId);
        return vm;
    }

    public override IEnumerable<PageDiscoveryDetails?> GetDiscoveryDetails(IWorkspaceContext workspaceContext)
    {
        if (!_settingsManager.Get<ExperimentalViewSettings>().ShowNewTreeViews) yield break;
        if (workspaceContext is not LoadoutContext loadoutContext) yield break;

        yield return new PageDiscoveryDetails
        {
            SectionName = "Mods",
            ItemName = "Library (new)",
            Icon = IconValues.ModLibrary,
            PageData = CreatePageData(loadoutContext.LoadoutId),
        };
    }
}
