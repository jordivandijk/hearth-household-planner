using HouseholdPlanner.Api.Models.Entities;
using HouseholdPlanner.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Household> Households => Set<Household>();
    public DbSet<HouseholdTask> Tasks => Set<HouseholdTask>();
    public DbSet<TaskCompletion> TaskCompletions => Set<TaskCompletion>();
    public DbSet<GroceryItem> GroceryItems => Set<GroceryItem>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Household>(household =>
        {
            household.HasIndex(h => h.InviteCode).IsUnique();
            household.Property(h => h.CreatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<User>(user =>
        {
            user.HasIndex(u => u.Username).IsUnique();
            user.Property(u => u.CreatedAt).HasDefaultValueSql("now()");
            user.Property(u => u.Language).HasMaxLength(5);
            user.HasOne(u => u.Household).WithMany(h => h.Members).HasForeignKey(u => u.HouseholdId);
        });

        modelBuilder.Entity<HouseholdTask>(task =>
        {
            task.HasIndex(t => new { t.HouseholdId, t.Done });
            task.Property(t => t.CreatedAt).HasDefaultValueSql("now()");
            task.HasOne(t => t.Household).WithMany().HasForeignKey(t => t.HouseholdId);
            task.HasOne(t => t.Assignee).WithMany().HasForeignKey(t => t.AssigneeId).OnDelete(DeleteBehavior.SetNull);
            task.HasOne(t => t.DoneBy).WithMany().HasForeignKey(t => t.DoneById).OnDelete(DeleteBehavior.SetNull);
        });

        // Completions outlive their task, so the weekly stats don't drop when a task is deleted.
        modelBuilder.Entity<TaskCompletion>(completion =>
        {
            completion.HasIndex(c => new { c.HouseholdId, c.On });
            completion.Property(c => c.CreatedAt).HasDefaultValueSql("now()");
            completion.HasOne(c => c.Task).WithMany().HasForeignKey(c => c.TaskId).OnDelete(DeleteBehavior.SetNull);
            completion.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId);
        });

        modelBuilder.Entity<GroceryItem>(item =>
        {
            item.HasIndex(g => new { g.HouseholdId, g.ClearedAt });
            item.Property(g => g.Category).HasConversion<string>().HasMaxLength(20);
            item.Property(g => g.CreatedAt).HasDefaultValueSql("now()");
            item.HasOne(g => g.Household).WithMany().HasForeignKey(g => g.HouseholdId);
        });

        // Deleting the admin deletes their keys.
        modelBuilder.Entity<ApiKey>(key =>
        {
            key.HasIndex(k => k.Hash).IsUnique();
            key.HasIndex(k => k.UserId);
            key.Property(k => k.Name).HasMaxLength(50);
            key.Property(k => k.Prefix).HasMaxLength(ApiKeySecrets.PrefixLength);
            key.Property(k => k.Hash).HasMaxLength(64);
            key.Property(k => k.CreatedAt).HasDefaultValueSql("now()");
            key.HasOne(k => k.User).WithMany().HasForeignKey(k => k.UserId);
        });
    }
}
