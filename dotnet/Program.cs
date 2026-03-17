using System.Text.Json;
using LytxDotNetStandard.McpServer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

var builder = WebApplication.CreateBuilder(args);
var catalogService = new KnowledgeCatalogService();

builder.WebHost.ConfigureKestrel((context, kestrel) =>
{
    kestrel.Configure(context.Configuration.GetSection("Kestrel"));
});

builder.Services.AddSingleton(catalogService);

builder.Services
    .AddMcpServer(options =>
    {
        options.ServerInfo = new Implementation
        {
            Name = "lytx-dotnet-standard-mcp-server",
            Version = "0.1.0"
        };
        options.ServerInstructions = "Use this server to retrieve company engineering standards, build Jira-story-specific instruction bundles, and review planned changes against segmented internal guidance.";
        options.ProtocolVersion = "2024-11-05";
        options.Capabilities = new ServerCapabilities
        {
            Tools = new ToolsCapability { ListChanged = false },
            Resources = new ResourcesCapability { Subscribe = false, ListChanged = false },
            Prompts = new PromptsCapability { ListChanged = false }
        };
        options.Handlers = new McpServerHandlers
        {
            ListToolsHandler = (request, cancellationToken) => ValueTask.FromResult(BuildListToolsResult()),
            CallToolHandler = async (request, cancellationToken) => await HandleToolCallAsync(request, catalogService, cancellationToken),
            ListResourcesHandler = async (request, cancellationToken) => await HandleListResourcesAsync(catalogService, cancellationToken),
            ReadResourceHandler = async (request, cancellationToken) => await HandleReadResourceAsync(request, catalogService, cancellationToken)
        };
    })
    .WithHttpTransport()
    .AddAuthorizationFilters();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    name = "lytx-dotnet-standard-mcp-server",
    version = "0.1.0",
    protocol = "mcp",
    endpoint = "/mcp",
    description = "Company engineering standards MCP server"
}));

app.MapMcp("/mcp");

await app.RunAsync();

static ListToolsResult BuildListToolsResult()
{
    return new ListToolsResult
    {
        Tools =
        [
            new Tool
            {
                Name = "list_categories",
                Description = "List available company standards categories.",
                InputSchema = JsonSerializer.SerializeToElement(new
                {
                    type = "object",
                    properties = new { },
                    additionalProperties = false
                })
            },
            new Tool
            {
                Name = "search_standards",
                Description = "Search segmented company standards by Jira story, technology, or keyword.",
                InputSchema = JsonSerializer.SerializeToElement(new
                {
                    type = "object",
                    properties = new
                    {
                        query = new { type = "string", description = "Search terms." },
                        category = new { type = "string", description = "Optional category filter." },
                        limit = new { type = "integer", minimum = 1, maximum = 10, description = "Maximum number of matches." }
                    },
                    required = new[] { "query" },
                    additionalProperties = false
                })
            },
            new Tool
            {
                Name = "get_standard",
                Description = "Get the full text of a standard document by id.",
                InputSchema = JsonSerializer.SerializeToElement(new
                {
                    type = "object",
                    properties = new
                    {
                        id = new { type = "string", description = "Document id from the catalog." }
                    },
                    required = new[] { "id" },
                    additionalProperties = false
                })
            },
            new Tool
            {
                Name = "build_instruction_bundle",
                Description = "Create a context bundle for implementing or reviewing a Jira story against company standards.",
                InputSchema = JsonSerializer.SerializeToElement(new
                {
                    type = "object",
                    properties = new
                    {
                        story = new { type = "string", description = "Jira story summary and acceptance criteria." },
                        techStack = new { type = "string", description = "Optional stack hint." },
                        categories = new
                        {
                            type = "array",
                            items = new { type = "string" },
                            description = "Optional categories to force include."
                        },
                        maxDocuments = new { type = "integer", minimum = 1, maximum = 10, description = "Maximum number of documents." }
                    },
                    required = new[] { "story" },
                    additionalProperties = false
                })
            }
        ]
    };
}

