# MCP Server Testing

## Quick smoke test

1. Start the server:
   - `dotnet run --project mcp/dotnet/CompanyStandards.McpServer.csproj`
2. In another terminal, run the smoke test:
   - `dotnet run --project mcp/tests/CompanyStandards.McpSmokeTest/CompanyStandards.McpSmokeTest.csproj`

## Custom endpoint

- `dotnet run --project mcp/tests/CompanyStandards.McpSmokeTest/CompanyStandards.McpSmokeTest.csproj -- http://localhost:5000/mcp`

## What it validates

- HTTP connection to the MCP endpoint
- server initialization handshake
- tool discovery
- resource discovery
- resource read for `standards://catalog`
- tool call for `search_standards`
- tool call for `build_instruction_bundle`

## Manual test options

You can also test through:

- VS Code using [.vscode/mcp.json](../../.vscode/mcp.json)
- Postman or another HTTP client if you want raw MCP protocol testing
- a future xUnit integration test project if you want this in CI
