
namespace FlowERP.Identity.Application.PasswordResetTokens;

public interface IPasswordResetEmailSender
{
    Task SendResetLinkAsync(
        string email,
        string resetLink,
        CancellationToken cancellationToken = default);
}
