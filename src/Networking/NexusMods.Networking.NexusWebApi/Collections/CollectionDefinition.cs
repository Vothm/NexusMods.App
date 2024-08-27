using NexusMods.Abstractions.MnemonicDB.Attributes;
using NexusMods.MnemonicDB.Abstractions.Attributes;
using NexusMods.MnemonicDB.Abstractions.Models;

namespace NexusMods.Networking.NexusWebApi.Collections;

/// <summary>
/// A collection definition.
/// </summary>
public partial class CollectionDefinition : IModelDefinition
{
    private const string Namespace = "NexusMods.Networking.NexusWebApi.CollectionDefinition";
    
    /// <summary>
    /// Name of the author of the collection.
    /// </summary>
    public static readonly StringAttribute Author = new(Namespace, nameof(Author));
    
    /// <summary>
    /// The author's URI.
    /// </summary>
    public static readonly UriAttribute AuthorUri = new(Namespace, nameof(AuthorUri)) { IsOptional = true };

    /// <summary>
    /// The name of the collection.
    /// </summary>
    public static readonly StringAttribute Name = new(Namespace, nameof(Name));
    
    /// <summary>
    /// The description of the collection.
    /// </summary>
    public static readonly StringAttribute Description = new(Namespace, nameof(Description)) { IsOptional = true };
    
    /// <summary>
    /// Installation instructions for the collection.
    /// </summary>
    public static readonly StringAttribute InstallInstructions = new(Namespace, nameof(InstallInstructions)) { IsOptional = true };
    
    /// <summary>
    /// The collection's game domain.
    /// </summary>
    public static readonly GameDomainAttribute GameDomain = new(Namespace, nameof(GameDomain));
    
    /// <summary>
    /// Game versions for which the collection is available.
    /// </summary>
    public static readonly StringsAttribute Versions = new(Namespace, nameof(Versions));
}
