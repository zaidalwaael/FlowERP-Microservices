using FlowERP.Identity.Application.Authorization;
using FlowERP.Identity.Application.Users;
using FlowERP.Identity.Domain.RefreshTokens;

namespace FlowERP.Identity.Application.Authentication;

public sealed class LoginService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenRepository refreshTokenRepository,
    IUserAuthorizationRepository userAuthorizationRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<TokenPairResult> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidCredentialsException();
        }

        var user = await userRepository.GetByEmailAsync(
            email,
            cancellationToken);

        if (user is null)
            throw new InvalidCredentialsException();

        if (!user.IsActive)
            throw new InvalidCredentialsException();

        var passwordIsValid = passwordHasher.Verify(
            user.PasswordHash,
            password);

        if (!passwordIsValid)
            throw new InvalidCredentialsException();

        // Load the user's assigned roles.
        var roles = await userAuthorizationRepository.GetRolesAsync(
            user.Id,
            cancellationToken);

        // Load permissions granted through the user's roles.
        var permissions = await userAuthorizationRepository.GetPermissionsAsync(
            user.Id,
            cancellationToken);

        // Generate an access token containing roles and permissions.
        var accessToken = tokenGenerator.GenerateAccessToken(
            user.Id,
            user.Email,
            roles,
            permissions);

        // Generate a secure refresh token.
        var rawRefreshToken =
            refreshTokenGenerator.Generate();

        var refreshTokenHash =
            refreshTokenGenerator.Hash(rawRefreshToken);

        var refreshToken = new RefreshToken(
            Guid.NewGuid(),
            user.Id,
            refreshTokenHash,
            refreshTokenGenerator.GetExpirationDate());

        await refreshTokenRepository.AddAsync(
            refreshToken,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new TokenPairResult(
            accessToken,
            rawRefreshToken);
    }
}
