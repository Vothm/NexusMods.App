using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using NexusMods.Abstractions.NexusWebApi.DTOs.Interfaces;
using CDNName = NexusMods.Abstractions.NexusWebApi.Types.CDNName;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace NexusMods.Abstractions.NexusWebApi.DTOs;

/// <summary>
/// Represents an individual download link returned from the API.
/// </summary>
/// <remarks>
///    At the current moment in time; only premium users can receive this; with the exception of NXM links.
/// </remarks>
public class DownloadLinks : IJsonSerializable<DownloadLinks>
{
    /// <summary>
    /// The download links.
    /// </summary>
    [JsonPropertyName("download_links")]
    public DownloadLink[] Links { get; set; } = [];

    /// <inheritdoc />
    public static JsonTypeInfo<DownloadLinks> GetTypeInfo() => DownloadLinksContext.Default.DownloadLinks;
}

/// <summary/>
[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(DownloadLinks))]
public partial class DownloadLinksContext : JsonSerializerContext { }
