# Company Standards MCP Server (.NET)

This is the .NET implementation of the local company-standards MCP server.

## Why .NET here

- aligns with company backend standards
- easier adoption for .NET teams
- easier reuse of internal auth, hosting, and logging conventions later
- same server core can move from local stdio to internal HTTP hosting later

## Project files

- [Program.cs](Program.cs) - stdio MCP server entry point
- [Services/KnowledgeCatalogService.cs](Services/KnowledgeCatalogService.cs) - catalog loading, search, and bundle generation
- [Models/StandardsCatalog.cs](Models/StandardsCatalog.cs) - catalog models
- [../knowledge/standards/catalog.json](../knowledge/standards/catalog.json) - knowledge index
- [../copilot-instructions.md](../copilot-instructions.md) - original raw instruction source

## Local run

1. restore packages
2. run the project over stdio

Suggested commands:

- `dotnet restore mcp/dotnet/CompanyStandards.McpServer.csproj`
- `dotnet run --project mcp/dotnet/CompanyStandards.McpServer.csproj`

## Exposed capabilities

### Tools

- `list_categories`
- `search_standards`
- `get_standard`
- `build_instruction_bundle`

### Resources

- `standards://catalog`
- `standards://source/legacy-copilot-instructions`
- `standards://document/{id}`
- `standards://category/{category}`

## Internal hosting later

When ready, keep the same content model and move to `ModelContextProtocol.AspNetCore` for HTTP transport.
