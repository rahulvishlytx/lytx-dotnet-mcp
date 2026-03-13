using System.Text;
using System.Text.Json;
using LytxDotNetStandard.McpServer.Models;

namespace LytxDotNetStandard.McpServer.Services;

public sealed class KnowledgeCatalogService
{
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public string WorkspaceRoot { get; }
    public string CatalogPath { get; }

    public KnowledgeCatalogService()
    {
        WorkspaceRoot = ResolveWorkspaceRoot();
        CatalogPath = Path.Combine(WorkspaceRoot, "mcp", "knowledge", "standards", "catalog.json");
    }

    public async Task<StandardsCatalog> LoadCatalogAsync(CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(CatalogPath);
        var catalog = await JsonSerializer.DeserializeAsync<StandardsCatalog>(stream, _jsonOptions, cancellationToken);
        return catalog ?? throw new InvalidOperationException("Unable to deserialize standards catalog.");
    }

    public async Task<string> ReadWorkspaceFileAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var absolutePath = Path.Combine(WorkspaceRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        return await File.ReadAllTextAsync(absolutePath, cancellationToken);
    }

    public async Task<object> BuildCatalogSummaryAsync(CancellationToken cancellationToken = default)
    {
        var catalog = await LoadCatalogAsync(cancellationToken);

        return new
        {
            catalog.Version,
            catalog.Title,
            catalog.Description,
            Categories = catalog.Categories.Select(category => new
            {
                category.Id,
                category.Title,
                category.Description,
                DocumentCount = catalog.Documents.Count(document => document.Category == category.Id)
            }),
            Documents = catalog.Documents
                .OrderBy(document => document.Order)
                .Select(document => new
                {
                    document.Id,
                    document.Title,
                    document.Category,
                    document.Tags,
                    Uri = $"standards://document/{document.Id}"
                }),
            RawInstructionSources = catalog.RawInstructionSources.Select(source => new
            {
                source.Id,
                source.Title,
                Uri = $"standards://source/{source.Id}"
            })
        };
    }

    public async Task<(StandardDocument Document, string Content)?> GetDocumentAsync(string id, CancellationToken cancellationToken = default)
    {
        var catalog = await LoadCatalogAsync(cancellationToken);
        var document = catalog.Documents.FirstOrDefault(entry => string.Equals(entry.Id, id, StringComparison.OrdinalIgnoreCase));
        if (document is null)
        {
            return null;
        }

        var content = await ReadWorkspaceFileAsync(document.Path, cancellationToken);
        return (document, content);
    }

    public async Task<(StandardsCategory Category, IReadOnlyList<(StandardDocument Document, string Content)> Documents, string Markdown)?> GetCategoryBundleAsync(string categoryId, CancellationToken cancellationToken = default)
    {
        var catalog = await LoadCatalogAsync(cancellationToken);
        var category = catalog.Categories.FirstOrDefault(entry => string.Equals(entry.Id, categoryId, StringComparison.OrdinalIgnoreCase));
        if (category is null)
        {
            return null;
        }

        var documents = new List<(StandardDocument Document, string Content)>();
        foreach (var document in catalog.Documents.Where(entry => string.Equals(entry.Category, categoryId, StringComparison.OrdinalIgnoreCase)).OrderBy(entry => entry.Order))
        {
            documents.Add((document, await ReadWorkspaceFileAsync(document.Path, cancellationToken)));
        }

        var markdown = new StringBuilder()
            .AppendLine($"# {category.Title}")
            .AppendLine()
            .AppendLine(category.Description)
            .AppendLine();

        foreach (var (document, content) in documents)
        {
            markdown.AppendLine($"## {document.Title}")
                .AppendLine()
                .AppendLine(content.Trim())
                .AppendLine();
        }

        return (category, documents, markdown.ToString().Trim());
    }

    public async Task<(RawInstructionSource Source, string Content)?> GetRawInstructionSourceAsync(string sourceId = "legacy-copilot-instructions", CancellationToken cancellationToken = default)
    {
        var catalog = await LoadCatalogAsync(cancellationToken);
        var source = catalog.RawInstructionSources.FirstOrDefault(entry => string.Equals(entry.Id, sourceId, StringComparison.OrdinalIgnoreCase));
        if (source is null)
        {
            return null;
        }

        var content = await ReadWorkspaceFileAsync(source.Path, cancellationToken);
        return (source, content);
    }

    public async Task<IReadOnlyList<StandardSearchResult>> SearchDocumentsAsync(string query, string? category = null, int limit = 5, CancellationToken cancellationToken = default)
    {
        var catalog = await LoadCatalogAsync(cancellationToken);
        var queryTokens = Tokenize(query);
        if (queryTokens.Count == 0)
        {
            return [];
        }

        var documents = catalog.Documents.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(category))
        {
            documents = documents.Where(document => string.Equals(document.Category, category, StringComparison.OrdinalIgnoreCase));
        }

        var results = new List<StandardSearchResult>();
        foreach (var document in documents)
        {
            var content = await ReadWorkspaceFileAsync(document.Path, cancellationToken);
            var score = ScoreDocument(queryTokens, document, content);
            if (score <= 0)
            {
                continue;
            }

            results.Add(new StandardSearchResult(
                document.Id,
                document.Title,
                document.Category,
                document.Tags,
                $"standards://document/{document.Id}",
                score,
                BuildExcerpt(content, queryTokens)));
        }

