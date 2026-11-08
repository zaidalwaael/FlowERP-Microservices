
using FlowERP.Identity.Domain.PasswordResetTokens;

namespace FlowERP.Identity.Application.PasswordResetTokens;

public interface IPasswordResetTokenRepository
{
    Task AddAsync(
        PasswordResetToken token,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PasswordResetToken>> GetActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<PasswordResetToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);
}
