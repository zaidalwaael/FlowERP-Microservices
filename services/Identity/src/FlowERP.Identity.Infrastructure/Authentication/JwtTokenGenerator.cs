
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FlowERP.Identity.Application.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FlowERP.Identity.Infrastructure.Authentication;

public sealed class JwtTokenGenerator(
    IOptions<JwtOptions> options) : ITokenGenerator
{
    private readonly JwtOptions _options = options.Value;

    public string GenerateAccessToken(
        Guid userId,
        string email,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions)
    {
        // Add the standard JWT claims.
        var claims = new List<Claim>
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                userId.ToString()),

            new Claim(
                JwtRegisteredClaimNames.Email,
                email),

            new Claim(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

        // Add a role claim for each assigned role.
        foreach (var role in roles.Distinct())
        {
            claims.Add(new Claim(
                ClaimTypes.Role,
                role));
        }

        // Add a permission claim for each granted permission.
        foreach (var permission in permissions.Distinct())
        {
            claims.Add(new Claim(
                "permission",
                permission));
        }

        // Decode the Base64-encoded JWT signing key.
        var securityKey = new SymmetricSecurityKey(
            Convert.FromBase64String(_options.SecretKey));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        // Generate the signed JWT access token.
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                _options.AccessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}
