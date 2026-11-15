using FlowERP.Identity.Application;
using FlowERP.Identity.Application.Authentication;
using FlowERP.Identity.Application.Authorization;
using FlowERP.Identity.Application.Users;
using FlowERP.Identity.Domain.RefreshTokens;
using FlowERP.Identity.Domain.Users;
using Moq;
using Xunit;

namespace FlowERP.Identity.UnitTests;

public sealed class RefreshTokenServiceTests
{
    private readonly Mock<IUserRepository> users = new();
    private readonly Mock<ITokenGenerator> accessTokens = new();
    private readonly Mock<IRefreshTokenGenerator> tokens = new();
    private readonly Mock<IRefreshTokenRepository> sessions = new();
    private readonly Mock<IUserAuthorizationRepository> authorization = new();
    private readonly Mock<IUnitOfWork> unitOfWork = new();

    // Tell xUnit to discover this method as a test.
    [Fact]
    public async Task RefreshAsync_WhenTokenWasRevoked_RejectsItWithoutSaving()
    {
        var session = new RefreshToken(Guid.NewGuid(), Guid.NewGuid(), "old-hash", DateTime.UtcNow.AddDays(1));
        session.Revoke();
        tokens.Setup(generator => generator.Hash("old-token")).Returns(session.TokenHash);
        sessions.Setup(repository => repository.GetByTokenHashAsync(session.TokenHash, default))
            .ReturnsAsync(session);

        await Assert.ThrowsAsync<InvalidRefreshTokenException>(() => CreateService().RefreshAsync("old-token"));

        users.VerifyNoOtherCalls();
        unitOfWork.VerifyNoOtherCalls();
    }

    // Tell xUnit to discover this method as a test.
    [Fact]
    public async Task RefreshAsync_WhenTokenIsActive_RotatesSessionAndUsesCurrentPermissions()
    {
        var user = new User(Guid.NewGuid(), "user@flowerp.com", "password-hash");
        var session = new RefreshToken(Guid.NewGuid(), user.Id, "old-hash", DateTime.UtcNow.AddDays(1));
        var expiration = DateTime.UtcNow.AddDays(7);
        string[] roles = ["Manager"];
        string[] permissions = ["users.read"];

        // Configure the returned user and the authorization data used for the new token.
        tokens.Setup(generator => generator.Hash("old-token")).Returns(session.TokenHash);
        sessions.Setup(repository => repository.GetByTokenHashAsync(session.TokenHash, default))
            .ReturnsAsync(session);
        users.Setup(repository => repository.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        authorization.Setup(repository => repository.GetRolesAsync(user.Id, default)).ReturnsAsync(roles);
        authorization.Setup(repository => repository.GetPermissionsAsync(user.Id, default)).ReturnsAsync(permissions);
        accessTokens.Setup(generator => generator.GenerateAccessToken(user.Id, user.Email, roles, permissions))
            .Returns("new-access-token");
        tokens.Setup(generator => generator.Generate()).Returns("new-refresh-token");
        tokens.Setup(generator => generator.Hash("new-refresh-token")).Returns("new-hash");
        tokens.Setup(generator => generator.GetExpirationDate()).Returns(expiration);

        var result = await CreateService().RefreshAsync("old-token");

        // Confirm that old session revocation and the replacement are persisted together.
        Assert.NotNull(session.RevokedAtUtc);
        Assert.Equal("new-access-token", result.AccessToken);
        Assert.Equal("new-refresh-token", result.RefreshToken);
        sessions.Verify(repository => repository.AddAsync(
            It.Is<RefreshToken>(token => token.Id != session.Id && token.UserId == user.Id &&
                token.TokenHash == "new-hash" && token.ExpiresAtUtc == expiration),
            default), Times.Once());
        unitOfWork.Verify(work => work.SaveChangesAsync(default), Times.Once());
    }

    // Tell xUnit to discover this method as a test.
    [Fact]
    public async Task LogoutAsync_WhenTokenIsActive_RevokesAndSavesTheSession()
    {
        var session = new RefreshToken(Guid.NewGuid(), Guid.NewGuid(), "session-hash", DateTime.UtcNow.AddDays(1));
        tokens.Setup(generator => generator.Hash("raw-token")).Returns(session.TokenHash);
        sessions.Setup(repository => repository.GetByTokenHashAsync(session.TokenHash, default))
            .ReturnsAsync(session);

        var service = new LogoutService(tokens.Object, sessions.Object, unitOfWork.Object);
        await service.LogoutAsync("raw-token");

        Assert.NotNull(session.RevokedAtUtc);
        unitOfWork.Verify(work => work.SaveChangesAsync(default), Times.Once());
    }

    private RefreshTokenService CreateService() => new(
        users.Object, accessTokens.Object, tokens.Object, sessions.Object, authorization.Object, unitOfWork.Object);
}
