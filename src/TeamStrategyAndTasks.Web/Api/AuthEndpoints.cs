using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using TeamStrategyAndTasks.Infrastructure.Identity;

namespace TeamStrategyAndTasks.Web.Api;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapApiAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/token", async (
            TokenRequest req,
            UserManager<ApplicationUser> userManager,
            IConfiguration config) =>
        {
            var user = await userManager.FindByEmailAsync(req.Email);
            if (user is null || !await userManager.CheckPasswordAsync(user, req.Password))
                return Results.Unauthorized();

            var token = GenerateToken(user, config);
            return Results.Ok(token);
        })
        .WithSummary("Exchange credentials for a JWT bearer token")
        .Produces<TokenResponse>()
        .Produces(401);

        return app;
    }

    private static TokenResponse GenerateToken(ApplicationUser user, IConfiguration config)
    {
        var keyBytes = Encoding.UTF8.GetBytes(
            config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured"));
        var signingKey = new SymmetricSecurityKey(keyBytes);
        var expiryHours = config.GetValue<int>("Jwt:ExpiryHours", 8);
        var expiresAt = DateTime.UtcNow.AddHours(expiryHours);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, user.DisplayName ?? user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"] ?? "TeamStrategyAndTasks",
            audience: config["Jwt:Audience"] ?? "TeamStrategyAndTasks.Api",
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

        return new TokenResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt,
            user.DisplayName ?? user.Email ?? string.Empty,
            user.Email ?? string.Empty);
    }
}
