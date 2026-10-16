using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HouseholdPlanner.Api.Models.Entities;
using HouseholdPlanner.Api.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace HouseholdPlanner.Api.Services.Implementations;

public class TokenService(IConfiguration configuration) : ITokenService
{
    public const string HouseholdClaim = "household";
    /// <summary>The account's <see cref="User.SecurityStamp"/> when the token was made.</summary>
    public const string SecurityStampClaim = "stamp";

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(HouseholdClaim, user.HouseholdId.ToString()),
            new Claim(SecurityStampClaim, user.SecurityStamp.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(int.Parse(configuration["Jwt:ExpiryInDays"]!)),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
