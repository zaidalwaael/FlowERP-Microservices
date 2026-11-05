namespace FlowERP.Identity.Domain.RefreshTokens;

public sealed class RefreshToken
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }

    public bool IsActive =>
        RevokedAtUtc is null &&
        ExpiresAtUtc > DateTime.UtcNow;

    private RefreshToken()
    {
    }

    public RefreshToken(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTime expiresAtUtc)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void Revoke()
    {
        if (RevokedAtUtc is not null)
            return;

        RevokedAtUtc = DateTime.UtcNow;
    }
}