using JetBrains.Annotations;
using NexusMods.Abstractions.MnemonicDB.Attributes;
using NexusMods.Abstractions.NexusModsLibrary.Attributes;
using NexusMods.MnemonicDB.Abstractions.Attributes;
using NexusMods.MnemonicDB.Abstractions.Models;

namespace NexusMods.Abstractions.NexusModsLibrary;

/// <summary>
/// The metadata of a collection. 
/// </summary>
[UsedImplicitly]
public partial class NexusModsCollectionMetadata : IModelDefinition
{
    private const string Namespace = "NexusMods.Abstractions.NexusModsLibrary.NexusModsCollectionMetadata";
    
    /// <summary>
    /// The name of the collection.
    /// </summary>
    public static readonly StringAttribute Name = new(Namespace, nameof(Name));
    
    /// <summary>
    /// The collection's slug.
    /// </summary>
    public static readonly CollectionSlugAttribute Slug = new(Namespace, nameof(Slug)) { IsIndexed = true };
    
    /// <summary>
    /// The collection's revision.
    /// </summary>
    public static readonly RevisionNumberAttribute Revision = new(Namespace, nameof(Revision)) { IsIndexed = true };
    
    /// <summary>
    /// The game domain of the collection.
    /// </summary>
    public static readonly GameDomainAttribute GameDomainAttribute = new(Namespace, nameof(GameDomainAttribute));
}
