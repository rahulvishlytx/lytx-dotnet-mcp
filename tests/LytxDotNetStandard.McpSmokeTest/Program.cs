using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

var parsedArgs = ParseArguments(args);
var endpoint = parsedArgs.TryGetValue("endpoint", out var endpointValue)
    ? endpointValue
    : "http://localhost:5000/mcp";
var story = parsedArgs.TryGetValue("story", out var storyValue)
    ? storyValue
    : "Implement a .NET API change that publishes Kafka events and persists data safely";
var techStack = parsedArgs.TryGetValue("techStack", out var techStackValue)
    ? techStackValue
    : ".NET API Kafka PostgreSQL";
var searchQuery = parsedArgs.TryGetValue("search", out var searchValue)
    ? searchValue
    : "sqs kafka authentication";

Console.WriteLine($"Connecting to MCP server: {endpoint}");

try
{
    var transport = new HttpClientTransport(new HttpClientTransportOptions
    {
        Endpoint = new Uri(endpoint),
        TransportMode = HttpTransportMode.AutoDetect,
        ConnectionTimeout = TimeSpan.FromSeconds(30)
    });

    await using var client = await McpClient.CreateAsync(transport);

    Console.WriteLine($"Connected to: {client.ServerInfo.Name} {client.ServerInfo.Version}");
    if (!string.IsNullOrWhiteSpace(client.ServerInstructions))
    {
        Console.WriteLine("Server instructions:");
        Console.WriteLine(client.ServerInstructions);
    }

    var tools = await client.ListToolsAsync();
    Console.WriteLine();
    Console.WriteLine("Tools:");
    foreach (var tool in tools)
    {
        Console.WriteLine($"- {tool.Name}: {tool.Description}");
    }

    var resources = await client.ListResourcesAsync();
    Console.WriteLine();
    Console.WriteLine("Resources:");
    foreach (var resource in resources)
    {
        Console.WriteLine($"- {resource.Uri} ({resource.MimeType})");
    }

    var catalog = await client.ReadResourceAsync("standards://catalog");
    var catalogText = catalog.Contents.OfType<TextResourceContents>().FirstOrDefault()?.Text;
    Console.WriteLine();
    Console.WriteLine("Catalog preview:");
    Console.WriteLine(Preview(catalogText));

    var searchResult = await client.CallToolAsync(
        "search_standards",
        new Dictionary<string, object?>
        {
            ["query"] = searchQuery,
            ["limit"] = 3
        });

    Console.WriteLine();
    Console.WriteLine("search_standards result:");
    PrintToolText(searchResult);

    var bundleResult = await client.CallToolAsync(
        "build_instruction_bundle",
        new Dictionary<string, object?>
        {
            ["story"] = story,
            ["techStack"] = techStack,
            ["maxDocuments"] = 4
        });

    Console.WriteLine();
    Console.WriteLine("build_instruction_bundle result:");
    PrintToolText(bundleResult);

    Console.WriteLine();
    Console.WriteLine("Smoke test completed successfully.");
}
catch (Exception ex)
{
    Console.Error.WriteLine("Smoke test failed.");
    Console.Error.WriteLine(ex.ToString());
    Environment.ExitCode = 1;
}

static void PrintToolText(CallToolResult result)
{
    foreach (var block in result.Content.OfType<TextContentBlock>())
    {
        Console.WriteLine(Preview(block.Text));
    }
}

static string Preview(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return "<empty>";
    }

    const int maxLength = 1200;
    var normalized = value.Replace("\r", string.Empty).Trim();
    return normalized.Length <= maxLength
        ? normalized
        : normalized[..maxLength] + Environment.NewLine + "...";
}

static Dictionary<string, string> ParseArguments(string[] args)
{
    var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    if (args.Length > 0 && Uri.TryCreate(args[0], UriKind.Absolute, out _))
    {
        values["endpoint"] = args[0];
    }

    for (var index = 0; index < args.Length; index++)
    {
        var current = args[index];
        if (!current.StartsWith("--", StringComparison.Ordinal))
        {
            continue;
        }

        var key = current[2..];
        if (index + 1 < args.Length)
        {
            values[key] = args[index + 1];
            index++;
        }
    }

    return values;
}
