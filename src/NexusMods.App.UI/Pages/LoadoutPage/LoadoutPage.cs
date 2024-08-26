using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Abstractions.Settings;
using NexusMods.App.UI.Settings;
using NexusMods.App.UI.Windows;
using NexusMods.App.UI.WorkspaceSystem;
using NexusMods.Icons;

namespace NexusMods.App.UI.Pages.LoadoutPage;

[UsedImplicitly]
public class LoadoutPageFactory : APageFactory<ILoadoutViewModel, LoadoutId>
{
    private readonly ISettingsManager _settingsManager;
    public LoadoutPageFactory(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _settingsManager = serviceProvider.GetRequiredService<ISettingsManager>();
    }

    public static readonly PageFactoryId StaticId = PageFactoryId.From(Guid.Parse("62fda6ce-e6b7-45d6-936f-a8f325bfc644"));
    public override PageFactoryId Id => StaticId;

    public override ILoadoutViewModel CreateViewModel(LoadoutId loadoutId)
    {
        var vm = new LoadoutViewModel(ServiceProvider.GetRequiredService<IWindowManager>(), ServiceProvider, loadoutId);
        return vm;
    }
    
    public override IEnumerable<PageDiscoveryDetails?> GetDiscoveryDetails(IWorkspaceContext workspaceContext)
    {
        if (!_settingsManager.Get<ExperimentalViewSettings>().ShowNewTreeViews) yield break;
        if (workspaceContext is not LoadoutContext loadoutContext) yield break;

        yield return new PageDiscoveryDetails
        {
            SectionName = "Mods",
            ItemName = "My Mods (new)",
            Icon = IconValues.Collections,
            PageData = CreatePageData(loadoutContext.LoadoutId),
        };
    }
}
