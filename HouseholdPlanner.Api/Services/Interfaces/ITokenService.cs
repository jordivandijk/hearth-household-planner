using HouseholdPlanner.Api.Models.Entities;

namespace HouseholdPlanner.Api.Services.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
