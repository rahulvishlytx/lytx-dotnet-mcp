namespace LytxStandardsDemoApi.Infrastructure.FeatureToggles;

public interface IFeatureToggleCollection
{
    bool IsFeatureEnabled(string featureKey);
    bool IsFeatureEnabled(string featureKey, Guid groupId);
}
