
using FlowERP.Identity.Application.Users;
using FlowERP.Identity.Domain.PasswordResetTokens;
using Microsoft.Extensions.Options;

namespace FlowERP.Identity.Application.PasswordResetTokens;

public sealed class ForgotPasswordService(
    IUserRepository userRepository,
    IPasswordResetTokenRepository tokenRepository,
    IPasswordResetTokenGenerator tokenGenerator,
    IPasswordResetEmailSender emailSender,
    IOptions<PasswordResetOptions> options,
    IUnitOfWork unitOfWork)
{
    public async Task RequestResetAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(
                "Email is required.",
                nameof(email));

        // Email delivery is disabled during local development.
        if (!options.Value.EmailEnabled)
            return;

        var user = await userRepository.GetByEmailAsync(
            email,
            cancellationToken);

        // Do not reveal whether the account exists or is active.
        if (user is null || !user.IsActive)
            return;

        var resetUrl = options.Value.ResetPasswordUrl;

        if (!Uri.TryCreate(
                resetUrl,
                UriKind.Absolute,
                out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException(
                "A valid HTTPS password reset URL is required.");
        }

        // Generate a secure password reset token.
        var rawToken = tokenGenerator.Generate();

        // Store only the token hash.
        var tokenHash = tokenGenerator.Hash(rawToken);

        var resetToken = new PasswordResetToken(
            Guid.NewGuid(),
            user.Id,
            tokenHash,
            tokenGenerator.GetExpirationDate());

        await tokenRepository.AddAsync(
            resetToken,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        // Append the token to the trusted frontend URL.
        var separator = resetUrl.Contains('?') ? "&" : "?";

        var resetLink =
            $"{resetUrl}{separator}token={Uri.EscapeDataString(rawToken)}";

        // Send the reset link to the user's email.
        await emailSender.SendResetLinkAsync(
            user.Email,
            resetLink,
            cancellationToken);
    }
}
