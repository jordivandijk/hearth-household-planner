using HouseholdPlanner.Api.Data;
using HouseholdPlanner.Api.Models.Entities;
using HouseholdPlanner.Api.Models.Results;
using HouseholdPlanner.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HouseholdPlanner.Api.Repositories.Implementations;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<EntityResult<User>> GetByIdAsync(Guid id)
    {
        var user = await context.Users.FindAsync(id);
        return user is null
            ? new(null, false, "User not found.", ErrorCode.NotFound)
            : new(user, true, "User found.");
    }

    // Ignores case, since phones capitalise the first letter of a username field.
    public async Task<EntityResult<User>> GetByUsernameAsync(string username)
    {
        var lower = username.ToLower();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == lower);
        return user is null
            ? new(null, false, "User not found.", ErrorCode.NotFound)
            : new(user, true, "User found.");
    }

    public Task<List<User>> GetByHouseholdAsync(Guid householdId)
        => context.Users.Where(u => u.HouseholdId == householdId).OrderBy(u => u.CreatedAt).ToListAsync();

    public async Task<bool> ExistsAsync(string username, Guid? exceptUserId = null)
    {
        var lower = username.ToLower();
        return await context.Users.AnyAsync(u => u.Username.ToLower() == lower && u.Id != exceptUserId);
    }

    public async Task<EntityResult<User>> CreateAsync(User user)
    {
        try
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return new(user, true, "User created successfully.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
        {
            return new(null, false, "A user with this username already exists.", ErrorCode.DuplicateEntry);
        }
    }

    public async Task<Result> UpdateAsync(User user)
    {
        await context.SaveChangesAsync();
        return new("User updated successfully.", true);
    }

    public async Task DeleteAsync(User user)
    {
        context.Users.Remove(user);
        await context.SaveChangesAsync();
    }
}
