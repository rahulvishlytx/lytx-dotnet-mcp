# Lytx Standards Demo API

This is a real sample ASP.NET Core API project created from the company standards MCP guidance.

## Implemented feature

- `GET /entities/{id}/summary`

## Lytx-style standards demonstrated

- `[Authorize]` on the controller
- user context extracted from `User.GetUniqueId()`, `User.GetRootGroupId()`, `User.GetCompany()`
- feature toggle in `Infrastructure/FeatureToggleKeys.cs`
- `ITransactionLogger` with custom parameters
- PostgreSQL-style data access with `ConnectionTarget.ReadInstance`
- service methods returning `Result<T>`
- response mapping through `HandleResult()`

## Run

- `dotnet run --project mcp/samples/LytxStandardsDemoApi/LytxStandardsDemoApi.csproj`

The API listens on:

- `http://localhost:5085`

## Test request

Use the sample request in [LytxStandardsDemoApi.http](LytxStandardsDemoApi.http) or call:

- `GET http://localhost:5085/entities/44444444-4444-4444-4444-444444444444/summary`

Optional headers:

- `x-demo-user-id`
- `x-demo-root-group-id`
- `x-demo-company-id`

Defaults are provided by the demo auth handler so the endpoint is easy to test locally.

## Files to review

- [Program.cs](Program.cs)
- [Controllers/EntitiesController.cs](Controllers/EntitiesController.cs)
- [Services/EntitySummaryService.cs](Services/EntitySummaryService.cs)
- [Data/PostgreSqlEntityDataAccess.cs](Data/PostgreSqlEntityDataAccess.cs)
- [Infrastructure/Result.cs](Infrastructure/Result.cs)
- [Docs/mcp-guidance.md](Docs/mcp-guidance.md)
