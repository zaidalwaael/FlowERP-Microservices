
using System.Security.Cryptography;
using FlowERP.Identity.Application.PasswordResetTokens;

namespace FlowERP.Identity.Infrastructure.Authentication;

public sealed class PasswordResetTokenGenerator
    : IPasswordResetTokenGenerator
{
    public string Generate()
    {
        // Generate 32 cryptographically secure random bytes.
        var bytes = RandomNumberGenerator.GetBytes(32);

        // Convert the bytes into a URL-safe token.
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public string Hash(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException(
                "Token is required.",
                nameof(token));

        // Calculate the SHA-256 hash of the token.
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);

        var hash = SHA256.HashData(bytes);

        // Return the hash as a 64-character hexadecimal string.
        return Convert.ToHexString(hash);
    }

    public DateTime GetExpirationDate()
    {
        // Password reset tokens expire after 30 minutes.
        return DateTime.UtcNow.AddMinutes(30);
    }
}
