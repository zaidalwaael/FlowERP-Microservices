
namespace FlowERP.Identity.Domain.Users;

public sealed class User
{
    private User()
    {
        // Required by EF Core when loading a user from the database.
    }

    public User(Guid id, string email, string passwordHash)
    {
        if (id == Guid.Empty)
            throw new ArgumentException(
                "User ID is required.",
                nameof(id));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(
                "Email is required.",
                nameof(email));

        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (normalizedEmail.Length > 256)
            throw new ArgumentException(
                "Email cannot exceed 256 characters.",
                nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException(
                "Password hash is required.",
                nameof(passwordHash));

        Id = id;
        Email = normalizedEmail;
        PasswordHash = passwordHash;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public void ChangeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) ||
            email.Trim().Length > 256)
        {
            throw new ArgumentException(
                "Valid email is required.",
                nameof(email));
        }

        Email = email.Trim().ToLowerInvariant();
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
        {
            throw new ArgumentException(
                "Password hash is required.",
                nameof(newPasswordHash));
        }

        PasswordHash = newPasswordHash;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
