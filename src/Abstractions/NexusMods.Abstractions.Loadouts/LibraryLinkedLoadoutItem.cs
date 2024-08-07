using System.ComponentModel;
using JetBrains.Annotations;
using NexusMods.Abstractions.Library.Models;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.MnemonicDB.Abstractions.Attributes;
using NexusMods.MnemonicDB.Abstractions.Models;

namespace NexusMods.Abstractions.Loadouts;

/// <summary>
/// Represents a loadout item that is linked to a library item.
/// </summary>
[Include<LoadoutItem>]
[PublicAPI]
public partial class LibraryLinkedLoadoutItem : IModelDefinition
{
    private const string Namespace = "NexusMods.Loadouts.LibraryLinkedLoadoutItem";

    /// <summary>
    /// The linked library item.
    /// </summary>
    public static readonly ReferenceAttribute<LibraryItem> LibraryItem = new(Namespace, nameof(LibraryItem)) { IsIndexed = true };
}

public partial class LibraryLinkedLoadoutItem
{
    public partial struct ReadOnly
    {
        /// <summary>
        /// Adds a retraction which effectively deletes the current archived file from the data store.
        /// </summary>
        /// <param name="tx">The transaction to add the retraction to.</param>
        public void Retract(ITransaction tx) => tx.Retract(Id, LibraryLinkedLoadoutItem.LibraryItem, LibraryItem.Id);
    }
}
