using HouseholdPlanner.Api.Models.Entities;
using HouseholdPlanner.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IConfiguration config, ILogger logger)
    {
        var username = config["AdminSeed:Username"];
        var password = config["AdminSeed:Password"];

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            // Without an admin there's no household, and so no invite code for anyone to sign up with.
            if (!await db.Users.AnyAsync(u => u.Role == UserRole.Admin))
                logger.LogWarning("There's no admin account yet. Set AdminSeed:Username and AdminSeed:Password (ADMIN_SEED_USERNAME and ADMIN_SEED_PASSWORD in .env) and restart to create it.");
            return;
        }

        // Only to bootstrap the first admin and their household. Checking the name alone would bring the
        // seed account back (with the seed password) on every start after it was renamed or deleted.
        var lower = username.ToLower();
        if (await db.Users.AnyAsync(u => u.Role == UserRole.Admin || u.Username.ToLower() == lower))
            return;

        var household = new Household
        {
            Id         = Guid.NewGuid(),
            Name       = config["AdminSeed:HouseholdName"] is { Length: > 0 } name ? name : "Home",
            InviteCode = InviteCodes.Generate()
        };

        db.Households.Add(household);
        db.Users.Add(new User
        {
            Id           = Guid.NewGuid(),
            Username     = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role         = UserRole.Admin,
            Hue          = MemberHues.Next([]),
            HouseholdId  = household.Id
        });

        await db.SaveChangesAsync();
    }
}
