using HouseholdPlanner.Api.Models.Entities;

namespace HouseholdPlanner.Api.Models.Results;

public class EntityResult<T>(T? data, bool success, string? message, ErrorCode errorCode = ErrorCode.None)
    : Result(message, success, errorCode)
    where T : IEntity
{
    public T? Data { get; set; } = data;
}