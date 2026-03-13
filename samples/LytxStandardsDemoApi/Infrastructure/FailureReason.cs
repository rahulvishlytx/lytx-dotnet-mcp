namespace LytxStandardsDemoApi.Infrastructure;

public enum FailureReason
{
    None = 0,
    NotFound = 1,
    AccessDenied = 2,
    BadRequest = 3,
    FeatureDisabled = 4,
    InternalServerError = 5
}
