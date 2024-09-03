using FluentAssertions;
using NexusMods.Games.RedEngine.Cyberpunk2077;
using NexusMods.Games.RedEngine.Cyberpunk2077.Types;
using NexusMods.Paths;

namespace NexusMods.Games.RedEngine.Tests;

public class ArchiveParserTests
{
    [Fact]
    public async Task CanGetArchiveHashes()
    {
        var path = FileSystem.Shared.GetKnownPath(KnownPath.EntryDirectory).Combine("Resources/ArchiveTest.archive");
        await using var stream = path.Read();
        
        var hashes = await Archive.GetNameHashes(stream);

        hashes.Should().BeEquivalentTo([Fnv1aHash.From(0)]);
    }
}