static async ValueTask<CallToolResult> HandleToolCallAsync(
    RequestContext<CallToolRequestParams> request,
    KnowledgeCatalogService catalogService,
    CancellationToken cancellationToken)
{
    var toolName = request.Params?.Name ?? throw new McpProtocolException("Tool name is required.", McpErrorCode.InvalidRequest);
    var arguments = request.Params?.Arguments;

    return toolName switch
    {
        "list_categories" => await ListCategoriesAsync(catalogService, cancellationToken),
        "search_standards" => await SearchStandardsAsync(arguments, catalogService, cancellationToken),
        "get_standard" => await GetStandardAsync(arguments, catalogService, cancellationToken),
        "build_instruction_bundle" => await BuildInstructionBundleAsync(arguments, catalogService, cancellationToken),
        _ => throw new McpProtocolException($"Unknown tool: {toolName}", McpErrorCode.InvalidRequest)
    };
}

static async ValueTask<CallToolResult> ListCategoriesAsync(KnowledgeCatalogService catalogService, CancellationToken cancellationToken)
{
    var catalog = await catalogService.LoadCatalogAsync(cancellationToken);
    var text = string.Join(Environment.NewLine, catalog.Categories.Select(category => $"- {category.Id}: {category.Title} — {category.Description}"));

    return CreateToolResult(text, new
    {
        categories = catalog.Categories.Select(category => new
        {
            category.Id,
            category.Title,
            category.Description,
            Uri = $"standards://category/{category.Id}"
        })
    });
}

static async ValueTask<CallToolResult> SearchStandardsAsync(IDictionary<string, JsonElement>? arguments, KnowledgeCatalogService catalogService, CancellationToken cancellationToken)
{
    var query = GetRequiredString(arguments, "query");
    var category = GetOptionalString(arguments, "category");
    var limit = GetOptionalInt(arguments, "limit") ?? 5;

    var results = await catalogService.SearchDocumentsAsync(query, category, limit, cancellationToken);
    var text = results.Count > 0
        ? string.Join(Environment.NewLine + Environment.NewLine + "---" + Environment.NewLine + Environment.NewLine,
            results.Select(result => $"# {result.Title}{Environment.NewLine}Category: {result.Category}{Environment.NewLine}URI: {result.Uri}{Environment.NewLine}{Environment.NewLine}{result.Excerpt}"))
        : "No matching standards found.";

    return CreateToolResult(text, new { matches = results });
}

static async ValueTask<CallToolResult> GetStandardAsync(IDictionary<string, JsonElement>? arguments, KnowledgeCatalogService catalogService, CancellationToken cancellationToken)
{
    var id = GetRequiredString(arguments, "id");
    var document = await catalogService.GetDocumentAsync(id, cancellationToken);

    return CreateToolResult(
        document?.Content ?? $"No standard document found for '{id}'.",
        new
        {
            document = document is null
                ? null
                : new
                {
                    document.Value.Document.Id,
                    document.Value.Document.Title,
                    document.Value.Document.Category,
                    document.Value.Document.Path,
                    Uri = $"standards://document/{document.Value.Document.Id}"
                }
                });
}

static async ValueTask<CallToolResult> BuildInstructionBundleAsync(IDictionary<string, JsonElement>? arguments, KnowledgeCatalogService catalogService, CancellationToken cancellationToken)
{
    var story = GetRequiredString(arguments, "story");
    var techStack = GetOptionalString(arguments, "techStack");
    var categories = GetOptionalStringArray(arguments, "categories");
    var maxDocuments = GetOptionalInt(arguments, "maxDocuments") ?? 5;

    var bundle = await catalogService.BuildInstructionBundleAsync(story, techStack, categories, maxDocuments, cancellationToken);
    return CreateToolResult(bundle.Text, new
    {
        story,
        techStack,
        documents = bundle.Documents
    });
}

static async ValueTask<ListResourcesResult> HandleListResourcesAsync(KnowledgeCatalogService catalogService, CancellationToken cancellationToken)
{
    var catalog = await catalogService.LoadCatalogAsync(cancellationToken);

    var resources = new List<Resource>
    {
        new()
        {
            Uri = "standards://catalog",
            Name = "Company standards catalog",
            MimeType = "application/json",
            Description = "Catalog of segmented company standards documents."
        },
        new()
        {
            Uri = "standards://source/legacy-copilot-instructions",
            Name = "Legacy copilot instructions",
            MimeType = "text/markdown",
            Description = "Raw instruction source provided by engineering."
        }
    };

    resources.AddRange(catalog.Categories.Select(category => new Resource
    {
        Uri = $"standards://category/{category.Id}",
        Name = category.Title,
        MimeType = "text/markdown",
        Description = category.Description
    }));

    resources.AddRange(catalog.Documents.OrderBy(document => document.Order).Select(document => new Resource
    {
        Uri = $"standards://document/{document.Id}",
        Name = document.Title,
        MimeType = "text/markdown",
        Description = $"{document.Category} standard"
    }));

    return new ListResourcesResult { Resources = resources };
}

