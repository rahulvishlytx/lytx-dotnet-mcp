# Company Standards MCP Server Design

## Objective

Provide a local MCP server now, with a clean upgrade path to internal network hosting later.

## Implemented local design

- Local stdio MCP server entry: [mcp/dotnet/Program.cs](../dotnet/Program.cs)
- .NET project: [mcp/dotnet/CompanyStandards.McpServer.csproj](../dotnet/CompanyStandards.McpServer.csproj)
- Knowledge catalog: [mcp/knowledge/standards/catalog.json](../knowledge/standards/catalog.json)
- Segmented standards docs: [mcp/knowledge/standards/documents](../knowledge/standards/documents)
- Content maintenance guide: [mcp/knowledge/standards/MAINTAINING.md](../knowledge/standards/MAINTAINING.md)
- Raw instruction source: [mcp/copilot-instructions.md](../copilot-instructions.md)
- VS Code registration: [.vscode/mcp.json](../../.vscode/mcp.json)

## Why .NET is a good fit

- matches a .NET-first engineering environment
- easier to align with internal hosting, auth, and operational standards later
- lets backend teams maintain the server with familiar tooling
- still keeps the knowledge base transport-agnostic

## Standard document model

Each standard should have:

- stable `id`
- human title
- category
- tags
- markdown path
- source reference

This keeps retrieval reliable for AI agents.

## Why segmented documents are better than one long file

- easier review in PRs
- easier ownership by different teams
- easier targeted retrieval for story-specific context
- less prompt bloat
- easier long-term maintenance

## Recommended governance model

- standards stored in Git
- PR approval required for changes
- code owners for sensitive categories
- periodic review for outdated standards
- validation in CI using a catalog validation step

## Current server capabilities

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

## Recommended document taxonomy for scale

For larger organizations, extend to:

- `documents/architecture/`
- `documents/api/`
- `documents/backend/`
- `documents/frontend/`
- `documents/data/`
- `documents/integration/`
- `documents/observability/`
- `documents/security/`
- `documents/testing/`
- `documents/release/`

If one topic grows too much, split by:

- core standard
- examples
- checklist
- migration notes
- anti-patterns

## Hosting later on internal network

When ready to host internally:

1. keep the same knowledge base and ids
2. reuse the same server core
3. add HTTP transport
4. add authentication and access control
5. add CI validation and release versioning

That avoids redesign later.
