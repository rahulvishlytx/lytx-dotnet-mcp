# Feature Toggle Standards

## Required Libraries

- Use `Tina.FeatureToggle.NetCore` (`700.*`) for all feature toggles.

## Mandatory Rules

All new features must:

- Be behind a feature toggle.
- Define toggle keys in `Infrastructure/FeatureToggleKeys.cs`.
- Check the feature toggle before executing new logic.
- Provide fallback to existing behavior.

## Example

```csharp
public static class FeatureToggleKeys
{
    public const string EnableNewFeature = "enable-new-feature";
}

private readonly IFeatureToggleCollection _featureToggleCollection;

private bool IsFeatureEnabled(string featureKey, Guid? groupId = null)
{
    return groupId.HasValue
        ? _featureToggleCollection.IsFeatureEnabled(featureKey, groupId.Value)
        : _featureToggleCollection.IsFeatureEnabled(featureKey);
}

if (IsFeatureEnabled(FeatureToggleKeys.EnableNewFeature, rootGroupId))
{
    // New feature implementation
}
else
{
    // Legacy implementation
}
```

## Registration

Register feature toggles in `Bootstrapper.cs` and `Program.cs`:

```csharp
services.AddSingleton(GetFeatureToggleCollection);
builder.Configuration.AddJsonFile("apptoggles.json", optional: false, reloadOnChange: true);
builder.Services.AddFeatureManagement();
```

## Review Checklist

- Feature key added to central constants
- New behavior gated
- Legacy fallback retained
- Toggle config loaded in startup
