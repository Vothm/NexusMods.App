using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.GameLocators;
using NexusMods.Abstractions.Loadouts;
using NexusMods.App.UI.WorkspaceSystem;

namespace NexusMods.App.UI.Pages.TextEdit;

[UsedImplicitly]
public class TextEditorPageFactory(IServiceProvider serviceProvider) : APageFactory<ITextEditorPageViewModel, LoadoutFileId, GamePath>(serviceProvider)
{
    public static readonly PageFactoryId StaticId = PageFactoryId.From(Guid.Parse("ea4947ea-5f97-4e4d-abf2-bd511fbd9b4e"));
    public override PageFactoryId Id => StaticId;
    
    /// <summary>
    /// Creates PageData for the TextEditorPage, with the provided LoadoutFileId and GamePath.
    /// </summary>
    public static PageData NewPageData(LoadoutFileId loadoutFileId, GamePath gamePath) => CreatePageData(StaticId, loadoutFileId, gamePath);

    protected override ITextEditorPageViewModel CreateViewModel(LoadoutFileId context, GamePath gamePath)
    {
        var vm = ServiceProvider.GetRequiredService<ITextEditorPageViewModel>();
        vm.Context = context;
        return vm;
    }
}
