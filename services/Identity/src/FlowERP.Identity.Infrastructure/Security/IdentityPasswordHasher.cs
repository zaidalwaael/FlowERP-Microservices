using FlowERP.Identity.Application.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace FlowERP.Identity.Infrastructure.Security;

public sealed class IdentityPasswordHasher(
    IOptions<PasswordHasherOptions> options) : IPasswordHasher
{
    private static readonly object UserMarker = new();
    private readonly PasswordHasher<object> _hasher = new(options);

    public string Hash(string password)
    {
        return _hasher.HashPassword(UserMarker, password);
    }

    public bool Verify(string passwordHash, string password)
    {
        var result = _hasher.VerifyHashedPassword(
            UserMarker,
            passwordHash,
            password);

        return result != PasswordVerificationResult.Failed;
    }
}