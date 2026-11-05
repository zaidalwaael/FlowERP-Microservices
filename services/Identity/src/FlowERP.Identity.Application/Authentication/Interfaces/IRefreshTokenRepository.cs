using FlowERP.Identity.Domain.RefreshTokens;

namespace FlowERP.Identity.Application.Authentication;

public interface IRefreshTokenRepository
{
    Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);
}