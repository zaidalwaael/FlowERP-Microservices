
using FlowERP.Identity.Application.PasswordResetTokens;
using FlowERP.Identity.Domain.PasswordResetTokens;
using Microsoft.EntityFrameworkCore;

namespace FlowERP.Identity.Infrastructure.Persistence.Repositories;

public sealed class PasswordResetTokenRepository(
    IdentityDbContext dbContext)
    : IPasswordResetTokenRepository
{
    public async Task AddAsync(
        PasswordResetToken token,
        CancellationToken cancellationToken = default)
    {
        await dbContext.PasswordResetTokens.AddAsync(
            token,
            cancellationToken);
    }

    public async Task<IReadOnlyList<PasswordResetToken>> GetActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.PasswordResetTokens
            .Where(token => token.UserId == userId &&
                            token.UsedAtUtc == null &&
                            token.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public Task<PasswordResetToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return dbContext.PasswordResetTokens
            .FirstOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                cancellationToken);
    }
}
