namespace HouseholdPlanner.Web.Services;

public interface IAuthService
{
    /// <summary>False when the username or password is wrong.</summary>
    Task<bool> LoginAsync(string username, string password);
    /// <summary>Creates the account in the household of the invite code and signs the new user in.</summary>
    Task RegisterAsync(string inviteCode, string username, string password);
    /// <summary>Signs the account out on every other device, and keeps this one signed in with a new token.</summary>
    Task ChangePasswordAsync(string currentPassword, string newPassword);
    Task LogoutAsync();
}
