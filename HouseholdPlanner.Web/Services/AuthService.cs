using HouseholdPlanner.Web.ApiClients.Interfaces;
using HouseholdPlanner.Web.Auth;
using HouseholdPlanner.Web.Models;

namespace HouseholdPlanner.Web.Services;

public class AuthService(IAuthApiClient authApiClient, HouseholdAuthStateProvider authStateProvider, HomeStore store) : IAuthService
{
    public async Task<bool> LoginAsync(string username, string password)
    {
        var response = await authApiClient.LoginAsync(new LoginRequest { Username = username, Password = password });
        if (response is null)
            return false;

        await authStateProvider.NotifyLoginAsync(response.Token);
        return true;
    }

    public async Task RegisterAsync(string inviteCode, string username, string password)
    {
        var response = await authApiClient.RegisterAsync(new RegisterRequest
        {
            InviteCode = inviteCode,
            Username   = username,
            Password   = password
        });
        await authStateProvider.NotifyLoginAsync(response.Token);
    }

    public async Task ChangePasswordAsync(string currentPassword, string newPassword)
    {
        var response = await authApiClient.ChangePasswordAsync(new ChangePasswordRequest
        {
            CurrentPassword = currentPassword,
            NewPassword     = newPassword
        });
        await authStateProvider.NotifyLoginAsync(response.Token);
    }

    public async Task LogoutAsync()
    {
        await authStateProvider.NotifyLogoutAsync();
        store.Clear();
    }
}
