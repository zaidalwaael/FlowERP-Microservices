using FlowERP.Identity.Application.Users;
using Microsoft.EntityFrameworkCore;

namespace FlowERP.Identity.Application.PasswordResetTokens;

public sealed class ResetPasswordService(
    IPasswordResetTokenRepository tokenRepository,
    IPasswordResetTokenGenerator tokenGenerator,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
{
    public async Task ResetAsync(
        string rawToken,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
            throw new InvalidPasswordResetTokenException();

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 12)
            throw new ArgumentException(
                "Password must contain at least 12 characters.",
                nameof(newPassword));

        var tokenHash = tokenGenerator.Hash(rawToken);

        // Load with tracking: EF Core needs to persist token consumption.
        var token = await tokenRepository.GetByTokenHashAsync(
            tokenHash, cancellationToken);

        if (token is null || !token.IsValid)
            throw new InvalidPasswordResetTokenException();

        var user = await userRepository.GetByIdAsync(
            token.UserId, cancellationToken);

        if (user is null || !user.IsActive)
            throw new InvalidPasswordResetTokenException();

        // Password hashes are created by the dedicated password hasher.
        user.ChangePassword(passwordHasher.Hash(newPassword));
        token.MarkAsUsed();

        // Invalidate every other pending reset link for this account.
        // All changes are saved atomically by EF Core's transaction.
        var otherTokens = await tokenRepository.GetActiveByUserIdAsync(
            user.Id, cancellationToken);
        foreach (var otherToken in otherTokens)
        {
            if (otherToken.Id != token.Id)
                otherToken.MarkAsUsed();
        }

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Concurrent attempts to consume the same token must not succeed.
            throw new InvalidPasswordResetTokenException();
        }
    }
}
