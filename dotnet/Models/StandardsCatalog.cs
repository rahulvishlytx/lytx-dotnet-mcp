using System.Text.Json.Serialization;

namespace LytxDotNetStandard.McpServer.Models;

public sealed class StandardsCatalog
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("rawInstructionSources")]
    public List<RawInstructionSource> RawInstructionSources { get; set; } = [];

    [JsonPropertyName("categories")]
    public List<StandardsCategory> Categories { get; set; } = [];

    [JsonPropertyName("documents")]
    public List<StandardDocument> Documents { get; set; } = [];
}

public sealed class RawInstructionSource
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public sealed class StandardsCategory
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public sealed class StandardDocument
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = [];

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("order")]
    public int Order { get; set; }
}

public sealed record StandardSearchResult(
    string Id,
    string Title,
    string Category,
    IReadOnlyList<string> Tags,
    string Uri,
    int Score,
    string Excerpt);
