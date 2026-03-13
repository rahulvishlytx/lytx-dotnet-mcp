using LytxStandardsDemoApi.Data;
using LytxStandardsDemoApi.Infrastructure;
using LytxStandardsDemoApi.Infrastructure.FeatureToggles;
using LytxStandardsDemoApi.Infrastructure.Logging;
using LytxStandardsDemoApi.Models;

namespace LytxStandardsDemoApi.Services;

public sealed class EntitySummaryService : IEntitySummaryService
{
    private readonly IEntityDataAccess _dataAccess;
    private readonly IFeatureToggleCollection _featureToggleCollection;
    private readonly ITransactionLogger _logger;

    public EntitySummaryService(
        IEntityDataAccess dataAccess,
        IFeatureToggleCollection featureToggleCollection,
        ITransactionLogger logger)
    {
        _dataAccess = dataAccess;
        _featureToggleCollection = featureToggleCollection;
        _logger = logger;
    }

    public async Task<Result<EntitySummary>> GetEntitySummaryAsync(Guid entityId, Guid actorId, Guid rootGroupId, int companyId)
    {
        _logger.AddCustomParameter(CustomParameters.EntityId, entityId);
        _logger.AddCustomParameter(CustomParameters.ActorId, actorId);
        _logger.AddCustomParameter(CustomParameters.RootGroupId, rootGroupId);
        _logger.AddCustomParameter(CustomParameters.CompanyId, companyId);
        _logger.AddCustomParameter(CustomParameters.FeatureToggle, FeatureToggleKeys.EnableEntitySummaryEndpoint);

        try
        {
            if (!IsFeatureEnabled(FeatureToggleKeys.EnableEntitySummaryEndpoint, rootGroupId))
            {
                _logger.LogInformation("Feature toggle disabled. Returning legacy entity summary.");
                return Result<EntitySummary>.SuccessWith(new EntitySummary
                {
                    EntityId = entityId,
                    Name = "Legacy summary placeholder",
                    Status = "Unavailable",
                    Source = "LegacyFallback",
                    UpdatedAtUtc = DateTimeOffset.UtcNow,
                    IsLegacyFallback = true
                });
            }

            var entity = await _dataAccess.GetEntityById(entityId);
            if (entity is null)
            {
                return Result<EntitySummary>.FailWith(FailureReason.NotFound, "Entity summary was not found.");
            }

            _logger.LogInformation("Entity summary retrieved successfully.");

            return Result<EntitySummary>.SuccessWith(new EntitySummary
            {
                EntityId = entity.EntityId,
                Name = entity.Name,
                Status = entity.Status,
                Source = "PostgreSqlReadModel",
                UpdatedAtUtc = entity.UpdatedAtUtc,
                IsLegacyFallback = false
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving entity summary.", ex);
            return Result<EntitySummary>.FailWith(FailureReason.InternalServerError, "Unexpected error while retrieving the entity summary.");
        }
    }

    private bool IsFeatureEnabled(string featureKey, Guid? groupId = null)
    {
        return groupId.HasValue
            ? _featureToggleCollection.IsFeatureEnabled(featureKey, groupId.Value)
            : _featureToggleCollection.IsFeatureEnabled(featureKey);
    }
}
