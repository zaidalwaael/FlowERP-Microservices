using FlowERP.Identity.Application.Authorization;
using FlowERP.Identity.Application.Users;
using FlowERP.Identity.Domain.RefreshTokens;

namespace FlowERP.Identity.Application.Authentication;

public sealed class RefreshTokenService(
    IUserRepository userRepository,
    ITokenGenerator tokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenRepository refreshTokenRepository,
    IUserAuthorizationRepository userAuthorizationRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<TokenPairResult> RefreshAsync(
        string rawRefreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawRefreshToken))
            throw new InvalidRefreshTokenException();

        // Hash the provided refresh token before database lookup.
        var refreshTokenHash =
            refreshTokenGenerator.Hash(rawRefreshToken);

        // Find the stored refresh token.
        var existingRefreshToken =
            await refreshTokenRepository.GetByTokenHashAsync(
                refreshTokenHash,
                cancellationToken);

        if (existingRefreshToken is null ||
            !existingRefreshToken.IsActive)
        {
            throw new InvalidRefreshTokenException();
        }

        // Ensure the token belongs to an active user.
        var user = await userRepository.GetByIdAsync(
            existingRefreshToken.UserId,
            cancellationToken);

        if (user is null || !user.IsActive)
            throw new InvalidRefreshTokenException();

        // Load the user's current roles.
        var roles = await userAuthorizationRepository.GetRolesAsync(
            user.Id,
            cancellationToken);

        // Load permissions granted through the user's current roles.
        var permissions = await userAuthorizationRepository.GetPermissionsAsync(
            user.Id,
            cancellationToken);

        // Revoke the old refresh token to prevent reuse.
        existingRefreshToken.Revoke();

        // Generate a new access token with updated authorization claims.
        var accessToken = tokenGenerator.GenerateAccessToken(
            user.Id,
            user.Email,
            roles,
            permissions);

        // Generate a new secure refresh token.
        var newRawRefreshToken =
            refreshTokenGenerator.Generate();

        var newRefreshTokenHash =
            refreshTokenGenerator.Hash(newRawRefreshToken);

        var newRefreshToken = new RefreshToken(
            Guid.NewGuid(),
            user.Id,
            newRefreshTokenHash,
            refreshTokenGenerator.GetExpirationDate());

        await refreshTokenRepository.AddAsync(
            newRefreshToken,
            cancellationToken);

        // Persist both the revocation and the new refresh token.
        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new TokenPairResult(
            accessToken,
            newRawRefreshToken);
    }
}
