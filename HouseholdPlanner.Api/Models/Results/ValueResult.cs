namespace HouseholdPlanner.Api.Models.Results;

public class ValueResult<T>(string? message, bool success, T data, ErrorCode errorCode = ErrorCode.None) : Result(message, success, errorCode)
{
    public T Data { get; set; } = data;
}