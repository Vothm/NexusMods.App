using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Loadouts;
using NexusMods.App.UI.WorkspaceSystem;

namespace NexusMods.App.UI.Pages.LoadoutGroupFiles;


[UsedImplicitly]
public class LoadoutGroupFilesPageFactory(IServiceProvider serviceProvider) : APageFactory<ILoadoutGroupFilesViewModel, LoadoutItemGroupId>(serviceProvider)
{
    public static readonly PageFactoryId StaticId = PageFactoryId.From(Guid.Parse("85d19521-0414-4a31-a873-43757234ed43"));
    public override PageFactoryId Id => StaticId;
    
    /// <summary>
    /// Creates PageData for the LoadoutGroupFilesPage, with the provided LoadoutId.
    /// </summary>
    public static PageData NewPageData(LoadoutItemGroupId groupId) => CreatePageData(StaticId, groupId);

    public override ILoadoutGroupFilesViewModel CreateViewModel(LoadoutItemGroupId groupId)
    {
        var vm = ServiceProvider.GetRequiredService<ILoadoutGroupFilesViewModel>();
        vm.Context = groupId;
        return vm;
    }
}
