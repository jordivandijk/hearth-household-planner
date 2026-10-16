using HouseholdPlanner.Api.Models.Entities;
using HouseholdPlanner.Api.Models.Results;

namespace HouseholdPlanner.Api.Repositories.Interfaces;

public interface IUserRepository
{
    Task<EntityResult<User>> GetByIdAsync(Guid id);
    /// <summary>The user with this username, ignoring case.</summary>
    Task<EntityResult<User>> GetByUsernameAsync(string username);
    Task<List<User>> GetByHouseholdAsync(Guid householdId);
    /// <summary>Whether another user has this username, ignoring case, so "Bob" and "bob" can't both exist.</summary>
    Task<bool> ExistsAsync(string username, Guid? exceptUserId = null);
    Task<EntityResult<User>> CreateAsync(User user);
    Task<Result> UpdateAsync(User user);
    /// <summary>Deletes the user and their completions; tasks they had or did become anyone's.</summary>
    Task DeleteAsync(User user);
}
