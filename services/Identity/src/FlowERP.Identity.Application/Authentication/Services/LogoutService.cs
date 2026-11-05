namespace FlowERP.Identity.Application.Authentication;

public sealed class LogoutService(
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork)
{
    public async Task LogoutAsync(
        string rawRefreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawRefreshToken))
            throw new InvalidRefreshTokenException();

        var refreshTokenHash =
            refreshTokenGenerator.Hash(rawRefreshToken);

        var refreshToken =
            await refreshTokenRepository.GetByTokenHashAsync(
                refreshTokenHash,
                cancellationToken);

        if (refreshToken is null ||
            !refreshToken.IsActive)
        {
            throw new InvalidRefreshTokenException();
        }

        refreshToken.Revoke();

        await unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}