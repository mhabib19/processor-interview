using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CardProcessor.Infrastructure.Services;

public interface IJwtService
{
    string GenerateToken(string username, string role);
    ClaimsPrincipal? ValidateToken(string token);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private record JwtSettings(string SecretKey, string Issuer, string Audience, string ExpirationHours);

    private JwtSettings GetJwtSettings()
    {
        return new JwtSettings(
            SecretKey: _configuration["JwtSettings:SecretKey"] 
                ?? throw new InvalidOperationException("JWT SecretKey is not configured. Please add 'JwtSettings:SecretKey' to your configuration."),
            Issuer: _configuration["JwtSettings:Issuer"] 
                ?? throw new InvalidOperationException("JWT Issuer is not configured. Please add 'JwtSettings:Issuer' to your configuration."),
            Audience: _configuration["JwtSettings:Audience"] 
                ?? throw new InvalidOperationException("JWT Audience is not configured. Please add 'JwtSettings:Audience' to your configuration."),
            ExpirationHours: _configuration["JwtSettings:ExpirationHours"] 
                ?? throw new InvalidOperationException("JWT ExpirationHours is not configured. Please add 'JwtSettings:ExpirationHours' to your configuration.")
        );
    }

    public string GenerateToken(string username, string role)
    {
        var settings = GetJwtSettings();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(Convert.ToDouble(settings.ExpirationHours)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var settings = GetJwtSettings();
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(settings.SecretKey);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = settings.Issuer,
                ValidateAudience = true,
                ValidAudience = settings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}
