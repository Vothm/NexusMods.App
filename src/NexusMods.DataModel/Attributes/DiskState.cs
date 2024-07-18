using NexusMods.Abstractions.Loadouts;
using NexusMods.Abstractions.MnemonicDB.Attributes;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.MnemonicDB.Abstractions.Attributes;
using NexusMods.MnemonicDB.Abstractions.Models;

namespace NexusMods.DataModel.Attributes;


/// <summary>
///     A sibling of <see cref="DiskState"/>, but only for the initial state.
/// </summary>
/// <remarks>
///     We don't want to keep history in <see cref="DiskState"/> as that is only
///     supposed to hold the latest state, so in order to keep things clean,
///     we separated this out to the class.
///
///     This will also make cleaning out loadouts in MneumonicDB easier in the future.
/// </remarks>
public partial class DiskState : IModelDefinition
{
    private static readonly string Namespace = "NexusMods.DataModel.DiskStateRegistry.DiskState";

    /// <summary>
    /// The associated game id.
    /// </summary>
    public static readonly GameDomainAttribute Game = new(Namespace, nameof(Game)) { IsIndexed = true, NoHistory = true };
    
    /// <summary>
    /// The game's root folder. Stored as a string, since AbsolutePaths require an IFileSystem, and we don't know or care
    /// what filesystem is being used when reading/writing the data from the database.
    /// </summary>
    public static readonly StringAttribute Root = new(Namespace, nameof(Root)) { IsIndexed = true, NoHistory = true };
    
    /// <summary>
    /// The File states associated with this disk state.
    /// </summary>
    public static readonly BackReferenceAttribute<FileState> FileStates = new(FileState.DiskState);
    
    /// <summary>
    /// Marks this disk state as being the initial state.
    /// </summary>
    public static readonly MarkerAttribute IsInitial = new(Namespace, nameof(IsInitial));
}


/// <summary>
/// MnemonicDB attributes for the DiskStateTree registry.
/// </summary>
[Include<DiskState>]
public partial class LoadoutDiskState : IModelDefinition
{
    private static readonly string Namespace = "NexusMods.DataModel.DiskStateRegistry.LoadoutDiskState";

    /// <summary>
    /// The associated loadout id.
    /// </summary>
    public static readonly ReferenceAttribute<Loadout> Loadout = new(Namespace, nameof(Loadout)) { IsIndexed = true, NoHistory = true };
    
    /// <summary>
    /// The associated transaction id.
    /// </summary>
    public static readonly TxIdAttribute TxId = new(Namespace, nameof(TxId)) { IsIndexed = true, NoHistory = true };

}

/// <summary>
/// The state of a file on disk, used to track file changes and updates.
/// </summary>
public partial class FileState : IModelDefinition
{
    private static readonly string Namespace = "NexusMods.DataModel.DiskStateRegistry.FileState";
    
    /// <summary>
    /// The DiskState this file state belongs to
    /// </summary>
    public static readonly ReferenceAttribute<DiskState> DiskState = new(Namespace, nameof(DiskState)) { NoHistory = true };
    
    /// <summary>
    /// The path of the file
    /// </summary>
    public static readonly GamePathAttribute Path = new(Namespace, nameof(Path)) { IsIndexed = true, NoHistory = true };
    
    /// <summary>
    /// The last modified date of the file
    /// </summary>
    public static readonly DateTimeAttribute LastModified = new(Namespace, nameof(LastModified)) { NoHistory = true };
    
    /// <summary>
    /// The hash of the file
    /// </summary>
    public static readonly HashAttribute Hash = new(Namespace, nameof(Hash)) { NoHistory = true };
    
    /// <summary>
    /// The size of the file
    /// </summary>
    public static readonly SizeAttribute Size = new(Namespace, nameof(Size)) { NoHistory = true };
}

