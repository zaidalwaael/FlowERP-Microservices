using FlowERP.Identity.Application;
using FlowERP.Identity.Application.Authentication;
using FlowERP.Identity.Application.Authorization;
using FlowERP.Identity.Application.Users;
using FlowERP.Identity.Domain.Users;
using Moq;
using Xunit;

namespace FlowERP.Identity.UnitTests;

public sealed class LoginServiceTests
{
    // Tell xUnit to discover this method as a test.
    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ThrowsInvalidCredentials()
    {
        // Arrange: Simulate a repository lookup for a missing user.
        var email = "missing@flowerp.com";
        var password = "ExamplePassword!123";
        var userRepository = new Mock<IUserRepository>();

        userRepository
            .Setup(repository => repository.GetByEmailAsync(
                email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Create the real login service with mock dependencies.
        var loginService = new LoginService(
            userRepository.Object,
            Mock.Of<IPasswordHasher>(),
            Mock.Of<ITokenGenerator>(),
            Mock.Of<IRefreshTokenGenerator>(),
            Mock.Of<IRefreshTokenRepository>(),
            Mock.Of<IUserAuthorizationRepository>(),
            Mock.Of<IUnitOfWork>());

        // Act and assert: Verify that login rejects the missing user.
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            loginService.LoginAsync(email, password));

        // Verify that the service looked up the specified email exactly once.
        userRepository.Verify(
            repository => repository.GetByEmailAsync(
                email,
                It.IsAny<CancellationToken>()),
            Times.Once());
    }

    // Tell xUnit to discover this method as a test.
    [Fact]
    public async Task LoginAsync_WhenEmailIsEmpty_ThrowsWithoutLookingUpUser()
    {
        // Arrange: Create the login service with a mock user repository.
        var userRepository = new Mock<IUserRepository>();

        var loginService = new LoginService(
            userRepository.Object,
            Mock.Of<IPasswordHasher>(),
            Mock.Of<ITokenGenerator>(),
            Mock.Of<IRefreshTokenGenerator>(),
            Mock.Of<IRefreshTokenRepository>(),
            Mock.Of<IUserAuthorizationRepository>(),
            Mock.Of<IUnitOfWork>());

        // Act and assert: Verify that login rejects an empty email.
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            loginService.LoginAsync(string.Empty, "ExamplePassword!123"));

        // Verify that the service rejected the input before looking up a user.
        userRepository.Verify(
            repository => repository.GetByEmailAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never());
    }

    // Tell xUnit to discover this method as a test.
    [Fact]
    public async Task LoginAsync_WhenPasswordIsIncorrect_ThrowsInvalidCredentials()
    {
        // Arrange: Create an active user with a stored password hash.
        var email = "user@flowerp.com";
        var password = "WrongPassword!123";
        var user = new User(Guid.NewGuid(), email, "stored-password-hash");

        // Return the existing user so login reaches password verification.
        var userRepository = new Mock<IUserRepository>();

        userRepository
            .Setup(repository => repository.GetByEmailAsync(
                email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Simulate a mismatch between the stored hash and the supplied password.
        var passwordHasher = new Mock<IPasswordHasher>();

        passwordHasher
            .Setup(hasher => hasher.Verify(user.PasswordHash, password))
            .Returns(false);

        // Create the login service with the configured repository and hasher.
        var loginService = new LoginService(
            userRepository.Object,
            passwordHasher.Object,
            Mock.Of<ITokenGenerator>(),
            Mock.Of<IRefreshTokenGenerator>(),
            Mock.Of<IRefreshTokenRepository>(),
            Mock.Of<IUserAuthorizationRepository>(),
            Mock.Of<IUnitOfWork>());

        // Act and assert: Verify that login rejects an incorrect password.
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            loginService.LoginAsync(email, password));

        // Verify that the service checked the supplied password exactly once.
        passwordHasher.Verify(
            hasher => hasher.Verify(user.PasswordHash, password),
            Times.Once());
    }
}