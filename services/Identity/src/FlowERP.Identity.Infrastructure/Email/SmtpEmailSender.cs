
using FlowERP.Identity.Application.PasswordResetTokens;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FlowERP.Identity.Infrastructure.Email;

public sealed class SmtpEmailSender(
    IOptions<SmtpOptions> options)
    : IPasswordResetEmailSender
{
    public async Task SendResetLinkAsync(
        string email,
        string resetLink,
        CancellationToken cancellationToken = default)
    {
        var settings = options.Value;

        // Create the email message.
        var message = new MimeMessage();

        message.From.Add(
            new MailboxAddress(
                settings.FromName,
                settings.FromEmail));

        message.To.Add(
            MailboxAddress.Parse(email));

        message.Subject = "FlowERP - Reset Your Password";

        // Send as plain text so the link is not interpreted as HTML.
        message.Body = new TextPart("plain")
        {
            Text =
                "We received a request to reset your password.\n\n" +
                $"Reset your password using this link:\n{resetLink}\n\n" +
                "This link expires in 30 minutes.\n\n" +
                "If you did not request this, ignore this email."
        };

        using var client = new SmtpClient();

        // Connect securely to the SMTP server.
        await client.ConnectAsync(
            settings.Host,
            settings.Port,
            SecureSocketOptions.StartTls,
            cancellationToken);

        // Authenticate with the SMTP server.
        await client.AuthenticateAsync(
            settings.Username,
            settings.Password,
            cancellationToken);

        // Send the email.
        await client.SendAsync(
            message,
            cancellationToken);

        // Disconnect gracefully.
        await client.DisconnectAsync(
            true,
            cancellationToken);
    }
}
