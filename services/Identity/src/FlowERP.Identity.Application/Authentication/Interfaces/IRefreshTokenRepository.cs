using FlowERP.Identity.Domain.RefreshTokens;

namespace FlowERP.Identity.Application.Authentication;

public interface IRefreshTokenRepository
{
    Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);
}