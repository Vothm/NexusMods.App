namespace NexusMods.App.UI.WorkspaceSystem;

public record PageData
{
    public required PageFactoryId FactoryId { get; init; }

    public bool IsEphemeral { get; init; } = false;
    public required object Context { get; init; }
}
