
namespace FlowERP.Identity.Domain.PasswordResetTokens;

public sealed class PasswordResetToken
{
    private PasswordResetToken()
    {
        // Required by EF Core.
    }

    public PasswordResetToken(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTime expiresAtUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException(
                "Token ID is required.",
                nameof(id));

        if (userId == Guid.Empty)
            throw new ArgumentException(
                "User ID is required.",
                nameof(userId));

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException(
                "Token hash is required.",
                nameof(tokenHash));

        if (expiresAtUtc <= DateTime.UtcNow)
            throw new ArgumentException(
                "Expiration must be in the future.",
                nameof(expiresAtUtc));

        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAtUtc = DateTime.UtcNow;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime? UsedAtUtc { get; private set; }

    public bool IsValid =>
        UsedAtUtc is null &&
        DateTime.UtcNow < ExpiresAtUtc;

    public void MarkAsUsed()
    {
        if (!IsValid)
            throw new InvalidOperationException(
                "Password reset token is invalid or expired.");

        UsedAtUtc = DateTime.UtcNow;
    }
}
