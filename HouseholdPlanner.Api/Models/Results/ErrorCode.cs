namespace HouseholdPlanner.Api.Models.Results;

public enum ErrorCode
{
    None,
    DuplicateEntry,
    NotFound,
    DatabaseError,
    Unauthorized,
    InvalidInput,
    Conflict,
    TooManyRequests
}
