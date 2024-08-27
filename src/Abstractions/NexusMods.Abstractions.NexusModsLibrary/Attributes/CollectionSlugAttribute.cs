using NexusMods.Abstractions.NexusWebApi.Types;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.MnemonicDB.Abstractions.Attributes;
using NexusMods.MnemonicDB.Abstractions.ElementComparers;

namespace NexusMods.Abstractions.NexusModsLibrary.Attributes;

/// <summary>
/// An attribute for a collection slug.
/// </summary>
public class CollectionSlugAttribute(string ns, string name) : ScalarAttribute<CollectionSlug, string>(ValueTags.Ascii, ns, name)
{
    /// <inheritdoc />
    protected override string ToLowLevel(CollectionSlug value) => value.Value;

    /// <inheritdoc />
    protected override CollectionSlug FromLowLevel(string value, ValueTags tag, RegistryId registryId) => CollectionSlug.From(value);
}
