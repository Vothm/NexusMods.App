using System.Text.Json.Serialization;

namespace NexusMods.Networking.NexusWebApi.Collections.Json;

public class CollectionConfig
{
    [JsonPropertyName("recommendNewProfile")]
    public bool? RecommendNewProfile { get; set; }
}

public class Details
{
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class Hash
{
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("md5")]
    public string Md5 { get; set; } = string.Empty;
}

public class Info
{
    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    [JsonPropertyName("authorUrl")]
    public string AuthorUrl { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("installInstructions")]
    public string InstallInstructions { get; set; } = string.Empty;

    [JsonPropertyName("domainName")]
    public string DomainName { get; set; } = string.Empty;

    [JsonPropertyName("gameVersions")]
    public List<string> GameVersions { get; set; } = [];
}

public class Mod
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("optional")]
    public bool? Optional { get; set; }

    [JsonPropertyName("domainName")]
    public string DomainName { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public Source Source { get; set; } = new();

    [JsonPropertyName("hashes")]
    public List<Hash> Hashes { get; set; } = [];

    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    [JsonPropertyName("details")]
    public Details Details { get; set; } = new();

    [JsonPropertyName("phase")]
    public int? Phase { get; set; }
}

public class ModRule
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("reference")]
    public Reference Reference { get; set; } = new();

    [JsonPropertyName("source")]
    public Source Source { get; set; } = new();
}

public class Reference
{
    [JsonPropertyName("fileExpression")]
    public string FileExpression { get; set; } = string.Empty;

    [JsonPropertyName("fileMD5")]
    public string FileMD5 { get; set; } = string.Empty;

    [JsonPropertyName("versionMatch")]
    public string VersionMatch { get; set; } = string.Empty;

    [JsonPropertyName("logicalFileName")]
    public string LogicalFileName { get; set; } = string.Empty;
}

public class CollectionDefinitionJson
{
    [JsonPropertyName("info")]
    public Info Info { get; set; } = new();

    [JsonPropertyName("mods")]
    public List<Mod> Mods { get; set; } = [];

    [JsonPropertyName("modRules")]
    public List<ModRule> ModRules { get; set; } = [];

    [JsonPropertyName("loadOrder")]
    public List<object> LoadOrder { get; set; } = [];

    [JsonPropertyName("tools")]
    public List<object> Tools { get; set; } = [];

    [JsonPropertyName("collectionConfig")]
    public CollectionConfig CollectionConfig { get; set; } = new();
}

public class Source
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("modId")]
    public int? ModId { get; set; }

    [JsonPropertyName("fileId")]
    public int? FileId { get; set; }

    [JsonPropertyName("md5")]
    public string Md5 { get; set; } = string.Empty;

    [JsonPropertyName("fileSize")]
    public int? FileSize { get; set; }

    [JsonPropertyName("logicalFilename")]
    public string LogicalFilename { get; set; } = string.Empty;

    [JsonPropertyName("updatePolicy")]
    public string UpdatePolicy { get; set; } = string.Empty;

    [JsonPropertyName("tag")]
    public string Tag { get; set; } = string.Empty;

    [JsonPropertyName("fileExpression")]
    public string FileExpression { get; set; } = string.Empty;

    [JsonPropertyName("fileMD5")]
    public string FileMD5 { get; set; } = string.Empty;

    [JsonPropertyName("versionMatch")]
    public string VersionMatch { get; set; } = string.Empty;

    [JsonPropertyName("logicalFileName")]
    public string LogicalFileName { get; set; } = string.Empty;
}

