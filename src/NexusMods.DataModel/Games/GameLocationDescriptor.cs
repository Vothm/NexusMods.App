﻿using NexusMods.Paths;

namespace NexusMods.DataModel.Games;

/// <summary>
/// Describes details of a game location (<see cref="GameFolderType"/>), e.g. the Data folder for Skyrim.
/// Contains the resolved path, any nested locations and the top level parent.
/// </summary>
public struct GameLocationDescriptor
{
    private readonly List<GameFolderType> _nestedLocations = new();

    /// <summary>
    /// Creates a new instance of <see cref="GameLocationDescriptor"/>.
    /// </summary>
    /// <param name="id">Id of the <see cref="GameFolderType"/> being described</param>
    /// <param name="resolvedPath">The resolved absolute path for the location</param>
    public GameLocationDescriptor(GameFolderType id, AbsolutePath resolvedPath)
    {
        Id = id;
        ResolvedPath = resolvedPath;
    }

    /// <summary>
    /// Identifier of the location being described.
    /// </summary>
    public required GameFolderType Id { get; init; }

    /// <summary>
    /// <see cref="AbsolutePath"/> of the current installation for the location being described.
    /// </summary>
    public required AbsolutePath ResolvedPath { get; init; }

    /// <summary>
    /// If true, no other game location contains this game location.
    /// </summary>
    public bool IsTopLevel { get; private set; } = true;

    private GameFolderType? _topLevelParent = null;

    /// <summary>
    /// The top level location that contains this location, if there is any.
    /// </summary>
    public GameFolderType? TopLevelParent => _topLevelParent;

    /// <summary>
    /// A collection of other <see cref="GameFolderType"/>s that are nested directories of this location.
    /// </summary>
    public IReadOnlyCollection<GameFolderType> NestedLocations => _nestedLocations;

    /// <summary>
    /// Adds the Id of a nested location to the collection of nested locations.
    /// Also sets the <see cref="IsTopLevel"/> property of the nested location to false.
    /// </summary>
    /// <param name="nestedLocation">A <see cref="GameLocationDescriptor"/> of the nested location </param>
    internal void AddNestedLocation(GameLocationDescriptor nestedLocation)
    {
        nestedLocation.IsTopLevel = false;

        _nestedLocations.Add(nestedLocation.Id);
    }

    /// <summary>
    /// Sets the the <see cref="TopLevelParent"/> property of the current location.
    /// </summary>
    /// <param name="topLevelParent"></param>
    internal void SetTopLevelParent(GameFolderType topLevelParent)
    {
        _topLevelParent = topLevelParent;
    }
}
