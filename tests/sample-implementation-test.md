# Sample Implementation Test Using the MCP Server

Yes. A good way to test this MCP server is to use it against a small but realistic backend change.

## Suggested test story

**Story**

Add a new .NET API endpoint to return an entity summary by id.

### Acceptance criteria

- endpoint is protected with `[Authorize]`
- user context is passed into the service layer
- service returns `Result<T>`
- data is read through the approved PostgreSQL data access pattern
- logging uses `ITransactionLogger`
- new behavior is behind a feature toggle
- controller maps service result through `HandleResult()`

## Why this is a good test

This story exercises multiple standards at once:

- authentication
- feature toggles
- logging
- database access
- result pattern

That makes it a strong end-to-end validation of the MCP server.

## How to test with the MCP server

### 1. Start the MCP server

- `dotnet run --project mcp/dotnet/CompanyStandards.McpServer.csproj`

### 2. Use the MCP tool `build_instruction_bundle`

Use this input:

```json
{
  "story": "Add a new .NET API endpoint to return an entity summary by id. The endpoint must be authenticated, use feature toggles, log context, read from PostgreSQL, and return Result<T>.",
  "techStack": ".NET API PostgreSQL feature toggle logging",
  "maxDocuments": 5
}
```

Expected standards returned:

- Authentication Standards
- Feature Toggle Standards
- Logging Standards
- Database Access Standards
- Result Pattern Standards

### 3. Ask the coding agent to implement from the bundle

Example request:

> Implement this feature using the company standards from the MCP server. Do not invent new patterns. Follow the returned bundle exactly.

### 4. Validate the output

Check whether the generated code:

- uses `[Authorize]`
- reads user context from `User`
- uses `Result<T>` in the service layer
- uses approved DB access style
- adds logging custom parameters
- uses a feature toggle constant and fallback path
- maps results through `HandleResult()`

## Optional second test

A messaging-focused test can validate different standards.

**Story**

When an entity is updated, publish a Kafka event and add an SQS consumer for downstream processing.

This should pull standards for:

- AWS Kafka Standards
- AWS SQS Standards
- Logging Standards

## Success criteria for the MCP server

The MCP server is working well if it:

1. returns the correct standards for the story
2. avoids unrelated standards
3. produces a usable instruction bundle
4. helps the coding agent generate code aligned to company patterns
5. reduces review corrections
