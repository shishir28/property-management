using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PropertyManagement.API.Auth;

namespace PropertyManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController(IOptions<JwtOptions> jwtOptions, IOptions<DevAuthOptions> devAuthOptions) : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var devUser = devAuthOptions.Value;
        if (!string.Equals(request.Username, devUser.Username, StringComparison.OrdinalIgnoreCase) ||
            request.Password != devUser.Password)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        var jwt = jwtOptions.Value;
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(jwt.ExpirationMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, devUser.Username),
            new Claim(JwtRegisteredClaimNames.UniqueName, devUser.Username),
            new Claim(JwtRegisteredClaimNames.Email, devUser.Username),
            new Claim("name", devUser.DisplayName),
            new Claim(ClaimTypes.Name, devUser.DisplayName),
            new Claim(ClaimTypes.Role, devUser.Role)
        };

        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return Ok(new LoginResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAtUtc,
            devUser.DisplayName,
            devUser.Username,
            devUser.Role));
    }
}

public sealed record LoginRequest(string Username, string Password);

public sealed record LoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string DisplayName,
    string Username,
    string Role);