        return results
            .OrderByDescending(result => result.Score)
            .ThenBy(result => result.Title, StringComparer.OrdinalIgnoreCase)
            .Take(limit)
            .ToList();
    }

    public async Task<(string Text, IReadOnlyList<object> Documents)> BuildInstructionBundleAsync(string story, string? techStack, IReadOnlyList<string>? categories, int maxDocuments, CancellationToken cancellationToken = default)
    {
        var catalog = await LoadCatalogAsync(cancellationToken);
        var selected = new List<StandardDocument>();

        if (categories is not null)
        {
            foreach (var category in categories.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                selected.AddRange(catalog.Documents
                    .Where(document => string.Equals(document.Category, category, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(document => document.Order));
            }
        }

        var searchQuery = string.Join(' ', new[] { story, techStack }.Where(value => !string.IsNullOrWhiteSpace(value)));
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var searchResults = await SearchDocumentsAsync(searchQuery, limit: maxDocuments, cancellationToken: cancellationToken);
            foreach (var result in searchResults)
            {
                var document = catalog.Documents.First(entry => string.Equals(entry.Id, result.Id, StringComparison.OrdinalIgnoreCase));
                selected.Add(document);
            }
        }

        var chosen = selected
            .GroupBy(document => document.Id, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .Take(maxDocuments)
            .ToList();

        var sections = new List<string>();
        var metadata = new List<object>();
        foreach (var document in chosen)
        {
            var content = await ReadWorkspaceFileAsync(document.Path, cancellationToken);
            sections.Add($"## {document.Title}\n\n{content.Trim()}");
            metadata.Add(new
            {
                document.Id,
                document.Title,
                document.Category,
                Uri = $"standards://document/{document.Id}"
            });
        }

        var builder = new StringBuilder()
            .AppendLine("# Company Standards Instruction Bundle")
            .AppendLine();

        builder.AppendLine("## Jira Story")
            .AppendLine(story)
            .AppendLine();

        if (!string.IsNullOrWhiteSpace(techStack))
        {
            builder.AppendLine("## Tech Stack")
                .AppendLine(techStack)
                .AppendLine();
        }

        builder.AppendLine("## Relevant Standards");
        if (sections.Count == 0)
        {
            builder.AppendLine("No direct standard match found. Fall back to the catalog and the legacy instruction source.");
        }
        else
        {
            builder.AppendLine()
                .AppendLine(string.Join(Environment.NewLine + Environment.NewLine, sections));
        }

        builder.AppendLine()
            .AppendLine("## Working Rules")
            .AppendLine("- Prefer existing company patterns before introducing new abstractions.")
            .AppendLine("- Keep acceptance criteria and tests aligned with the standard.")
            .AppendLine("- If no standard exists, document the gap and propose a follow-up standards update.");

        return (builder.ToString().Trim(), metadata);
    }

    public string Serialize(object value) => JsonSerializer.Serialize(value, _jsonOptions);

    private static int ScoreDocument(IReadOnlyList<string> queryTokens, StandardDocument document, string content)
    {
        var title = Normalize(document.Title);
        var category = Normalize(document.Category);
        var tags = document.Tags.Select(Normalize).ToArray();
        var body = Normalize(content);
        var score = 0;

        foreach (var token in queryTokens)
        {
            if (title.Contains(token, StringComparison.Ordinal)) score += 8;
            if (category.Contains(token, StringComparison.Ordinal)) score += 5;
            if (tags.Any(tag => tag.Contains(token, StringComparison.Ordinal))) score += 4;
            if (body.Contains(token, StringComparison.Ordinal)) score += 2;
        }

        return score;
    }

    private static string BuildExcerpt(string content, IReadOnlyList<string> queryTokens)
    {
        var lines = content.Split(["\r\n", "\n"], StringSplitOptions.None);
        var matchIndex = Array.FindIndex(lines, line => queryTokens.Any(token => Normalize(line).Contains(token, StringComparison.Ordinal)));
        if (matchIndex < 0)
        {
            return string.Join(Environment.NewLine, lines.Take(8)).Trim();
        }

        var start = Math.Max(0, matchIndex - 2);
        var end = Math.Min(lines.Length, matchIndex + 4);
        return string.Join(Environment.NewLine, lines[start..end]).Trim();
    }

    private static List<string> Tokenize(string value)
    {
        return Normalize(value)
            .Split([' ', '\t', '\r', '\n', ',', '.', ':', ';', '(', ')', '[', ']', '{', '}', '/', '\\', '-', '_', '#'], StringSplitOptions.RemoveEmptyEntries)
            .Where(token => token.Length > 1)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToLowerInvariant();

    private static string ResolveWorkspaceRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var catalogPath = Path.Combine(current.FullName, "mcp", "knowledge", "standards", "catalog.json");
            if (File.Exists(catalogPath))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Unable to locate workspace root containing mcp/knowledge/standards/catalog.json.");
    }
}