static async ValueTask<ReadResourceResult> HandleReadResourceAsync(
    RequestContext<ReadResourceRequestParams> request,
    KnowledgeCatalogService catalogService,
    CancellationToken cancellationToken)
{
    var uri = request.Params?.Uri ?? throw new McpProtocolException("Resource URI is required.", McpErrorCode.InvalidRequest);

    if (string.Equals(uri, "standards://catalog", StringComparison.OrdinalIgnoreCase))
    {
        var summary = await catalogService.BuildCatalogSummaryAsync(cancellationToken);
        return CreateTextResourceResult(uri, catalogService.Serialize(summary), "application/json");
    }

    if (string.Equals(uri, "standards://source/legacy-copilot-instructions", StringComparison.OrdinalIgnoreCase))
    {
        var source = await catalogService.GetRawInstructionSourceAsync(cancellationToken: cancellationToken);
        return CreateTextResourceResult(uri, source?.Content ?? "Legacy source not found.", "text/markdown");
    }

    if (uri.StartsWith("standards://document/", StringComparison.OrdinalIgnoreCase))
    {
        var id = uri["standards://document/".Length..];
        var document = await catalogService.GetDocumentAsync(id, cancellationToken);
        return CreateTextResourceResult(uri, document?.Content ?? $"No standard document found for '{id}'.", "text/markdown");
    }

    if (uri.StartsWith("standards://category/", StringComparison.OrdinalIgnoreCase))
    {
        var categoryId = uri["standards://category/".Length..];
        var bundle = await catalogService.GetCategoryBundleAsync(categoryId, cancellationToken);
        return CreateTextResourceResult(uri, bundle?.Markdown ?? $"No category bundle found for '{categoryId}'.", "text/markdown");
    }

    throw new McpProtocolException($"Unknown resource: {uri}", McpErrorCode.InvalidRequest);
}

static ReadResourceResult CreateTextResourceResult(string uri, string text, string mimeType)
{
    return new ReadResourceResult
    {
        Contents =
        [
            new TextResourceContents
            {
                Uri = uri,
                Text = text,
                MimeType = mimeType
            }
        ]
    };
}

static CallToolResult CreateToolResult(string text, object structuredContent)
{
    return new CallToolResult
    {
        Content =
        [
            new TextContentBlock
            {
                Text = text
            }
        ],
        StructuredContent = JsonSerializer.SerializeToElement(structuredContent)
    };
}

static string GetRequiredString(IDictionary<string, JsonElement>? arguments, string key)
{
    if (arguments is null || !arguments.TryGetValue(key, out var value) || value.ValueKind != JsonValueKind.String)
    {
        throw new McpProtocolException($"Missing required string argument '{key}'.", McpErrorCode.InvalidParams);
    }

    return value.GetString()!;
}

static string? GetOptionalString(IDictionary<string, JsonElement>? arguments, string key)
{
    if (arguments is null || !arguments.TryGetValue(key, out var value) || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
    {
        return null;
    }

    return value.ValueKind == JsonValueKind.String ? value.GetString() : null;
}

static int? GetOptionalInt(IDictionary<string, JsonElement>? arguments, string key)
{
    if (arguments is null || !arguments.TryGetValue(key, out var value))
    {
        return null;
    }

    return value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var intValue) ? intValue : null;
}

static IReadOnlyList<string> GetOptionalStringArray(IDictionary<string, JsonElement>? arguments, string key)
{
    if (arguments is null || !arguments.TryGetValue(key, out var value) || value.ValueKind != JsonValueKind.Array)
    {
        return [];
    }

    return value.EnumerateArray()
        .Where(item => item.ValueKind == JsonValueKind.String)
        .Select(item => item.GetString())
        .OfType<string>()
        .ToList();
}
