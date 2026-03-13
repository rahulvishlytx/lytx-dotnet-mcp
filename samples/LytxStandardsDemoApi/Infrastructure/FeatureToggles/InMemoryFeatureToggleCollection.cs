namespace LytxStandardsDemoApi.Infrastructure.FeatureToggles;

public sealed class InMemoryFeatureToggleCollection : IFeatureToggleCollection
{
    private readonly Dictionary<string, bool> _globalToggles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["enable-entity-summary-endpoint"] = true
    };

    private readonly Dictionary<Guid, HashSet<string>> _groupEnabledToggles = new()
    {
        [Guid.Parse("11111111-1111-1111-1111-111111111111")] =
        [
            "enable-entity-summary-endpoint"
        ]
    };

    public bool IsFeatureEnabled(string featureKey) =>
        _globalToggles.TryGetValue(featureKey, out var isEnabled) && isEnabled;

    public bool IsFeatureEnabled(string featureKey, Guid groupId) =>
        _groupEnabledToggles.TryGetValue(groupId, out var toggles) && toggles.Contains(featureKey);
}
