using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.App.UI.WorkspaceSystem;

namespace NexusMods.App.UI.Pages.Settings;


[UsedImplicitly]
public class SettingsPageFactory : APageFactory<ISettingsPageViewModel>
{
    public SettingsPageFactory(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public static readonly PageFactoryId StaticId = PageFactoryId.From(Guid.Parse("3DE311A0-0AB0-4191-9CA5-5CE8EA76C393"));
    public override PageFactoryId Id => StaticId;

    protected override ISettingsPageViewModel CreateViewModel()
    {
        return ServiceProvider.GetRequiredService<ISettingsPageViewModel>();
    }
}
