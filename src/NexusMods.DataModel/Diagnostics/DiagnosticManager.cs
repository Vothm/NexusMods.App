using System.Reactive.Disposables;
using DynamicData;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexusMods.Abstractions.Diagnostics;
using NexusMods.Abstractions.Diagnostics.Emitters;
using NexusMods.Abstractions.Diagnostics.References;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Abstractions.Loadouts.Mods;
using NexusMods.Abstractions.Serialization;
using NexusMods.DataModel.Extensions;
using NexusMods.Extensions.BCL;
using NexusMods.Extensions.DynamicData;

namespace NexusMods.DataModel.Diagnostics;

/// <inheritdoc/>
[UsedImplicitly]
internal sealed class DiagnosticManager : IDiagnosticManager
{
    private bool _isDisposed;

    private readonly ILogger<DiagnosticManager> _logger;
    private readonly IDataStore _dataStore;
    private readonly IOptionsMonitor<DiagnosticOptions> _optionsMonitor;

    private readonly ILoadoutDiagnosticEmitter[] _loadoutDiagnosticEmitters;
    private readonly IModDiagnosticEmitter[] _modDiagnosticEmitters;
    private readonly IModFileDiagnosticEmitter[] _modFileDiagnosticEmitters;

    private readonly CompositeDisposable _compositeDisposable;
    private readonly SourceList<Diagnostic> _diagnosticCache = new();

    private readonly CancellationTokenSource _cts = new();

    /// <summary>
    /// Constructor.
    /// </summary>
    public DiagnosticManager(
        ILogger<DiagnosticManager> logger,
        IDataStore dataStore,
        IOptionsMonitor<DiagnosticOptions> optionsMonitor,
        IEnumerable<ILoadoutDiagnosticEmitter> loadoutDiagnosticEmitters,
        IEnumerable<IModDiagnosticEmitter> modDiagnosticEmitters,
        IEnumerable<IModFileDiagnosticEmitter> modFileDiagnosticEmitters,
        ILoadoutRegistry loadoutRegistry)
    {
        _logger = logger;
        _dataStore = dataStore;
        _optionsMonitor = optionsMonitor;

        _loadoutDiagnosticEmitters = loadoutDiagnosticEmitters.ToArray();
        _modDiagnosticEmitters = modDiagnosticEmitters.ToArray();
        _modFileDiagnosticEmitters = modFileDiagnosticEmitters.ToArray();

        _compositeDisposable = new CompositeDisposable();
        _compositeDisposable.Add(Disposable.Create(_cts, cts =>
        {
            try
            {
                cts.Cancel(throwOnFirstException: false);
                cts.Dispose();
            }
            catch (Exception)
            {
                // ignored
            }
        }));

        var loadoutChangesDisposable = loadoutRegistry.LoadoutChanges
            .EnsureUniqueKeys()
            .Transform(id => loadoutRegistry.GetLoadout(id)!)
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            .Filter(loadout => loadout is not null)
            .ForEachChange(change =>
            {
                if (_cts.IsCancellationRequested) return;
                Task.Run(() => OnLoadoutChanged(change.Current), _cts.Token);
            })
            .Subscribe();

        _compositeDisposable.Add(loadoutChangesDisposable);

        // Listen to option changes and update the currently existing diagnostics.
        var disposable = _optionsMonitor.OnChange(newOptions =>
        {
            var filteredDiagnostics = FilterDiagnostics(_diagnosticCache.Items, newOptions);
            _diagnosticCache.Edit(updater =>
            {
                updater.Clear();
                updater.AddRange(filteredDiagnostics);
            });
        });

        if (disposable is not null) _compositeDisposable.Add(disposable);
    }

    public IObservable<IChangeSet<Diagnostic>> DiagnosticChanges => _diagnosticCache.Connect();
    public IEnumerable<Diagnostic> ActiveDiagnostics => _diagnosticCache.Items;

