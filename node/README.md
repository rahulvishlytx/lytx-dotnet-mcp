# Company Standards MCP Server

Local MCP server for sharing company coding standards with multiple developers and AI agents.

## What it exposes

- segmented standards as MCP resources
- search and retrieval tools for standards lookup
- prompt templates for Jira story planning and PR review
- the original [copilot-instructions.md](../copilot-instructions.md) as a raw instruction source

## Local usage

1. Install dependencies with `npm install`
2. Validate content with `npm run mcp:validate`
3. Start the local server with `npm run mcp:company-standards`

## VS Code MCP registration

Use [.vscode/mcp.json](../../.vscode/mcp.json) in this workspace.

## Resource model

- `standards://catalog`
- `standards://source/legacy-copilot-instructions`
- `standards://document/{id}`
- `standards://category/{category}`

## Tool model

- `list_categories`
- `search_standards`
- `get_standard`
- `build_instruction_bundle`

## Why this design works for multiple developers

- Git-backed source of truth
- segmented docs instead of one long prompt
- stable document ids for automation
- category and tag based discovery
- same content works locally now and over HTTP later

## Recommended next step for internal hosting

Keep the content and catalog exactly as-is. Add an HTTP transport wrapper later so the same server core can be hosted on the internal network.
