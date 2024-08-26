using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.App.UI.WorkspaceSystem;
using NexusMods.Icons;

namespace NexusMods.App.UI.Pages.Downloads;

[UsedImplicitly]
public class InProgressPageFactory(IServiceProvider serviceProvider) : APageFactory<IInProgressViewModel>(serviceProvider)
{
    public static readonly PageFactoryId StaticId = PageFactoryId.From(Guid.Parse("1ca0baaf-725a-47fb-8596-2567734a4113"));
    public override PageFactoryId Id => StaticId;

    protected override IInProgressViewModel CreateViewModel()
    {
        return ServiceProvider.GetRequiredService<IInProgressViewModel>();
    }
    
    /// <summary>
    /// Creates a new <see cref="PageData"/> object with the provided context.
    /// </summary>
    public static PageData NewPageData() => CreatePageData(StaticId);

    public override IEnumerable<PageDiscoveryDetails?> GetDiscoveryDetails(IWorkspaceContext workspaceContext)
    {
        yield return new PageDiscoveryDetails
        {
            // TODO: translations?
            SectionName = "Mods",
            ItemName = "Downloads",
            Icon = IconValues.Downloading,
            PageData = CreatePageData(),
        };
    }
}
