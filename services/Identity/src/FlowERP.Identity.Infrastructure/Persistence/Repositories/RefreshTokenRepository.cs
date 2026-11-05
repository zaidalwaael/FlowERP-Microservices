using FlowERP.Identity.Domain.RefreshTokens;
using Microsoft.EntityFrameworkCore;
using FlowERP.Identity.Application.Authentication;

namespace FlowERP.Identity.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository(
    IdentityDbContext dbContext)
    : IRefreshTokenRepository
{
    public async Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default)
    {
        await dbContext.RefreshTokens.AddAsync(
            refreshToken,
            cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.RefreshTokens
            .SingleOrDefaultAsync(
                refreshToken =>
                    refreshToken.TokenHash == tokenHash,
                cancellationToken);
    }
}