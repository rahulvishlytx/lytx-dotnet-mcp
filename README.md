# MCP Assets

This folder contains all files related to the company standards MCP server.

## Structure

- [dotnet](dotnet) - primary .NET MCP server implementation
- [node](node) - Node.js prototype and validation scripts
- [knowledge](knowledge) - segmented standards knowledge base
- [docs](docs) - design and maintenance documentation
- [copilot-instructions.md](copilot-instructions.md) - original raw instruction seed

## Primary entry points

- Visual Studio solution: [CompanyStandards.McpServer.sln](CompanyStandards.McpServer.sln)
- Visual Studio project: [dotnet/CompanyStandards.McpServer.csproj](dotnet/CompanyStandards.McpServer.csproj)
- Local MCP config reference: [../.vscode/mcp.json](../.vscode/mcp.json)
- Knowledge catalog: [knowledge/standards/catalog.json](knowledge/standards/catalog.json)

## Notes

The workspace configuration remains at the repository root, but MCP-specific code, knowledge, and docs are centralized here.
