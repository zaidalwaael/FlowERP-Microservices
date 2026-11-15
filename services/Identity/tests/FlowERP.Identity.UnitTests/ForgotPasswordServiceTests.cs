using FlowERP.Identity.Application;
using FlowERP.Identity.Application.PasswordResetTokens;
using FlowERP.Identity.Application.Users;
using FlowERP.Identity.Domain.PasswordResetTokens;
using FlowERP.Identity.Domain.Users;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace FlowERP.Identity.UnitTests;

public sealed class ForgotPasswordServiceTests
{
    private readonly Mock<IUserRepository> users = new();
    private readonly Mock<IPasswordResetTokenRepository> tokens = new();
    private readonly Mock<IPasswordResetTokenGenerator> tokenGenerator = new();
    private readonly Mock<IPasswordResetEmailSender> emailSender = new();
    private readonly Mock<IUnitOfWork> unitOfWork = new();

    // Tell xUnit to discover this method as a test.
    [Fact]
    public async Task RequestResetAsync_WhenUserDoesNotExist_DoesNotSendEmail()
    {
        const string email = "missing@flowerp.com";
        users.Setup(repository => repository.GetByEmailAsync(email, default))
            .ReturnsAsync((User?)null);

        await CreateService().RequestResetAsync(email);

        tokens.VerifyNoOtherCalls();
        emailSender.VerifyNoOtherCalls();
    }

    // Tell xUnit to discover this method as a test.
    [Fact]
    public async Task RequestResetAsync_WhenUserExists_StoresHashAndSendsEncodedLink()
    {
        var user = new User(Guid.NewGuid(), "user@flowerp.com", "password-hash");
        var expiration = DateTime.UtcNow.AddMinutes(30);
        users.Setup(repository => repository.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        tokenGenerator.Setup(generator => generator.Generate()).Returns("reset+token");
        tokenGenerator.Setup(generator => generator.Hash("reset+token")).Returns("reset-hash");
        tokenGenerator.Setup(generator => generator.GetExpirationDate()).Returns(expiration);

        await CreateService().RequestResetAsync(user.Email);

        // Verify that only the hash is stored and that the email contains the encoded raw token.
        tokens.Verify(repository => repository.AddAsync(
            It.Is<PasswordResetToken>(token => token.UserId == user.Id &&
                token.TokenHash == "reset-hash" && token.ExpiresAtUtc == expiration),
            default), Times.Once());
        unitOfWork.Verify(work => work.SaveChangesAsync(default), Times.Once());
        emailSender.Verify(sender => sender.SendResetLinkAsync(
            user.Email, "https://example.test/reset-password?token=reset%2Btoken", default), Times.Once());
    }

    private ForgotPasswordService CreateService() => new(
        users.Object,
        tokens.Object,
        tokenGenerator.Object,
        emailSender.Object,
        Options.Create(new PasswordResetOptions
        {
            EmailEnabled = true,
            ResetPasswordUrl = "https://example.test/reset-password"
        }),
        unitOfWork.Object);
}
