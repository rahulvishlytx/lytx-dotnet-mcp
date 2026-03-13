# Maintaining the Company Standards MCP Content

## Source of Truth

Use the segmented documents under [mcp/knowledge/standards/documents](documents) as the maintained source of truth.

Use [copilot-instructions.md](../../copilot-instructions.md) as the original seed document and legacy reference.

## Standard Update Workflow

1. Update or add the relevant document in `mcp/knowledge/standards/documents/<category>/`.
2. Update `mcp/knowledge/standards/catalog.json` if:
   - a new document is added
   - a title changes
   - tags or category change
3. Run validation:
   - `npm run mcp:validate`
4. Open a PR with:
   - business reason
   - impacted stacks or teams
   - migration guidance if behavior changed
5. After merge, developers pick up the latest standards automatically from the MCP server.

## Authoring Rules

- Prefer one document per concern.
- Use imperative language for mandatory rules.
- Separate mandatory rules from examples.
- Reference required libraries explicitly.
- Document registration patterns when they are mandatory.
- Include safe defaults and fallback behavior.

## How to Handle Long Lists of Approaches

Do not keep very long lists in a single file.

Instead split them into:

- `*-core.md` for mandatory rules
- `*-examples.md` for common examples
- `*-anti-patterns.md` for prohibited implementations
- `*-migration.md` for upgrades or replacements
- `*-checklist.md` for review and PR checks

Then add each file to `catalog.json` with tags and the same category.

## Change Management Recommendation

For company-wide usage, follow this model:

- PR-based changes only
- code owner approval for standards changes
- semantic version in `catalog.json`
- release notes in the PR description
- optional monthly standards review

## Hosting Recommendation Later

When you move from local to internal hosting:

- keep the same document structure
- keep Git as the source of truth
- expose the same server core through HTTP transport
- add authentication for internal-only access
- add a CI step that runs `npm run mcp:validate`

This keeps local and hosted modes aligned.
