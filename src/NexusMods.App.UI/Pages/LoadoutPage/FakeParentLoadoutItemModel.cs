using DynamicData;
using NexusMods.Abstractions.Loadouts;
using NexusMods.App.UI.Extensions;
using NexusMods.MnemonicDB.Abstractions;
using ObservableCollections;
using R3;

namespace NexusMods.App.UI.Pages.LoadoutPage;

public class FakeParentLoadoutItemModel : LoadoutItemModel
{
    public required IObservable<DateTime> InstalledAtObservable { get; init; }

    public required IObservable<IChangeSet<LoadoutItemId, EntityId>> LoadoutItemIdsObservable { get; init; }
    public ObservableList<LoadoutItemId> LoadoutItemIds { get; private set; } = [];

    public override IReadOnlyCollection<LoadoutItemId> GetLoadoutItemIds() => LoadoutItemIds;

    private readonly IDisposable _modelActivationDisposable;
    public FakeParentLoadoutItemModel() : base(default(LoadoutItemId))
    {
        _modelActivationDisposable = WhenModelActivated(this, static (model, disposables) =>
        {
            model.InstalledAtObservable.OnUI().Subscribe(date => model.InstalledAt.Value = date).AddTo(disposables);

            model.LoadoutItemIdsObservable.OnUI().SubscribeWithErrorLogging(changeSet => model.LoadoutItemIds.ApplyChanges(changeSet)).AddTo(disposables);
            Disposable.Create(model.LoadoutItemIds, static collection => collection.Clear()).AddTo(disposables);
        });
    }

    private bool _isDisposed;
    protected override void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _modelActivationDisposable.Dispose();
            }

            LoadoutItemIds = null!;

            _isDisposed = true;
        }

        base.Dispose(disposing);
    }
}
