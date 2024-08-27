using NexusMods.Abstractions.NexusWebApi.Types;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.MnemonicDB.Abstractions.Attributes;
using NexusMods.MnemonicDB.Abstractions.ElementComparers;

namespace NexusMods.Abstractions.NexusModsLibrary.Attributes;

/// <summary>
/// An attribute for a collection slug.
/// </summary>
public class RevisionNumberAttribute(string ns, string name) : ScalarAttribute<RevisionNumber, uint>(ValueTags.UInt32, ns, name)
{
    /// <inheritdoc />
    protected override uint ToLowLevel(RevisionNumber value) => (uint)value.Value;

    /// <inheritdoc />
    protected override RevisionNumber FromLowLevel(uint value, ValueTags tag, RegistryId registryId) => RevisionNumber.From(value);
}
