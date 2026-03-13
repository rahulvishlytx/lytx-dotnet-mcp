namespace LytxStandardsDemoApi.Models;

public sealed class EntitySummary
{
    public Guid EntityId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public DateTimeOffset UpdatedAtUtc { get; init; }
    public bool IsLegacyFallback { get; init; }
}
