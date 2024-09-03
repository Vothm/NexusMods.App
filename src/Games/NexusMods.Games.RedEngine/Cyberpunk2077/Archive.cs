using System.Runtime.InteropServices;
using NexusMods.Games.RedEngine.Cyberpunk2077.Types;

namespace NexusMods.Games.RedEngine.Cyberpunk2077;

/// <summary>
/// Functions for reading basic metadata from a Cyberpunk 2077 archives
/// </summary>
public class Archive
{

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 56)]
    public unsafe struct FileRecord
    {
        readonly Fnv1aHash NameHash64;
        long Timestamp;
        uint NumInlineBufferSegments;
        uint SegmentsStart;
        uint SegmentsEnd;
        uint ResourceDependencyStart;
        uint ResourceDependencyEnd;
        fixed byte Sha1Hash[20];
    }

    public static async Task<FileRecord[]> GetNameHashes(Stream stream)
    {
        var nameHash64Values = new List<Fnv1aHash>();

        using var reader = new BinaryReader(stream);
        // Assuming the header provides the offset and size of the file list
        // Read Header values to locate File List (IndexPosition and IndexSize should be read from the header)
        stream.Seek(8, SeekOrigin.Begin); // Skip Magic and Version
        var indexPosition = reader.ReadUInt64(); // IndexPosition
        var indexSize = reader.ReadUInt32(); // IndexSize

        // Move to the File List
        stream.Seek((long)indexPosition, SeekOrigin.Begin);

        // Read File List data
        var fileEntryOffset = reader.ReadUInt32(); // Entry Offset - always 8
        var fileTableSize = reader.ReadUInt32(); // Size of the file table
        var crc = reader.ReadUInt64(); // CRC of the file table
        var fileEntryCount = reader.ReadUInt32(); // Number of file entries
        var fileSegmentCount = reader.ReadUInt32(); // Number of file segments
        var resourceDependencyCount = reader.ReadUInt32(); // Number of resource dependencies

        var chunk = GC.AllocateArray<byte>((int)fileEntryCount * 56);
        
        await stream.ReadExactlyAsync(chunk, 0, (int)fileEntryCount * 56);


        var memory = new Memory<byte>(chunk);
        var castedMemory = MemoryMarshal.Cast<byte, FileRecord>(memory.Span).ToArray();


        return castedMemory;
    }
    
}
