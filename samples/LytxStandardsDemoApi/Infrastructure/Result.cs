namespace LytxStandardsDemoApi.Infrastructure;

public sealed class Result<T>
{
    private Result(bool isSuccess, T? value, FailureReason failureReason, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        FailureReason = failureReason;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }
    public T? Value { get; }
    public FailureReason FailureReason { get; }
    public string? ErrorMessage { get; }

    public static Result<T> SuccessWith(T value) => new(true, value, FailureReason.None, null);

    public static Result<T> FailWith(FailureReason failureReason, string? errorMessage = null) =>
        new(false, default, failureReason, errorMessage);
}
