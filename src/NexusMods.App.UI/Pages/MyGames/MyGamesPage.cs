using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.App.UI.WorkspaceSystem;

namespace NexusMods.App.UI.Pages.MyGames;


[UsedImplicitly]
public class MyGamesPageFactory(IServiceProvider serviceProvider) : APageFactory<IMyGamesViewModel>(serviceProvider)
{
    public static readonly PageFactoryId StaticId = PageFactoryId.From(Guid.Parse("aa75df24-20e8-459d-a1cd-bb757728c019"));
    public override PageFactoryId Id => StaticId;
    
    /// <summary>
    /// The static PageData for the MyGamesPage.
    /// </summary>
    public static PageData PageData { get; } = CreatePageData(StaticId);

    protected override IMyGamesViewModel CreateViewModel()
    {
        return ServiceProvider.GetRequiredService<IMyGamesViewModel>();
    }
}
