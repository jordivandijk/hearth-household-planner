namespace HouseholdPlanner.Api.Models.Results;

public class Result(string? message, bool success, ErrorCode errorCode = ErrorCode.None)
{
    public bool Success { get; set; } = success;
    public string? Message { get; set; } = message;
    public ErrorCode ErrorCode { get; set; } = errorCode;
}