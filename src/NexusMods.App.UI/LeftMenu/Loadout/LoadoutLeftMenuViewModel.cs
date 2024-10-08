using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Diagnostics;
using NexusMods.Abstractions.FileStore.Downloads;
using NexusMods.Abstractions.Library;
using NexusMods.Abstractions.Library.Models;
using NexusMods.Abstractions.MnemonicDB.Attributes;
using NexusMods.Abstractions.Settings;
using NexusMods.App.UI.Controls.Navigation;
using NexusMods.App.UI.LeftMenu.Items;
using NexusMods.App.UI.Pages.Diagnostics;
using NexusMods.App.UI.Pages.LibraryPage;
using NexusMods.App.UI.Pages.LoadoutGrid;
using NexusMods.App.UI.Pages.LoadoutPage;
using NexusMods.App.UI.Pages.ModLibrary;
using NexusMods.App.UI.Resources;
using NexusMods.App.UI.Settings;
using NexusMods.App.UI.WorkspaceSystem;
using NexusMods.Icons;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.MnemonicDB.Abstractions.Query;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace NexusMods.App.UI.LeftMenu.Loadout;

public class LoadoutLeftMenuViewModel : AViewModel<ILoadoutLeftMenuViewModel>, ILoadoutLeftMenuViewModel
{
    public IApplyControlViewModel ApplyControlViewModel { get; }

    public ReadOnlyObservableCollection<ILeftMenuItemViewModel> Items { get; }
    public WorkspaceId WorkspaceId { get; }

    [Reactive] private int NewDownloadModelCount { get; set; }

