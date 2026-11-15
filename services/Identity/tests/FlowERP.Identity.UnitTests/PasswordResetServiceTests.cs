using FlowERP.Identity.Application;
using FlowERP.Identity.Application.Authentication;
using FlowERP.Identity.Application.PasswordResetTokens;
using FlowERP.Identity.Application.Users;
using FlowERP.Identity.Domain.PasswordResetTokens;
using FlowERP.Identity.Domain.RefreshTokens;
using FlowERP.Identity.Domain.Users;
using Moq;
using Xunit;

namespace FlowERP.Identity.UnitTests;

public sealed class PasswordResetServiceTests
{
    private readonly Mock<IPasswordResetTokenRepository> resetTokens = new();
    private readonly Mock<IPasswordResetTokenGenerator> tokenGenerator = new();
    private readonly Mock<IUserRepository> users = new();
    private readonly Mock<IPasswordHasher> passwordHasher = new();
    private readonly Mock<IRefreshTokenRepository> sessions = new();
    private readonly Mock<IUnitOfWork> unitOfWork = new();

    // Tell xUnit to discover this method as a test.
    [Fact]
    public async Task ResetAsync_WhenTokenWasUsed_RejectsItWithoutChangingThePassword()
    {
        var token = new PasswordResetToken(Guid.NewGuid(), Guid.NewGuid(), "reset-hash", DateTime.UtcNow.AddMinutes(30));
        token.MarkAsUsed();
        tokenGenerator.Setup(generator => generator.Hash("reset-token")).Returns(token.TokenHash);
        resetTokens.Setup(repository => repository.GetByTokenHashAsync(token.TokenHash, default))
            .ReturnsAsync(token);

        await Assert.ThrowsAsync<InvalidPasswordResetTokenException>(() =>
            CreateService().ResetAsync("reset-token", "NewPassword!123"));

        passwordHasher.VerifyNoOtherCalls();
        unitOfWork.VerifyNoOtherCalls();
    }

    // Tell xUnit to discover this method as a test.
    [Fact]
    public async Task ResetAsync_WhenTokenIsValid_ChangesPasswordAndInvalidatesExistingTokens()
    {
        const string newPassword = "NewPassword!123";
        var user = new User(Guid.NewGuid(), "user@flowerp.com", "old-password-hash");
        var expiration = DateTime.UtcNow.AddMinutes(30);
        var token = new PasswordResetToken(Guid.NewGuid(), user.Id, "reset-hash", expiration);
        var otherToken = new PasswordResetToken(Guid.NewGuid(), user.Id, "other-reset-hash", expiration);
        var session = new RefreshToken(Guid.NewGuid(), user.Id, "session-hash", DateTime.UtcNow.AddDays(1));

        // Simulate a tracked user, another pending reset link and an active session.
        tokenGenerator.Setup(generator => generator.Hash("reset-token")).Returns(token.TokenHash);
        resetTokens.Setup(repository => repository.GetByTokenHashAsync(token.TokenHash, default))
            .ReturnsAsync(token);
        resetTokens.Setup(repository => repository.GetActiveByUserIdAsync(user.Id, default))
            .ReturnsAsync(new[] { token, otherToken });
        users.Setup(repository => repository.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        passwordHasher.Setup(hasher => hasher.Hash(newPassword)).Returns("new-password-hash");
        sessions.Setup(repository => repository.GetActiveByUserIdAsync(user.Id, default))
            .ReturnsAsync(new[] { session });

        await CreateService().ResetAsync("reset-token", newPassword);

        // Verify that the password and the affected tokens are updated before saving.
        Assert.Equal("new-password-hash", user.PasswordHash);
        Assert.NotNull(token.UsedAtUtc);
        Assert.NotNull(otherToken.UsedAtUtc);
        Assert.NotNull(session.RevokedAtUtc);
        unitOfWork.Verify(work => work.SaveChangesAsync(default), Times.Once());
    }

    private ResetPasswordService CreateService() => new(
        resetTokens.Object, tokenGenerator.Object, users.Object, passwordHasher.Object, sessions.Object, unitOfWork.Object);
}
