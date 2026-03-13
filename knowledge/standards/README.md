# Company Standards Knowledge Base

This folder is the structured knowledge source for the local MCP server.

## Goals

- Keep company standards in small, reviewable documents.
- Support multiple teams and long lists of standards without one giant prompt file.
- Make the same content usable by AI agents, reviewers, and developers.

## Structure

- `catalog.json` - master index used by the MCP server
- `documents/<category>/<topic>.md` - one standard per document
- `MAINTAINING.md` - update process and contribution rules
- [mcp/copilot-instructions.md](../../copilot-instructions.md) - original raw source reference

## Segmentation Rules

1. Keep one standard or one pattern family per file.
2. Keep examples close to the standard they belong to.
3. Use categories for discovery, not giant monolithic documents.
4. Add tags in `catalog.json` so agents can find relevant standards by story context.
5. If a topic becomes very large, split it into:
   - core standard
   - examples
   - migration notes
   - checklist

## Recommended Expansion Pattern

For future growth, keep this hierarchy:

- `documents/messaging/`
- `documents/security/`
- `documents/platform/`
- `documents/integration/`
- `documents/data/`
- `documents/frontend/` later if needed
- `documents/testing/` later if needed
- `documents/architecture/` later if needed

This lets the MCP server expose both targeted standards and broader bundles.
