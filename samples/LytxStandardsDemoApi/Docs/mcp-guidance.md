# MCP Guidance Used For This Demo API

The project was designed from the company standards MCP server using this story:

> Add a new .NET API endpoint to return an entity summary by id. The endpoint must be authenticated, use feature toggles, log context, read from PostgreSQL, and return Result<T>.

## MCP-derived standards applied

- Authentication Standards
- Feature Toggle Standards
- Logging Standards
- Database Access Standards
- Result Pattern Standards

## How the demo reflects them

- controller uses `[Authorize]`
- user context is read through claims principal extensions
- service returns `Result<T>`
- data access uses PostgreSQL-style repository abstractions and `ConnectionTarget.ReadInstance`
- logging uses `ITransactionLogger` custom parameters
- endpoint behavior is gated by `FeatureToggleKeys.EnableEntitySummaryEndpoint`
- controller response mapping is centralized in `HandleResult()`
