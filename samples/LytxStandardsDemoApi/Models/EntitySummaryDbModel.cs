namespace LytxStandardsDemoApi.Models;

public sealed class EntitySummaryDbModel
{
    public Guid EntityId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset UpdatedAtUtc { get; init; }
}
