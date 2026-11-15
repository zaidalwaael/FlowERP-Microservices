using FlowERP.Identity.Application;
using FlowERP.Identity.Application.Authentication;
using FlowERP.Identity.Application.Authorization;
using FlowERP.Identity.Application.Users;
using FlowERP.Identity.Domain.RefreshTokens;
using FlowERP.Identity.Domain.Users;
using Moq;
using Xunit;

namespace FlowERP.Identity.UnitTests;

public sealed class LoginServiceTests
{
    private readonly Mock<IUserRepository> users = new();
    private readonly Mock<IPasswordHasher> passwordHasher = new();
    private readonly Mock<ITokenGenerator> accessTokens = new();
    private readonly Mock<IRefreshTokenGenerator> refreshTokens = new();
    private readonly Mock<IRefreshTokenRepository> sessions = new();
    private readonly Mock<IUserAuthorizationRepository> authorization = new();
    private readonly Mock<IUnitOfWork> unitOfWork = new();

    // Tell xUnit to discover this method as a test.
    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ThrowsInvalidCredentials()
    {
        // Arrange: Simulate a repository lookup for a missing user.
        const string email = "missing@flowerp.com";
        const string password = "ExamplePassword!123";
        users.Setup(repository => repository.GetByEmailAsync(email, default))
            .ReturnsAsync((User?)null);

        // Act and assert: Verify that login rejects the missing user.
        var service = CreateService();
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            service.LoginAsync(email, password));

        // Verify that the service looked up the specified email exactly once.
        users.Verify(repository => repository.GetByEmailAsync(email, default), Times.Once());
    }

    // Tell xUnit to discover this method as a test.
    [Fact]
    public async Task LoginAsync_WhenEmailIsEmpty_ThrowsWithoutLookingUpUser()
    {
        // Act and assert: Verify that login rejects an empty email.
        var service = CreateService();
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            service.LoginAsync(string.Empty, "ExamplePassword!123"));

        // Verify that the service rejected the input before looking up a user.
        users.Verify(repository => repository.GetByEmailAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    // Tell xUnit to discover this method as a test.
    [Fact]
    public async Task LoginAsync_WhenPasswordIsIncorrect_ThrowsInvalidCredentials()
    {
        // Arrange: Create an active user with a stored password hash.
        const string password = "WrongPassword!123";
        var user = new User(Guid.NewGuid(), "user@flowerp.com", "stored-password-hash");
        users.Setup(repository => repository.GetByEmailAsync(user.Email, default))
            .ReturnsAsync(user);

        // Simulate a mismatch between the stored hash and the supplied password.
        passwordHasher.Setup(hasher => hasher.Verify(user.PasswordHash, password))
            .Returns(false);

        // Act and assert: Verify that login rejects an incorrect password.
        var service = CreateService();
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            service.LoginAsync(user.Email, password));

        // Verify that the service checked the supplied password exactly once.
        passwordHasher.Verify(hasher => hasher.Verify(user.PasswordHash, password), Times.Once());
        sessions.VerifyNoOtherCalls();
    }

    // Tell xUnit to discover this method as a test.
    [Fact]
    public async Task LoginAsync_WhenCredentialsAreValid_ReturnsTokensAndStoresOnlyRefreshHash()
    {
        const string password = "ExamplePassword!123";
        const string refreshTokenHash = "refresh-token-hash";
        var user = new User(Guid.NewGuid(), "user@flowerp.com", "stored-password-hash");
        string[] roles = ["Admin"];
        string[] permissions = ["users.read"];
        var expiration = DateTime.UtcNow.AddDays(7);

        // Supply the authorization data and distinguish the raw token from its stored hash.
        users.Setup(repository => repository.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        passwordHasher.Setup(hasher => hasher.Verify(user.PasswordHash, password)).Returns(true);
        authorization.Setup(repository => repository.GetRolesAsync(user.Id, default)).ReturnsAsync(roles);
        authorization.Setup(repository => repository.GetPermissionsAsync(user.Id, default)).ReturnsAsync(permissions);
        accessTokens.Setup(generator => generator.GenerateAccessToken(user.Id, user.Email, roles, permissions))
            .Returns("access-token");
        refreshTokens.Setup(generator => generator.Generate()).Returns("raw-refresh-token");
        refreshTokens.Setup(generator => generator.Hash("raw-refresh-token")).Returns(refreshTokenHash);
        refreshTokens.Setup(generator => generator.GetExpirationDate()).Returns(expiration);

        var result = await CreateService().LoginAsync(user.Email, password);

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("raw-refresh-token", result.RefreshToken);

        // Confirm that persistence receives the hash and the correct user and expiration.
        sessions.Verify(repository => repository.AddAsync(
            It.Is<RefreshToken>(token => token.UserId == user.Id &&
                token.TokenHash == refreshTokenHash && token.ExpiresAtUtc == expiration),
            default), Times.Once());
        unitOfWork.Verify(work => work.SaveChangesAsync(default), Times.Once());
    }

    // Each test gets fresh mocks and constructs the same application service.
    private LoginService CreateService() => new(
        users.Object,
        passwordHasher.Object,
        accessTokens.Object,
        refreshTokens.Object,
        sessions.Object,
        authorization.Object,
        unitOfWork.Object);
}
