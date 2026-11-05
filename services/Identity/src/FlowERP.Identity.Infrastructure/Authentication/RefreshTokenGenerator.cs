using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using FlowERP.Identity.Application.Authentication;
namespace FlowERP.Identity.Infrastructure.Authentication;

public sealed class RefreshTokenGenerator(
    IOptions<JwtOptions> options)
    : IRefreshTokenGenerator
{
    private readonly JwtOptions _options = options.Value;

    public string Generate()
    {
        var randomBytes =
            RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(randomBytes);
    }

    public string Hash(string refreshToken)
    {
        var tokenBytes =
            Encoding.UTF8.GetBytes(refreshToken);

        var hashBytes =
            SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashBytes);
    }

    public DateTime GetExpirationDate()
    {
        return DateTime.UtcNow.AddDays(
            _options.RefreshTokenExpirationDays);
    }
}