    public async ValueTask OnLoadoutChanged(Loadout loadout)
    {
        await RefreshLoadoutDiagnostics(loadout);
        RefreshModDiagnostics(loadout);
    }

    public void ClearDiagnostics()
    {
        _diagnosticCache.Edit(updater => updater.Clear());
    }

    internal async Task RefreshLoadoutDiagnostics(Loadout loadout)
    {
        // Remove outdated diagnostics for previous revisions of the loadout
        RemoveDiagnostics(kv => kv.DataReferences.Values
            .OfType<LoadoutReference>()
            .Any(loadoutReference =>
                loadoutReference.DataId == loadout.LoadoutId
                && !Equals(loadoutReference.DataStoreId, loadout.DataStoreId)
            )
        );

        var newDiagnostics = await _loadoutDiagnosticEmitters
            .SelectAsync(async emitter =>
            {
                try
                {
                    return await emitter.Diagnose(loadout).ToArrayAsync();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Exception running emitter {Emitter}", emitter.GetType());
                }

                return Array.Empty<Diagnostic>();
            })
            .ToArrayAsync();

        AddDiagnostics(newDiagnostics.SelectMany(x => x));
    }

    internal void RefreshModDiagnostics(Loadout loadout)
    {
        var previousLoadout = loadout.PreviousVersion.Value;

        Mod[] addedMods;
        if (previousLoadout is not null)
        {
            var modChangeSet = loadout.Mods.Diff(previousLoadout.Mods);
            var removedMods = modChangeSet
                .Where(change => change.Reason is ChangeReason.Remove or ChangeReason.Update)
                .Select(change => new ModCursor(loadout.LoadoutId, change.Key))
                .ToHashSet();

            // Remove outdated diagnostics for mods that have been removed or updated.
            RemoveDiagnostics(kv => kv.DataReferences.Values
                .OfType<ModReference>()
                .Any(modReference => removedMods.Contains(modReference.DataId))
            );

            // Prepare a collection of new or updated mods.
            addedMods = modChangeSet
                .Where(change => change.Reason is ChangeReason.Add or ChangeReason.Update)
                .Select(change => change.Key)
                .Select(modId => loadout.Mods[modId])
                .ToArray();
        }
        else
        {
            // Every mod is considered to be new if there is no previous loadout revision.
            addedMods = loadout.Mods
                .Select(x => x.Value)
                .ToArray();
        }

        // Run the emitters on the added mods.
        var newDiagnostics = addedMods
            .SelectMany(mod => _modDiagnosticEmitters
                .SelectMany(emitter => emitter.Diagnose(loadout, mod))
            )
            .ToArray();

        AddDiagnostics(newDiagnostics);
    }

    internal void RefreshModFileDiagnostics()
    {
        // TODO: figure out how to track changes to files (mods and files are "immutable")
        throw new NotImplementedException();
    }

    private void RemoveDiagnostics(Func<Diagnostic, bool> predicate)
    {
        var toRemove = _diagnosticCache.Items.Where(predicate);

        _diagnosticCache.Edit(updater =>
        {
            updater.Remove(toRemove);
        });
    }

    private void AddDiagnostics(IEnumerable<Diagnostic> newDiagnostics)
    {
        var toAdd = FilterDiagnostics(newDiagnostics, _optionsMonitor.CurrentValue);

        _diagnosticCache.Edit(updater =>
        {
            updater.Add(toAdd);
        });
    }

    internal static IEnumerable<Diagnostic> FilterDiagnostics(
        IEnumerable<Diagnostic> diagnostics,
        DiagnosticOptions options)
    {
        return diagnostics
            .Where(x => x.Severity >= options.MinimumSeverity)
            .Where(x => !options.IgnoredDiagnostics.Contains(x.Id));
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _diagnosticCache.Dispose();
        _compositeDisposable.Dispose();

        _isDisposed = true;
    }
}