    public LoadoutLeftMenuViewModel(
        LoadoutContext loadoutContext,
        WorkspaceId workspaceId,
        IWorkspaceController workspaceController,
        IServiceProvider serviceProvider)
    {
        var diagnosticManager = serviceProvider.GetRequiredService<IDiagnosticManager>();
        var conn = serviceProvider.GetRequiredService<IConnection>();

        var loadout = Abstractions.Loadouts.Loadout.Load(conn.Db, loadoutContext.LoadoutId);
        var game = loadout.InstallationInstance.Game;

        var settingsManager = serviceProvider.GetRequiredService<ISettingsManager>();

        WorkspaceId = workspaceId;
        ApplyControlViewModel = new ApplyControlViewModel(loadoutContext.LoadoutId, serviceProvider);

        var oldLoadoutItem = new IconViewModel
        {
            Name = Language.LoadoutLeftMenuViewModel_LoadoutGridEntry,
            Icon = IconValues.Collections,
            NavigateCommand = ReactiveCommand.Create<NavigationInformation>(info =>
                {
                    var pageData = new PageData
                    {
                        FactoryId = LoadoutGridPageFactory.StaticId,
                        Context = new LoadoutGridContext { LoadoutId = loadoutContext.LoadoutId },
                    };

                    var behavior = workspaceController.GetOpenPageBehavior(pageData, info);
                    workspaceController.OpenPage(WorkspaceId, pageData, behavior);
                }
            ),
        };

        var loadoutItem = new IconViewModel
        {
            Name = "My Mods (new)",
            Icon = IconValues.Collections,
            NavigateCommand = ReactiveCommand.Create<NavigationInformation>(info =>
            {
                var pageData = new PageData
                {
                    FactoryId = LoadoutPageFactory.StaticId,
                    Context = new LoadoutPageContext
                    {
                        LoadoutId = loadoutContext.LoadoutId,
                    },
                };

                var behavior = workspaceController.GetOpenPageBehavior(pageData, info);
                workspaceController.OpenPage(WorkspaceId, pageData, behavior);
            }),
        };

        var oldLibraryItem = new IconViewModel
        {
            Name = Language.FileOriginsPageTitle,
            Icon = IconValues.ModLibrary,
            NavigateCommand = ReactiveCommand.Create<NavigationInformation>(info =>
            {
                var pageData = new PageData
                {
                    FactoryId = FileOriginsPageFactory.StaticId,
                    Context = new FileOriginsPageContext
                    {
                        LoadoutId = loadoutContext.LoadoutId,
                    },
                };

                var behavior = workspaceController.GetOpenPageBehavior(pageData, info);
                workspaceController.OpenPage(WorkspaceId, pageData, behavior);
            }),
        };

        var libraryItem = new IconViewModel
        {
            Name = "Library (new)",
            Icon = IconValues.ModLibrary,
            NavigateCommand = ReactiveCommand.Create<NavigationInformation>(info =>
            {
                NewDownloadModelCount = 0;

                var pageData = new PageData
                {
                    FactoryId = LibraryPageFactory.StaticId,
                    Context = new LibraryPageContext
                    {
                        LoadoutId = loadoutContext.LoadoutId,
                    },
                };

                var behavior = workspaceController.GetOpenPageBehavior(pageData, info);
                workspaceController.OpenPage(WorkspaceId, pageData, behavior);
            }),
        };

        var diagnosticItem = new IconViewModel
        {
            Name = Language.LoadoutLeftMenuViewModel_LoadoutLeftMenuViewModel_Diagnostics,
            Icon = IconValues.Stethoscope,
            NavigateCommand = ReactiveCommand.Create<NavigationInformation>(info =>
            {
                var pageData = new PageData
                {
                    FactoryId = DiagnosticListPageFactory.StaticId,
                    Context = new DiagnosticListPageContext
                    {
                        LoadoutId = loadoutContext.LoadoutId,
                    },
                };

                var behavior = workspaceController.GetOpenPageBehavior(pageData, info);
                workspaceController.OpenPage(WorkspaceId, pageData, behavior);
            }),
        };


        var items = new ILeftMenuItemViewModel[]
        {
            oldLoadoutItem,
            // TODO: loadoutItem,
            oldLibraryItem,
            // TODO: libraryItem,
            diagnosticItem,
        };

        var observableCollection = new ObservableCollection<ILeftMenuItemViewModel>(items);

        Items = new ReadOnlyObservableCollection<ILeftMenuItemViewModel>(observableCollection);

        this.WhenActivated(disposable =>
        {
            settingsManager
                .GetChanges<ExperimentalViewSettings>(prependCurrent: true)
                .OnUI()
                .Select(x => x.ShowNewTreeViews)
                .SubscribeWithErrorLogging(showNewTreeViews =>
                {
                    observableCollection.Remove(libraryItem);
                    observableCollection.Remove(loadoutItem);

                    if (showNewTreeViews)
                    {
                        observableCollection.Add(libraryItem);
                        observableCollection.Add(loadoutItem);
                    }
                })
                .DisposeWith(disposable);

            diagnosticManager
                .CountDiagnostics(loadoutContext.LoadoutId)
                .OnUI()
                .Select(counts =>
                {
                    var badges = new List<string>(capacity: 3);
                    if (counts.NumCritical != 0)
                        badges.Add(counts.NumCritical.ToString());
                    if (counts.NumWarnings != 0)
                        badges.Add(counts.NumWarnings.ToString());
                    if (counts.NumSuggestions != 0)
                        badges.Add(counts.NumSuggestions.ToString());
                    return badges.ToArray();
                })
                .BindToVM(diagnosticItem, vm => vm.Badges)
                .DisposeWith(disposable);

            LibraryUserFilters.ObserveFilteredLibraryItems(connection: conn)
                .RemoveKey()
                .OnUI()
                .WhereReasonsAre(ListChangeReason.Add, ListChangeReason.AddRange)
                .SubscribeWithErrorLogging(changeSet => NewDownloadModelCount += changeSet.Adds)
                .DisposeWith(disposable);

            // NOTE(erri120): No new downloads when the Left Menu gets loaded. Must be set here because the observable stream
            // above will count all existing downloads, which we want to ignore.
            NewDownloadModelCount = 0;

            this.WhenAnyValue(vm => vm.NewDownloadModelCount)
                .Select(count => count == 0 ? [] : new[] { count.ToString() })
                .BindToVM(oldLibraryItem, vm => vm.Badges)
                .DisposeWith(disposable);
        });
    }
}
